﻿namespace Pong {
    using Pong.Network;
    using UnityEngine;

    public class Ball : MonoBehaviour {
        protected RectTransform tr;
        protected Vector2 dir;
        protected float velocity;
        protected Paddle spawnPaddle = null;
        public BallModel speedModel;

        public int hitCounter;

        public float Velocity { get { return velocity; } }
        public float Radius { get { return tr.sizeDelta.y / 2; } }
        public Vector2 Pos { get { return tr.anchoredPosition; } }
        public Vector2 Dir { get { return dir; } }

        void Awake() {
            tr = GetComponent<RectTransform>();
        }

        public void Spawn(Paddle paddle) {
            this.spawnPaddle = paddle;
        }

        private RaycastHit2D[] hits = new RaycastHit2D[3];
        void Update() {
            if(spawnPaddle != null) {
                if(spawnPaddle.IsLeftSided) {
                    tr.anchoredPosition = new Vector2(GameController.Instance.leftBorder + Radius * 2, spawnPaddle.YPos);
                } else {
                    tr.anchoredPosition = new Vector2(GameController.Instance.rightBorder - Radius * 2, spawnPaddle.YPos);
                }

                if(Input.GetKeyDown(KeyCode.Space)) {
                    float angle = 45f * Mathf.Deg2Rad;//angle in range XX from sides to center of paddle
                    bool xSign = spawnPaddle.IsLeftSided;
                    bool ySign = Velocity > 0f;
                    dir.x = Mathf.Cos(angle) * (xSign ? (1) : (-1));
                    dir.y = Mathf.Sin(angle) * (ySign ? (1) : (-1));
                    Launch(dir);
                    spawnPaddle = null;
                    NetworkController.Instance.SendUdpCommand(new CommandBallLaunch(dir));
                }
                return;
            }
            if(NetworkController.Instance.Role == NetworkController.PlayerRole.Server && dir != Vector2.zero) {
                var oldPos = tr.position;
                var pos = tr.anchoredPosition;
                var dist = velocity * Time.deltaTime;
                pos += dir * dist;
                tr.anchoredPosition = pos;
                var vec = tr.position - oldPos;
                if(Physics2D.CircleCastNonAlloc(oldPos, Radius, vec.normalized, hits, vec.magnitude) > 0) {
                if(hits[0].transform.tag == "Paddle") {
                    var paddle = hits[0].transform.GetComponent<Paddle>();
                    Bounce(paddle);
                }
                } else if(!Bounce()) {
                    CheckForGoal();
                }
                NetworkController.Instance.SendUdpCommand(new CommandBallUpdate(dir, pos));
            }
        }

        public void SetBallPosition(Vector2 pos, Vector2 dir) {
            this.dir = dir;
            this.tr.anchoredPosition = pos;
        }

        private void CheckForGoal() {
            if(tr.anchoredPosition.x <= GameController.Instance.leftBorder + Radius) {
                GameController.Instance.BallScored(this, GameSides.Left);
            } else if(tr.anchoredPosition.x >= GameController.Instance.rightBorder - Radius) {
                GameController.Instance.BallScored(this, GameSides.Right);
            }
        }

        /// <summary>
        /// Handle interception with walls
        /// </summary>
        private bool Bounce() {
            if(tr.anchoredPosition.y + Radius > GameController.Instance.TopBorder) {
                tr.anchoredPosition = new Vector2(tr.anchoredPosition.x, GameController.Instance.TopBorder - Radius);
                dir.y = -dir.y;
                return true;
            }
            if(tr.anchoredPosition.y - Radius < GameController.Instance.BotBorder) {
                tr.anchoredPosition = new Vector2(tr.anchoredPosition.x, GameController.Instance.BotBorder + Radius);
                dir.y = -dir.y;
                return true;
            }
            return false;
        }

        /// <summary>
        /// ball bounce from Paddle
        /// </summary>
        private void Bounce(Paddle paddle) {
            float diff = Mathf.Max(paddle.YPos, Pos.y) - Mathf.Min(paddle.YPos, Pos.y);
            float angle = (diff / paddle.HalfSize * 40f + 10f) * Mathf.Deg2Rad;//angle in range XX from sides to center of paddle
            bool xSign = paddle.IsLeftSided;
            bool ySign = dir.y > 0f;
            dir.x = Mathf.Cos(angle) * (xSign ? (1) : (-1));
            dir.y = Mathf.Sin(angle) * (ySign ? (1) : (-1));
            ++hitCounter;
            tr.anchoredPosition = new Vector2(Pos.x + (xSign ? (Radius ) : (-Radius )), Pos.y);
            UpdateVelocity();
        }

        private void UpdateVelocity() {
            velocity = speedModel.GetSpeed(hitCounter);
        }

        public void Launch(Vector2 dir) {
            tr.anchoredPosition = Vector2.zero;
            gameObject.SetActive(true);
            this.dir = dir;
            hitCounter = 10;
            UpdateVelocity();
        }
    }
}