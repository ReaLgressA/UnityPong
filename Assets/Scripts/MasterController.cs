namespace Pong {
    using Pong.Network;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    public class MasterController : MonoBehaviour {
        private static MasterController instance;
        public Image imgBack;
        public Button btnSingle;
        public Button btnMulti;
        public Button btnHotseat;
        public Button btnAI;
        public Button brnQuit;
        public Text textStatus;

        public static MasterController Instance { get { return instance; } }

        void Awake() {
            if(instance != null) {
                Destroy(gameObject);
                return;
            }
            instance = this;

            ShowBackground();
            ShowMenuScreen();
        }

        private void ShowBackground() {
            imgBack.gameObject.SetActive(true);
        }

        private void HideBackground() {
            imgBack.gameObject.SetActive(false);
        }

        private void ShowMenuScreen() {
            btnMulti.GetComponentInChildren<Text>().text = "Multiplayer";
            btnSingle.gameObject.SetActive(true);
            btnMulti.gameObject.SetActive(true);
            btnHotseat.gameObject.SetActive(true);
            btnAI.gameObject.SetActive(true);
            brnQuit.gameObject.SetActive(true);
            textStatus.gameObject.SetActive(true);
            textStatus.text = string.Empty;
        }

        private void HideMenuScreen() {
            btnSingle.gameObject.SetActive(false);
            btnMulti.gameObject.SetActive(false);
            btnAI.gameObject.SetActive(false);
            btnHotseat.gameObject.SetActive(false);
            brnQuit.gameObject.SetActive(false);
            textStatus.gameObject.SetActive(false);
        }

        private void Update() {
            if(Input.GetKeyDown(KeyCode.Escape)) {
                SceneManager.LoadScene(0); //reload
            }
            if(NetworkController.Instance.LanBC != null) {
                textStatus.text = NetworkController.Instance.LanBC.strMessage;
            }
        }

        public void Singleplayer() {
            HideMenuScreen();
            HideBackground();
            GameController.Instance.StartSingleplayer();
        }


        public void Multiplayer() {
            var multiText = btnMulti.GetComponentInChildren<Text>();
            if(multiText.text == "Multiplayer") {
                btnSingle.interactable = false;
                btnHotseat.interactable = false;
                btnAI.interactable = false;
                btnMulti.GetComponentInChildren<Text>().text = "Cancel";
                NetworkController.Instance.StartSearchForGame();
            } else {
                btnSingle.interactable = true;
                btnHotseat.interactable = true;
                btnAI.interactable = true;
                multiText.text = "Multiplayer";
                NetworkController.Instance.StopServer();
                NetworkController.Instance.LanBC.StopBroadcasting();
            }
        }

        public void MultiplayerGameStarted() {
            HideMenuScreen();
            HideBackground();
        }

        public void Hotseat() {
            HideMenuScreen();
            HideBackground();
            GameController.Instance.StartHotseat();
        }

        public void AIvsAI() {
            HideMenuScreen();
            HideBackground();
            GameController.Instance.StartAIvsAI();
        }

        public void Exit() {
            Application.Quit();
        }

    }
}