namespace Pong {
    using UnityEngine;

    public enum PaddleMoveDir {
        None = 0,
        Up = 1,
        Down = -1
    }

    public enum PaddleColors {
        Undefined = 0,
        Red,
        Blue
    }

    public class Paddle : MonoBehaviour {
        private RectTransform tr;
        public float curVelocity = 0f;
        public PaddleMoveDir curDir = PaddleMoveDir.None;
        public PaddleColors color = PaddleColors.Undefined;

        public float YPos { get { return tr.anchoredPosition.y; } }
        public float Xpos { get { return tr.anchoredPosition.x; } }
        public bool IsLeftSided { get { return tr.pivot.x == 0; } }
        public float HalfSize { get { return tr.sizeDelta.y / 2; } }

        private void Awake() {
            tr = GetComponent<RectTransform>();
        }

        private void Update() {
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
            AcceleratePaddle();
            MovePaddle();
        }

        private void AcceleratePaddle() {
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

        private void MovePaddle() {
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
            SetBatMovement(dir);
        }

        private void SetPosition(float yPos) {
            if(yPos > GameController.Instance.botBorder + HalfSize && yPos < GameController.Instance.topBorder - HalfSize) {
                tr.anchoredPosition = new Vector2(tr.anchoredPosition.x, yPos);
            }
        }

        public void SetBatMovement(PaddleMoveDir dir) {
            if(dir != curDir) {
                curVelocity = 0f;
            }
            curDir = dir;
        }
    }
}