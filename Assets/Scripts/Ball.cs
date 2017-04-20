namespace Pong {
    using UnityEngine;

    public class Ball : MonoBehaviour {
        protected RectTransform tr;
        protected Vector2 dir;
        protected float velocity;
        public BallModel speedModel;

        public int hitCounter;

        public float Velocity { get { return velocity; } }
        public float HalfRadius { get { return tr.sizeDelta.y / 2; } }

        void Awake() {
            tr = GetComponent<RectTransform>();
            Launch(Vector3.left);
        }
        private RaycastHit2D[] hits = new RaycastHit2D[3];
        void Update() {
            var oldPos = tr.position;
            var pos = tr.anchoredPosition;
            var dist = velocity * Time.deltaTime;
            pos += dir * dist;
            tr.anchoredPosition = pos;
            pos += dir * (dir.x > 0 ? HalfRadius : (-HalfRadius));
            if(Physics2D.LinecastNonAlloc(oldPos, tr.position, hits) > 0) {
                tr.position = hits[0].point;
                if(hits[0].transform.tag == "Paddle") {
                    Bounce(hits[0].transform.GetComponent<Paddle>());
                }
            } else if(Bounce()) {
                
            } else {
                CheckForGoal();
            }
            
        }

        private void CheckForGoal() {
            if(tr.anchoredPosition.x <= GameController.Instance.leftBorder + HalfRadius) {
                GameController.Instance.BallScored(this, GameSides.Left);
            } else if(tr.anchoredPosition.x >= GameController.Instance.rightBorder - HalfRadius) {
                GameController.Instance.BallScored(this, GameSides.Right);
            }
        }

        /// <summary>
        /// Handle interception with walls
        /// </summary>
        private bool Bounce() {
            if(tr.anchoredPosition.y + HalfRadius >= GameController.Instance.topBorder) {
                tr.anchoredPosition = new Vector2(tr.anchoredPosition.x, GameController.Instance.topBorder - HalfRadius);
                dir.y = -dir.y;
                return true;
            }
            if(tr.anchoredPosition.y - HalfRadius <= GameController.Instance.botBorder) {
                tr.anchoredPosition = new Vector2(tr.anchoredPosition.x, GameController.Instance.botBorder + HalfRadius);
                dir.y = -dir.y;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Reflect ball from Paddle
        /// </summary>
        private void Bounce(Paddle paddle) {
            float diff = Mathf.Max(paddle.YPos, tr.anchoredPosition.y) - Mathf.Min(paddle.YPos, tr.anchoredPosition.y);
            float angle = diff / tr.sizeDelta.y / 2 * 70f + 20f;//angle in range [20; 90] from sides to center of paddle
            bool xSign = paddle.IsLeftSided;
            bool ySign = dir.y > 0f;
            dir.x = Mathf.Cos(angle * Mathf.Deg2Rad) * (xSign ? (1) : (-1));
            dir.y = Mathf.Sin(angle * Mathf.Deg2Rad) * (ySign ? (1) : (-1));
            ++hitCounter;
            tr.anchoredPosition = new Vector2(tr.anchoredPosition.x + (xSign ? (1f) : (-1f)), tr.anchoredPosition.y);
            UpdateVelocity();
        }

        private void UpdateVelocity() {
            velocity = speedModel.GetSpeed(hitCounter);
        }

        public void Launch(Vector2 dir) {
            tr.anchoredPosition = Vector2.zero;
            gameObject.SetActive(true);
            this.dir = dir;
            hitCounter = 0;
            UpdateVelocity();
        }
    }
}