namespace Pong.Network {
    using UnityEngine;
    using System.Net;
    public class NetworkController : MonoBehaviour {
        public int portBroadcast = 1500;
        public int portGame = 1555;
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
            udpHandler = new Pong.Network.UdpHandler(portGame);
            udpHandler.StartListening();
            
        }

        private void ConnectToServer(string ip) {
            Debug.LogError("Connecting to server: " + ip);
            lanBc.StopBroadcasting();
            udpHandler = new Pong.Network.UdpHandler(portGame);
            udpHandler.StartListening();
            udpHandler.SendMessage(new Pong.Network.UdpMessage(new IPEndPoint(IPAddress.Parse(ip), portGame), new Pong.Network.CommandConnect()));
        }

    }
}