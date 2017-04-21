namespace Pong {
    using Pong.Network;
    using System;
    using UnityEngine;

    public class Paddle : MonoBehaviour {
        private RectTransform tr;
        protected int id;
        protected bool isControllable;
        public float curVelocity = 0f;
        public PaddleMoveDir curDir = PaddleMoveDir.None;
        public PaddleColors color = PaddleColors.Undefined;

        public int Id { get { return id; } }
        public float YPos { get { InitRectTransform(); return tr.anchoredPosition.y; } }
        public float Xpos { get { InitRectTransform();  return tr.anchoredPosition.x; } }
        public bool IsLeftSided { get { InitRectTransform(); return tr.pivot.x == 0; } }
        public float HalfSize { get { InitRectTransform(); return tr.sizeDelta.y / 2; } }
        public bool IsControllable { get { return isControllable; } }
        
        private void InitRectTransform() {
            if(tr == null)
                tr = GetComponent<RectTransform>();
        }

        protected virtual void Start() { }

        protected virtual void Update() {
            if(IsControllable) {
                switch(color) {
                    case PaddleColors.Red:
                        ControlByKeycodes(new KeyCode[] { KeyCode.W, KeyCode.S });
                        break;
                    case PaddleColors.Blue:
                        ControlByKeycodes(new KeyCode[] { KeyCode.UpArrow, KeyCode.DownArrow });
                        break;
                    default:
                        Debug.LogError("Paddle color is undefined!");
                        break;
                }
            }
            AcceleratePaddle();
            MovePaddle();
        }

        public void InitializePaddle(int id, PaddleColors color, bool isLeftSide, bool isControllable) {
            this.id = id;
            this.color = color;
            InitRectTransform();
            tr.pivot = new Vector2(isLeftSide ? 0f : 1f, 0.5f);
            this.isControllable = isControllable;
        }

        protected void AcceleratePaddle() {
            float paddleMod = GameController.Ball.speedModel.GetPaddleMod(GameController.Ball.hitCounter);
            if(curVelocity == 0f) {
                curVelocity = GameController.Instance.batBasicVelocity * paddleMod;
            } else {
                curVelocity += GameController.Instance.batAcceleration * paddleMod * Time.deltaTime;
                if(curVelocity > GameController.Instance.batMaxVelocity * paddleMod) {
                    curVelocity = GameController.Instance.batMaxVelocity * paddleMod;
                }
            }
        }

        protected void MovePaddle() {
            if(curDir == PaddleMoveDir.Up) {
                SetPosition(YPos + curVelocity * Time.deltaTime);
            } else if(curDir == PaddleMoveDir.Down) {
                SetPosition(YPos - curVelocity * Time.deltaTime);
            }
        }

        private void ControlByKeycodes(KeyCode[] codes) {
            PaddleMoveDir dir = PaddleMoveDir.None;
            if(Input.GetKey(codes[0])) {
                dir = PaddleMoveDir.Up;
            } else if(Input.GetKey(codes[1])) {
                dir = PaddleMoveDir.Down;
            }
            if(curDir != dir) {
                NetworkController.Instance.SendUdpCommand(new CommandPaddleMove(Id, dir));
                SetPaddleMovement(dir);
            }
        }

        private void SetPosition(float yPos) {
            if(yPos > GameController.Instance.BotBorder + HalfSize && yPos < GameController.Instance.TopBorder - HalfSize) {
                tr.anchoredPosition = new Vector2(tr.anchoredPosition.x, yPos);
            }
        }

        public void SetPaddleMovement(PaddleMoveDir dir) {
            if(dir != curDir) {
                curVelocity = 0f;
            }
            curDir = dir;
        }
    }

    public enum PaddleMoveDir : Int32 {
        None = 0,
        Up = 1,
        Down = -1
    }

    public enum PaddleColors : Int32 {
        Undefined = 0,
        Red,
        Blue
    }

}