namespace Pong.Network {
    using UnityEngine;

    public class NetworkController : MonoBehaviour {
        LanBroadcaster lanBc;

        void Start() {
            lanBc = GetComponent<LanBroadcaster>();
            lanBc.StartSearchBroadCasting(ConnectToServer, StartServer, 22043);
        }

        private void StartServer() {
            Debug.LogError("Starting Server");
            lanBc.StopBroadCasting();
            lanBc.StartAnnounceBroadCasting();
        }

        private void ConnectToServer(string ip) {
            Debug.LogError("Connecting to server: " + ip);
        }

    }
}