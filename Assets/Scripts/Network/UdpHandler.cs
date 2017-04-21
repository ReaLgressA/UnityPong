namespace Pong.Network {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using UnityEngine;

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
                Debug.Log(remote.Address.ToString() + ": " + msg.Code.ToString());
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