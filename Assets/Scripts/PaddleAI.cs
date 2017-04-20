namespace Pong {
    using UnityEngine;

    public class PaddleAI : Paddle {
        public enum Tactic {
            Center,
            Calculate,
            Follow
        }

        public bool isLeftSide = false;
        private Ball ball;
        public Tactic tactic = Tactic.Center;
        private float? yFollowPos = null;
        protected override void Start() {
            base.Start();
            ball = FindObjectOfType<Ball>();
        }

        public override void BallBounced() {
            yFollowPos = null;
        }

        private void UpdateTactic() {
            if((isLeftSide && ball.Dir.x < 0f) || (!isLeftSide && ball.Dir.x > 0f)) {
                if(yFollowPos == null) {
                    yFollowPos = CalculateBallGoalPosY();
                    tactic = Tactic.Follow;
                }
            } else {
                tactic = Tactic.Center;
                yFollowPos = null;
            } 
            
        }

        protected override void Update() {
            //base.Update();
            UpdateTactic();
            if(tactic == Tactic.Center) {
                Follow(0f);
            } else if(tactic == Tactic.Follow) {
                Follow(yFollowPos.Value);
            }
            AcceleratePaddle();
            MovePaddle();
        }

        private void Follow(float yPos) {
            if(YPos >  yPos + 5f) {
                SetPaddleMovement(PaddleMoveDir.Down);
            } else if(YPos < yPos - 5f) {
                SetPaddleMovement(PaddleMoveDir.Up);
            } else {
                SetPaddleMovement(PaddleMoveDir.None);
            }
        }

        private float CalculateBallGoalPosY() {
            var pos = ball.Pos;
            var dir = ball.Dir;
            float shift;
            if(!isLeftSide) {
                while(pos.x < GameController.Instance.rightBorder) {
                    if(dir.y > 0) {
                        shift = Mathf.Abs((GameController.Instance.TopBorder - ball.Radius - pos.y) / dir.y);
                        if((pos + dir * shift).x < GameController.Instance.rightBorder - ball.Radius*2) {
                            pos += dir * shift;
                            dir.y = -dir.y;
                        } else {
                            break;
                        }
                    } else {
                        shift = Mathf.Abs(Mathf.Abs(GameController.Instance.BotBorder + ball.Radius - pos.y) / dir.y);
                        if((pos + dir * shift).x < GameController.Instance.rightBorder - ball.Radius*2) {
                            pos += dir * shift;
                            dir.y = -dir.y;
                        } else {
                            break;
                        }
                    }
                }
                shift = (GameController.Instance.rightBorder - ball.Radius*4 - pos.x) / dir.x;
                pos += dir * shift;
                return pos.y;
            } else {
                while(pos.x > GameController.Instance.leftBorder) {
                    if(dir.y > 0) {
                        shift = Mathf.Abs((GameController.Instance.TopBorder - ball.Radius - pos.y) / dir.y);
                        if((pos + dir * shift).x > GameController.Instance.leftBorder + ball.Radius*2) {
                            pos += dir * shift;
                            dir.y = -dir.y;
                        } else {
                            break;
                        }
                    } else {
                        shift = Mathf.Abs(Mathf.Abs(GameController.Instance.BotBorder + ball.Radius - pos.y) / dir.y);
                        if((pos + dir * shift).x > GameController.Instance.leftBorder + ball.Radius*2) {
                            pos += dir * shift;
                            dir.y = -dir.y;
                        } else {
                            break;
                        }
                    }
                }
                shift = (GameController.Instance.leftBorder + ball.Radius*2 - pos.x) / dir.x;
                pos += dir * shift;
                return pos.y;
            }
        }
    }
}