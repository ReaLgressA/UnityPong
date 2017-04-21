namespace Pong {
    using System;
    using Pong.Network;
    using UnityEngine;
    using UnityEngine.UI;

    public enum GameSides {
        Left,
        Right
    }

    public class GameController : MonoBehaviour {
        private static GameController instance = null;
        private float topBorder;
        private float botBorder;
        public Canvas gameCanvas;
        public float TopBorder { get { return topBorder; } }
        public float BotBorder { get { return botBorder; } }
        public float leftBorder;
        public float rightBorder;
        public float batBasicVelocity;
        public float batAcceleration;
        public float batMaxVelocity;
        public Text textScoreLeft;
        public Text textScoreRight;
        
        public static GameController Instance { get { return instance; } }
        public static Ball Ball { get { return instance.ball;} }

        public Paddle paddleRed;
        public PaddleAI paddleRedAI;
        public Paddle paddleBlue;
        public PaddleAI paddleBlueAI;
                
        private Ball ball;
        private int scoreLeft = 0;
        private int scoreRight = 0;


        private void Awake() {
            if(instance != null) {
                Debug.LogWarning("GameContoller duplicate attempt prevented");
                Destroy(gameObject);
                return;
            }
            instance = this;
            ball = FindObjectOfType<Ball>();
            topBorder = gameCanvas.GetComponent<RectTransform>().sizeDelta.y / 2 - ball.Radius * 2;
            botBorder = -topBorder;
        }
        
        public void BallScored(Ball ball, GameSides side) {
            ball.gameObject.SetActive(false);
            if(side == GameSides.Left) {
                ++scoreRight;
                if(NetworkController.Instance.Role == NetworkController.PlayerRole.Server) {
                    if(paddleRed.IsLeftSided) {
                        GameController.Ball.Spawn(GameController.Instance.paddleRed);
                        NetworkController.Instance.SendUdpCommand(new CommandBallSpawn(GameController.Instance.paddleRed.Id));
                    } else {
                        GameController.Ball.Spawn(GameController.Instance.paddleBlue);
                        NetworkController.Instance.SendUdpCommand(new CommandBallSpawn(GameController.Instance.paddleBlue.Id));
                    }
                }
            } else {
                ++scoreLeft;
                if(NetworkController.Instance.Role == NetworkController.PlayerRole.Server) {
                    if(!paddleRed.IsLeftSided) {
                        GameController.Ball.Spawn(GameController.Instance.paddleRed);
                        NetworkController.Instance.SendUdpCommand(new CommandBallSpawn(GameController.Instance.paddleRed.Id));
                    } else {
                        GameController.Ball.Spawn(GameController.Instance.paddleBlue);
                        NetworkController.Instance.SendUdpCommand(new CommandBallSpawn(GameController.Instance.paddleBlue.Id));
                    }
                }
            }
            UpdateScore(scoreLeft, scoreRight);
            NetworkController.Instance.SendUdpCommand(new CommandScoreUpdate(scoreLeft, scoreRight));
        }

        public void UpdateScore(int scoreLeft, int scoreRight) {
            this.scoreLeft = scoreLeft;
            this.scoreRight = scoreRight;
            UpdateScore(textScoreLeft, scoreLeft);
            UpdateScore(textScoreRight, scoreRight);
        }

        private void UpdateScore(Text text, int score) {
            text.text = score.ToString();
        }
    }
}