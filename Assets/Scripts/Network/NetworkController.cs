﻿namespace Pong.Network {
    using UnityEngine;
    using System.Net;

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

        private void ConnectionEstablished(CommandConnectEstablished cmd) {
            Debug.Log("Connected to server! SessionID: " + cmd.SessionId);
            udpHandler.SessionId = cmd.SessionId;
        }

        private void PaddleInitialized(CommandPaddleInitialized cmd) {
            if(cmd.Color == PaddleColors.Red) {
                GameController.Instance.paddleRed.InitializePaddle(cmd.Id, cmd.Color, cmd.IsLeftSide, cmd.IsControllable);
            } else {
                GameController.Instance.paddleBlue.InitializePaddle(cmd.Id, cmd.Color, cmd.IsLeftSide, cmd.IsControllable);
            }
        }


        private void ProcessClientMessage(UdpMessage msg) {
            switch(msg.Code) {
                case CommandCode.ConnectEstablished:
                    ConnectionEstablished(msg.cmd as CommandConnectEstablished);
                    break;
                case CommandCode.PaddleInitialized:
                    PaddleInitialized(msg.cmd as CommandPaddleInitialized);
                    break;
            }
        }

        private IPEndPoint clientAddr;
        private void ProcessServerMessage(UdpMessage msg) {
            switch(msg.Code) {
                case CommandCode.Connect:
                    Debug.Log("Incoming connection: " + msg.Remote.Address.ToString());
                    clientAddr = msg.Remote;
                    msg.Respond(new UdpMessage(msg.Remote, new CommandConnectEstablished(udpHandler.SessionId)));
                    GameController.Instance.paddleRed.InitializePaddle(0, PaddleColors.Red, true, true);
                    udpHandler.SendMessage(new UdpMessage(clientAddr, new CommandPaddleInitialized(0, PaddleColors.Red, true, false)));
                    GameController.Instance.paddleBlue.InitializePaddle(1, PaddleColors.Blue, false, false);
                    udpHandler.SendMessage(new UdpMessage(clientAddr, new CommandPaddleInitialized(1, PaddleColors.Blue, false, true)));

                    break;
            }
        }
    }
}