namespace Pong.Network {
    using System.Net;
    using UnityEngine;

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
}