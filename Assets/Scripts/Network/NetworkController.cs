namespace Pong.Network {
    using UnityEngine;

    public class NetworkController : MonoBehaviour {


        void Start() {
            LanBroadcaster lanBc = GetComponent<LanBroadcaster>();
            lanBc.StartSearchBroadCasting(ConnectToServer, StartServer);
        }

        private void StartServer() {
            Debug.LogError("Server should be started");
        }

        private void ConnectToServer(string ip) {
            Debug.LogError("Connecting to server: " + ip);
        }

    }
}