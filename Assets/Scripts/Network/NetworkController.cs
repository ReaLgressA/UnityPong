namespace Pong.Network {
    using UnityEngine;
    using System.Net;
    using System;

    public class NetworkController : MonoBehaviour {
        public enum PlayerRole {
            Server,
            Client
        }

        public int portBroadcast = 1500;
        public int portGameServer = 1555;
        public int portGameClient = 1556;
        protected LanBroadcaster lanBc;
        protected Pong.Network.UdpHandler udpHandler;
        protected PlayerRole role;
        void Start() {
            lanBc = GetComponent<LanBroadcaster>();
            lanBc.StartSearchBroadCasting(ConnectToServer, StartServer, portBroadcast);
        }

        private void StartServer() {
            
            lanBc.StopBroadcasting();
            lanBc.StartAnnounceBroadcasting();
            udpHandler = new Pong.Network.UdpHandler(portGameServer);
            udpHandler.StartListening();
            role = PlayerRole.Server;
            Debug.LogError("Starting Server -> " + udpHandler.SessionId);
        }

        private void ConnectToServer(string ip) {
            Debug.LogError("Connecting to server: " + ip);
            lanBc.StopBroadcasting();
            udpHandler = new Pong.Network.UdpHandler(portGameClient);
            udpHandler.StartListening();
            var endpoint = new IPEndPoint(IPAddress.Parse(ip), portGameServer);
            var command = new Pong.Network.CommandConnect();
            udpHandler.SendMessage(new Pong.Network.UdpMessage(endpoint, command));
            role = PlayerRole.Client;
        }

        private void Update() {
            while(udpHandler != null) {
                UdpMessage? msg = udpHandler.GetMessage();
                if(!msg.HasValue) {
                    break;
                }
                if(role == PlayerRole.Server) {
                    ProcessServerMessage(msg.Value);
                } else {
                    ProcessClientMessage(msg.Value);
                }
            }
        }

        private void ProcessClientMessage(UdpMessage msg) {
            switch(msg.Code) {
                case CommandCode.ConnectEstablished:
                    var cmd = msg.cmd as CommandConnectEstablished;
                    Debug.Log("Connected to server! SessionID: " + cmd.SessionId);
                    
                    break;
            }
        }

        private void ProcessServerMessage(UdpMessage msg) {
            switch(msg.Code) {
                case CommandCode.Connect:
                    Debug.Log("Incoming connection: " + msg.Remote.Address.ToString());
                    msg.Respond(new UdpMessage(msg.Remote, new CommandConnectEstablished(udpHandler.SessionId)));
                    break;
            }
        }



    }
}