namespace Pong {
    using UnityEngine;

    public class Ball : MonoBehaviour {
        protected RectTransform tr;
        protected Vector2 dir;
        protected float velocity;
        public BallModel speedModel;

        public int hitCounter;

        public float Velocity { get { return velocity; } }
        public float Radius { get { return tr.sizeDelta.y / 2; } }
        public Vector2 Pos { get { return tr.anchoredPosition; } }
        public Vector2 Dir { get { return dir; } }

        void Awake() {
            tr = GetComponent<RectTransform>();
            Launch(new Vector3(-0.9f, 0.1f));
        }

        private RaycastHit2D[] hits = new RaycastHit2D[3];
        void Update() {
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
            } else if(Bounce()) {
                Debug.Log("Wallbounced");
            } else {
                CheckForGoal();
            }
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