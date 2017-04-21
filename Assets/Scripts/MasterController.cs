namespace Pong {
    using Pong.Network;
    using UnityEngine;
    using UnityEngine.UI;

    public class MasterController : MonoBehaviour {
        public enum MenuState {
            NotInMenu = 0,
            Pause = 1,
            MainMenu = 2,
            Searching = 3
        }

        public Image imgBack;
        public Button btnSingle;
        public Button btnMulti;
        public Button btnHotseat;
        public Button btnAI;
        public Button brnQuit;

        protected MenuState state;

        void Awake() {
            ShowBackground();
            ShowMenuScreen();
            state = MenuState.MainMenu;
        }

        private void ShowBackground() {
            imgBack.gameObject.SetActive(true);
        }

        private void HideBackground() {
            imgBack.gameObject.SetActive(false);
        }

        private void ShowMenuScreen() {
            btnSingle.gameObject.SetActive(true);
            btnMulti.gameObject.SetActive(true);
            btnHotseat.gameObject.SetActive(true);
            btnAI.gameObject.SetActive(true);
            brnQuit.gameObject.SetActive(true);
        }

        private void HideMenuScreen() {
            btnSingle.gameObject.SetActive(false);
            btnMulti.gameObject.SetActive(false);
            btnAI.gameObject.SetActive(false);
            btnHotseat.gameObject.SetActive(false);
            brnQuit.gameObject.SetActive(false);
        }

        private void Update() {
            if(Input.GetKeyDown(KeyCode.Escape)) {

            }    
        }

        public void Singleplayer() {
            HideMenuScreen();
            HideBackground();
            GameController.Instance.StartSingleplayer();
        }

        public void Multiplayer() {

            NetworkController.Instance.StartSearchForGame();
        }

        public void Hotseat() {

        }

        public void AIvsAI() {

        }

        public void Exit() {
            Application.Quit();
        }

    }
}