namespace Pong.Network {
    using UnityEngine;
    using System.Net;

    public class NetworkController : MonoBehaviour {
        public enum PlayerRole {
            Server,
            Client
        }
        private static NetworkController instance = null;
        public int portBroadcast = 1500;
        public int portGameServer = 1555;
        public int portGameClient = 1556;
        protected LanBroadcaster lanBc;
        protected UdpHandler udpHandler;
        protected PlayerRole role;

        public static NetworkController Instance { get { return instance; } }
        public PlayerRole Role { get { return role; } }

        private IPEndPoint destAddr;

        public void SendUdpCommand(Command cmd) {
            udpHandler.SendMessage(new UdpMessage(destAddr, cmd, udpHandler));
        }

        void Start() {
            if(instance != null) {
                Destroy(gameObject);
                return;
            }
            instance = this;
            lanBc = GetComponent<LanBroadcaster>();
            lanBc.StartSearchBroadCasting(ConnectToServer, StartServer, portBroadcast);
        }

        private void StartServer() {
            
            lanBc.StopBroadcasting();
            lanBc.StartAnnounceBroadcasting();
            udpHandler = new UdpHandler(portGameServer);
            udpHandler.StartListening();
            role = PlayerRole.Server;
            Debug.LogError("Starting Server -> " + udpHandler.SessionId);
        }

        private void ConnectToServer(string ip) {
            Debug.LogError("Connecting to server: " + ip);
            lanBc.StopBroadcasting();
            udpHandler = new UdpHandler(portGameClient);
            udpHandler.StartListening();
            destAddr = new IPEndPoint(IPAddress.Parse(ip), portGameServer);
            var command = new CommandConnect();
            udpHandler.SendMessage(new UdpMessage(destAddr, command));
            role = PlayerRole.Client;
        }

        private void Update() {
            while(udpHandler != null) {
                UdpMessage? msg = udpHandler.GetMessage();
                if(!msg.HasValue) {
                    break;
                }
                if(!ProcessGeneralMessage(msg.Value) && role == PlayerRole.Server) {
                    ProcessServerMessage(msg.Value);
                } else {
                    ProcessClientMessage(msg.Value);
                }
            }
        }

        private void PaddleMoved(CommandPaddleMove cmd) {
            if(GameController.Instance.paddleBlue.Id == cmd.Id) {
                GameController.Instance.paddleBlue.SetPaddleMovement(cmd.Dir);
            } else if(GameController.Instance.paddleRed.Id == cmd.Id) {
                GameController.Instance.paddleRed.SetPaddleMovement(cmd.Dir);
            }
        }

        private void ScoreUpdated(CommandScoreUpdate cmd) {
            GameController.Instance.UpdateScore(cmd.leftScore, cmd.rightScore);
        }

        private bool ProcessGeneralMessage(UdpMessage msg) {
            switch(msg.Code) {
                case CommandCode.PaddleMove:
                    PaddleMoved(msg.cmd as CommandPaddleMove);
                    return true;
                case CommandCode.BallLaunch:
                    BallLaunched(msg.cmd as CommandBallLaunch);
                    return true;
                case CommandCode.ScoreUpdate:
                    ScoreUpdated(msg.cmd as CommandScoreUpdate);
                    return true;
            }
            return false;
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

        private void BallSpawned(CommandBallSpawn cmd) {
            if(cmd.PaddleId == GameController.Instance.paddleRed.Id) {
                GameController.Ball.Spawn(GameController.Instance.paddleRed);
            } else if(cmd.PaddleId == GameController.Instance.paddleBlue.Id) {
                GameController.Ball.Spawn(GameController.Instance.paddleBlue);
            }
        }

        private void BallLaunched(CommandBallLaunch cmd) {
            GameController.Ball.Launch(cmd.Dir, cmd.Pos);
        }

        private void BallUpdated(CommandBallUpdate cmd) {
            GameController.Ball.SetBallPosition(cmd.Pos, cmd.Dir);
        }

        private void ProcessClientMessage(UdpMessage msg) {
            switch(msg.Code) {
                case CommandCode.ConnectEstablished:
                    ConnectionEstablished(msg.cmd as CommandConnectEstablished);
                    break;
                case CommandCode.PaddleInitialized:
                    PaddleInitialized(msg.cmd as CommandPaddleInitialized);
                    break;
                case CommandCode.BallSpawn:
                    BallSpawned(msg.cmd as CommandBallSpawn);
                    break;
                case CommandCode.BallUpdate:
                    BallUpdated(msg.cmd as CommandBallUpdate);
                    break;
            }
        }

        private void ProcessServerMessage(UdpMessage msg) {
            switch(msg.Code) {
                case CommandCode.Connect:
                    Debug.Log("Incoming connection: " + msg.Remote.Address.ToString());
                    destAddr = msg.Remote;
                    msg.Respond(new UdpMessage(msg.Remote, new CommandConnectEstablished(udpHandler.SessionId)));
                    GameController.Instance.paddleRed.InitializePaddle(0, PaddleColors.Red, true, true);
                    udpHandler.SendMessage(new UdpMessage(destAddr, new CommandPaddleInitialized(0, PaddleColors.Red, true, false)));
                    GameController.Instance.paddleBlue.InitializePaddle(1, PaddleColors.Blue, false, false);
                    udpHandler.SendMessage(new UdpMessage(destAddr, new CommandPaddleInitialized(1, PaddleColors.Blue, false, true)));
                    GameController.Ball.Spawn(GameController.Instance.paddleRed);
                    udpHandler.SendMessage(new UdpMessage(destAddr, new CommandBallSpawn(0)));
                    break;
            }
        }
    }
}