namespace Pong {
    using UnityEngine;
    using UnityEngine.UI;

    public enum GameSides {
        Left,
        Right
    }

    public class GameController : MonoBehaviour {
        private static GameController instance = null;
        public float topBorder;
        public float botBorder;
        public float leftBorder;
        public float rightBorder;
        public float batBasicVelocity;
        public float batAcceleration;
        public float batMaxVelocity;
        public Text textScoreLeft;
        public Text textScoreRight;
        
        public Paddle[] bats;

        public static GameController Instance { get { return instance; } }
        public static Ball Ball { get { return instance.ball;} }

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
            InitializeBats();
        }
        
        private void InitializeBats() {
            bats = FindObjectsOfType<Paddle>();
        }

        public void BallScored(Ball ball, GameSides side) {
            ball.gameObject.SetActive(false);
            if(side == GameSides.Left) {
                ++scoreRight;
                UpdateScore(textScoreRight, scoreRight);
            } else {
                ++scoreLeft;
                UpdateScore(textScoreLeft, scoreLeft);
            }
            for(int i = 0; i < bats.Length; ++i) {
                bats[i].BallBounced();
            }
            ball.Launch(new Vector2(-0.9f, 0.2f));
        }

        private void UpdateScore(Text text, int score) {
            text.text = score.ToString();
        }
        
        //void Start() {
        //    udp = new UdpListener(2222);
        //    udp.StartListening();

        //}

        //void Update() {
        //    if(Input.GetKeyDown(KeyCode.Space)) {
        //        IPAddress multicastaddress = IPAddress.Parse("239.0.0.222");
        //        client = new UdpClient();
        //        client.JoinMulticastGroup(multicastaddress);
        //        IPEndPoint remoteep = new IPEndPoint(multicastaddress, 2222);
        //        Byte[] buffer = null;
        //        for(int i = 0; i <= 8000; i++) {
        //            buffer = Encoding.Unicode.GetBytes(i.ToString());
        //            client.Send(buffer, buffer.Length, remoteep);
        //        }
        //        //udp.StopListening();
        //    }
        //}
    }
}