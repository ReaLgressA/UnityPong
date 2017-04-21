namespace Pong.Network {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using UnityEngine;

    public class CommandConnect : Command {
        public override CommandCode Code { get { return CommandCode.Connect; } }

        public override byte[] GetBytes(string sessionId = null) {
            return BitConverter.GetBytes((Int32)Code);
        }

        protected override void Parse(byte[] data) { }

        public CommandConnect() { }
    }

    public class CommandConnectEstablished : Command {
        protected string sessionId;
        public override CommandCode Code { get { return CommandCode.PaddleInitialized; } }
        public string SessionId { get { return sessionId; } }

        protected override void Parse(byte[] data) {
            sessionId = BitConverter.ToString(data, 4, 16);
        }

        public override byte[] GetBytes(string sessionId) {
            byte[] bytes = new byte[20];
            Array.Copy(BitConverter.GetBytes((Int32)Code), 0, bytes, 0, 4);
            Array.Copy(Encoding.ASCII.GetBytes(sessionId), 0, bytes, 4, Command.SessionIdLength);
            return bytes;
        }

        public CommandConnectEstablished() { }
        public CommandConnectEstablished(string sessionId) {
            this.sessionId = sessionId;
        }
    }

    //public class CommandPaddleInitialized : Command {
    //    protected Int32 paddleId;
    //    protected PaddleColors paddleColor;
    //    protected bool isLeftSide;
    //    protected bool isControllable;
    //    public override CommandCode Code { get { return CommandCode.PaddleInitialized; } }

    //    public override byte[] GetBytes(string sessionId) {
    //        throw new NotImplementedException();
    //    }

    //    protected override void Parse(byte[] data) {
    //        paddleId = BitConverter.ToInt32(data, 20);
    //    }
    //}

    //public class CommandPaddleMove : Command {
    //    protected int paddleId;
    //    protected PaddleMoveDir dir;
    //    public override CommandCode Code { get { return CommandCode.PaddleMove; } }

    //    public override byte[] GetBytes(string sessionId) {
    //        throw new NotImplementedException();
    //    }

    //    protected override void Parse(byte[] data) {

    //    }
    //}

    //public class CommandBallLaunch : Command {
    //    public override CommandCode Code { get { return CommandCode.BallLaunch; } }

    //    public override byte[] GetBytes(string sessionId) {
    //        throw new NotImplementedException();
    //    }

    //    protected override void Parse(byte[] data) { }
    //}

    //public class CommandScoreUpdate : Command {
    //    public override CommandCode Code { get { return CommandCode.ScoreUpdate; } }

    //    public override byte[] GetBytes(string sessionId) {
    //        throw new NotImplementedException();
    //    }

    //    protected override void Parse(byte[] data) { }
    //}

    //public class CommandGameEnd : Command {
    //    public override CommandCode Code { get { return CommandCode.GameEnd; } }

    //    public override byte[] GetBytes(string sessionId) {
    //        throw new NotImplementedException();
    //    }

    //    protected override void Parse(byte[] data) { }
    //}

    public abstract class Command {
        public const int SessionIdLength = 16;

        protected abstract void Parse(byte[] data);

        public abstract CommandCode Code { get; }

        public static Command Create(byte[] data) {
            if(data.Length < 4) {
                return null;
            }
            CommandCode code = (CommandCode)BitConverter.ToInt32(data, 0);
            var type = Type.GetType(string.Format("Pong.Network.Command{0}", code.ToString()));
            if(type != null) {
                Command cmd = Activator.CreateInstance(type) as Command;
                cmd.Parse(data);
                return cmd;
            } else {
                return null;
            }
        }

        public abstract byte[] GetBytes(String sessionId);
    }
    
    public enum CommandCode : int {
        Undefined = 0,
        Connect = 1,
        ConnectEstablished = 2,
        PaddleInitialized = 3,
        PaddleMove = 4,
        BallLaunch = 5,
        ScoreUpdate = 6,
        GameEnd = 7
    }


    /// <summary>
    /// First 4 bytes of the message - is a command code
    /// If the command code is not CONNECT, next 'Command.SessionIdLength' bytes is client session ID. 
    /// Messages with innaproriate ID will be ignored
    /// </summary>
    public struct UdpMessage {
        private UdpHandler udpHandler;
        private IPEndPoint remote;
        public Command cmd;

        public bool IsCorrupted { get { return cmd == null; } }
        public CommandCode Code { get { return IsCorrupted ? CommandCode.Undefined : ((CommandCode)cmd.Code); } }
        public IPEndPoint Remote { get { return remote; } }

        public UdpMessage(IPEndPoint remote, byte[] data, UdpHandler udpHandler = null) {
            this.remote = remote;
            this.cmd = Command.Create(data);
            this.udpHandler = udpHandler;
        }
        public UdpMessage(IPEndPoint remote, Command cmd, UdpHandler udpHandler = null) {
            this.remote = remote;
            this.cmd = cmd;
            this.udpHandler = udpHandler;
        }

        public void Respond(UdpMessage msg) {
            if(udpHandler == null) {
                Debug.LogError("Can't respond to message. UDP handler is NULL");
                return;
            }
            msg.remote = remote;
            udpHandler.SendMessage(msg);
        }
    }

    public class UdpHandler {
        protected UdpClient udp;
        protected int port;
        protected LinkedList<UdpMessage> messagesIn;
        protected LinkedList<UdpMessage> messagesOut;
        protected string sessionId;

        public int Port { get { return port; } }
        public string SessionId { get { return sessionId; } }


        private static string GenerateSessionId() {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, Command.SessionIdLength).Select(s => s[UnityEngine.Random.Range(0, s.Length)]).ToArray());
        }

        public UdpHandler(int port = 0) {
            this.port = port;
            this.messagesIn = new LinkedList<UdpMessage>();
            this.messagesOut = new LinkedList<UdpMessage>();
        }

        public void StartListening() {
            try {
                this.udp = new UdpClient(port);
                udp.BeginReceive(Recv, null);
                sessionId = GenerateSessionId();
            } catch(Exception ex) {
                Debug.LogError("StartListening failed. Exception: " + ex.ToString());
                StopListening();
            }
        }

        public UdpMessage? GetMessage() {
            if(messagesIn.Count == 0) {
                return null;
            }
            UdpMessage msg = messagesIn.First.Value;
            messagesIn.RemoveFirst();
            return msg;
        }

        private void Recv(IAsyncResult res) {
            try {
                if(udp.Client == null) {
                    return;
                }
                IPEndPoint remote = new IPEndPoint(IPAddress.Any, port);
                byte[] bytes = udp.EndReceive(res, ref remote);
                var msg = new UdpMessage(remote, bytes, this);
                messagesIn.AddLast(msg);
                Debug.Log(remote.Address.ToString() + ": " + Encoding.ASCII.GetString(bytes));
                udp.BeginReceive(Recv, null);
            } catch(Exception ex) {
                Debug.LogError("recv() failed: " + ex.ToString());
                StopListening();
            }
        }

        private void BeginSend(UdpMessage msg) {
            var bytes = msg.cmd.GetBytes(sessionId);
            try {
                udp.BeginSend(bytes, bytes.Length, msg.Remote, Send, null);
            } catch(Exception ex) {
                Debug.LogError("BeginSend() failed: " + ex.ToString());
            }
        }

        private void Send(IAsyncResult res) {
            try {
                if(udp.Client == null) {
                    return;
                }
                udp.EndSend(res);
                messagesOut.RemoveFirst();
                if(messagesOut.Count > 0) {
                    BeginSend(messagesOut.First.Value);
                }
            } catch(Exception ex) {
                Debug.LogError("send() failed: " + ex.ToString());
                StopListening();
            }
        }

        public void SendMessage(UdpMessage msg) {
            messagesOut.AddLast(msg);
            if(messagesOut.Count == 1) {
                BeginSend(msg);
            }
        }

        public void StopListening() {
            try {
                messagesIn.Clear();
                messagesOut.Clear();
                udp.Close();
            } catch(Exception ex) {
                Debug.LogWarning("UDP client Close() failed: " + ex.ToString());
            }
        }
    }
}