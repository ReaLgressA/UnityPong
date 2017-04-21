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
        public override CommandCode Code { get { return CommandCode.ConnectEstablished; } }
        public string SessionId { get { return sessionId; } }

        public override byte[] GetBytes(string sessionId) {
            byte[] bytes = new byte[20];
            Array.Copy(BitConverter.GetBytes((Int32)Code), 0, bytes, 0, 4);
            Array.Copy(Encoding.ASCII.GetBytes(this.sessionId), 0, bytes, 4, Command.SessionIdLength);
            return bytes;
        }

        protected override void Parse(byte[] data) {
            sessionId = Encoding.ASCII.GetString(data.Skip(4).Take(16).ToArray());
        }

        public CommandConnectEstablished() { }
        public CommandConnectEstablished(string sessionId) {
            this.sessionId = sessionId;
        }
    }

    public class CommandPaddleInitialized : Command {
        protected Int32 paddleId;
        protected PaddleColors paddleColor;
        protected bool isLeftSide;
        protected bool isControllable;

        public override CommandCode Code { get { return CommandCode.PaddleInitialized; } }
        public Int32 Id { get { return paddleId; } }
        public PaddleColors Color { get { return paddleColor; } }
        public bool IsLeftSide { get { return isLeftSide; } }
        public bool IsControllable { get { return isControllable; } }

        public override byte[] GetBytes(string sessionId) {
            byte[] bytes = new byte[30];
            Array.Copy(BitConverter.GetBytes((Int32)Code), 0, bytes, 0, 4);
            Array.Copy(Encoding.ASCII.GetBytes(sessionId), 0, bytes, 4, Command.SessionIdLength);
            Array.Copy(BitConverter.GetBytes(paddleId), 0, bytes, 20, sizeof(Int32));
            Array.Copy(BitConverter.GetBytes((Int32)paddleColor), 0, bytes, 24, sizeof(Int32));
            bytes[28] = BitConverter.GetBytes(isLeftSide)[0];
            bytes[29] = BitConverter.GetBytes(isControllable)[0];
            return bytes;
        }

        protected override void Parse(byte[] data) {
            paddleId = BitConverter.ToInt32(data, 20);
            paddleColor = (PaddleColors)BitConverter.ToInt32(data, 24);
            isLeftSide = BitConverter.ToBoolean(data, 28);
            isControllable= BitConverter.ToBoolean(data, 29);
        }

        public CommandPaddleInitialized() { }
        public CommandPaddleInitialized(Int32 paddleId, PaddleColors paddleColor, bool isLeftSide, bool isControllable) {
            this.paddleId = paddleId;
            this.paddleColor = paddleColor;
            this.isLeftSide = isLeftSide;
            this.isControllable = isControllable;
        }
    }

    public class CommandPaddleMove : Command {
        protected Int32 paddleId;
        protected PaddleMoveDir dir;
        public override CommandCode Code { get { return CommandCode.PaddleMove; } }

        public Int32 Id { get { return paddleId; } }
        public PaddleMoveDir Dir { get { return dir; } }

        public override byte[] GetBytes(string sessionId) {
            byte[] bytes = new byte[28];
            Array.Copy(BitConverter.GetBytes((Int32)Code), 0, bytes, 0, 4);
            Array.Copy(Encoding.ASCII.GetBytes(sessionId), 0, bytes, 4, Command.SessionIdLength);
            Array.Copy(BitConverter.GetBytes(paddleId), 0, bytes, 20, sizeof(Int32));
            Array.Copy(BitConverter.GetBytes((Int32)dir), 0, bytes, 24, sizeof(Int32));
            return bytes;
        }

        protected override void Parse(byte[] data) {
            paddleId = BitConverter.ToInt32(data, 20);
            dir = (PaddleMoveDir)BitConverter.ToInt32(data, 24);
        }

        public CommandPaddleMove() { }
        public CommandPaddleMove(Int32 paddleId, PaddleMoveDir dir) {
            this.paddleId = paddleId;
            this.dir = dir;
        }
    }

    public class CommandBallSpawn : Command {
        public override CommandCode Code { get { return CommandCode.BallSpawn; } }
        protected Int32 paddleId;

        public Int32 PaddleId { get { return paddleId; } }

        public override byte[] GetBytes(string sessionId) {
            byte[] bytes = new byte[24];
            Array.Copy(BitConverter.GetBytes((Int32)Code), 0, bytes, 0, 4);
            Array.Copy(Encoding.ASCII.GetBytes(sessionId), 0, bytes, 4, Command.SessionIdLength);
            Array.Copy(BitConverter.GetBytes(paddleId), 0, bytes, 20, sizeof(Int32));
            return bytes;
        }

        protected override void Parse(byte[] data) {
            paddleId = BitConverter.ToInt32(data, 20);
        }

        public CommandBallSpawn() { }
        public CommandBallSpawn(Int32 paddleId) {
            this.paddleId = paddleId;
        }
    }

    public class CommandBallLaunch : Command {
        public override CommandCode Code { get { return CommandCode.BallLaunch; } }
        protected float dirX;
        protected float dirY;

        public Vector2 Dir { get { return new Vector2(dirX, dirY); } }

        public override byte[] GetBytes(string sessionId) {
            byte[] bytes = new byte[28];
            Array.Copy(BitConverter.GetBytes((Int32)Code), 0, bytes, 0, 4);
            Array.Copy(Encoding.ASCII.GetBytes(sessionId), 0, bytes, 4, Command.SessionIdLength);
            Array.Copy(BitConverter.GetBytes(dirX), 0, bytes, 20, sizeof(float));
            Array.Copy(BitConverter.GetBytes(dirY), 0, bytes, 24, sizeof(float));
            return bytes;
        }

        protected override void Parse(byte[] data) {
            dirX = BitConverter.ToSingle(data, 20);
            dirY = BitConverter.ToSingle(data, 24);
        }

        public CommandBallLaunch() { }
        public CommandBallLaunch(Vector2 dir) {
            this.dirX = dir.x;
            this.dirY = dir.y;
        }
    }

    public class CommandBallUpdate : Command {
        public override CommandCode Code { get { return CommandCode.BallUpdate; } }
        protected float dirX;
        protected float dirY;
        protected float posX;
        protected float posY;

        public Vector2 Dir { get { return new Vector2(dirX, dirY); } }
        public Vector2 Pos { get { return new Vector2(posX, posY); } }

        public override byte[] GetBytes(string sessionId) {
            byte[] bytes = new byte[36];
            Array.Copy(BitConverter.GetBytes((Int32)Code), 0, bytes, 0, 4);
            Array.Copy(Encoding.ASCII.GetBytes(sessionId), 0, bytes, 4, Command.SessionIdLength);
            Array.Copy(BitConverter.GetBytes(dirX), 0, bytes, 20, sizeof(float));
            Array.Copy(BitConverter.GetBytes(dirY), 0, bytes, 24, sizeof(float));
            Array.Copy(BitConverter.GetBytes(posX), 0, bytes, 20, sizeof(float));
            Array.Copy(BitConverter.GetBytes(posY), 0, bytes, 24, sizeof(float));
            return bytes;
        }

        protected override void Parse(byte[] data) {
            dirX = BitConverter.ToSingle(data, 20);
            dirY = BitConverter.ToSingle(data, 24);
            posX = BitConverter.ToSingle(data, 28);
            posY = BitConverter.ToSingle(data, 32);
        }

        public CommandBallUpdate() { }
        public CommandBallUpdate(Vector2 dir, Vector2 pos) {
            this.dirX = dir.x;
            this.dirY = dir.y;
            this.posX = pos.x;
            this.posY = pos.y;
        }
    }

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
        BallSpawn = 5,
        BallLaunch = 6,
        BallUpdate = 7,
        ScoreUpdate = 8,
        GameEnd = 9
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
        public string SessionId { get { return sessionId; } set { sessionId = value; } }


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