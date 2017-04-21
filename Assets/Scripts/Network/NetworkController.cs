namespace Pong.Network {
    using UnityEngine;
    using System.Net;
    public class NetworkController : MonoBehaviour {
        public int portBroadcast = 1500;
        public int portGameServer = 1555;
        public int portGameClient = 1556;
        protected LanBroadcaster lanBc;
        protected Pong.Network.UdpHandler udpHandler;
        
        void Start() {
            lanBc = GetComponent<LanBroadcaster>();
            lanBc.StartSearchBroadCasting(ConnectToServer, StartServer, portBroadcast);
        }

        private void StartServer() {
            Debug.LogError("Starting Server");
            lanBc.StopBroadcasting();
            lanBc.StartAnnounceBroadcasting();
            udpHandler = new Pong.Network.UdpHandler(portGameServer);
            udpHandler.StartListening();
            
        }

        private void ConnectToServer(string ip) {
            Debug.LogError("Connecting to server: " + ip);
            lanBc.StopBroadcasting();
            udpHandler = new Pong.Network.UdpHandler(portGameClient);
            udpHandler.StartListening();
            var endpoint = new IPEndPoint(IPAddress.Parse(ip), portGameServer);
            var command = new Pong.Network.CommandConnect();
            udpHandler.SendMessage(new Pong.Network.UdpMessage(endpoint, command));
        }

    }
}