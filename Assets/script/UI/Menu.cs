using script.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace script.UI {
    public class Menu : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI songData;
        [SerializeField] private TextMeshProUGUI bpmText;
        [SerializeField] private TextMeshProUGUI difficultyText;
        [SerializeField] private Button prev;
        [SerializeField] private Button next;
        [SerializeField] private GameObject escMenu;
        [SerializeField] private GameObject faq;
        
        private void Start() {
            escMenu.SetActive(false);
            faq.SetActive(false);
        }

        private void Update() {
            prev.interactable = GameManager.Instance.songNumber != 0;
            next.interactable = GameManager.Instance.songNumber != GameManager.Instance.jsonMapFile.Length - 2;
            if (GameManager.Instance.songNumber != GameManager.Instance.jsonMapFile.Length - 2 &&
                Keyboard.current.rightArrowKey.wasPressedThisFrame)
                GameManager.Instance.songNumber++;

            if (GameManager.Instance.songNumber != 0 && Keyboard.current.leftArrowKey.wasPressedThisFrame)
                GameManager.Instance.songNumber--;

            if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
                SceneManager.LoadSceneAsync(2);

            if (Keyboard.current.cKey.wasPressedThisFrame)
                SceneManager.LoadSceneAsync(5);

            if (Keyboard.current.escapeKey.wasPressedThisFrame) {
                escMenu.SetActive(true);
                Time.timeScale = 0;
            }

            switch (GameManager.Instance.songNumber) {
                case 0:
                    songData.SetText("Camellia - crystallized");
                    bpmText.SetText("BPM: 174");
                    difficultyText.SetText("Difficult: ●●○○○○○○○○");
                    break;
                case 1:
                    songData.SetText("Silentroom - Nhelv");
                    bpmText.SetText("BPM: 174.59");
                    difficultyText.SetText("Difficult: ●●●●●●●●●●");
                    break;
            }
        }

        public void OpenBtn() {
            faq.SetActive(true);
        }

        public void CloseBtn() {
            faq.SetActive(false);
        }

        public void CaliBtn() {
            SceneManager.LoadSceneAsync(5);
            Time.timeScale = 1;
        }

        public void ContinueBtn() {
            escMenu.SetActive(false);
            Time.timeScale = 1;
        }

        public void ExitBtn() {
            Application.Quit();
        }

        public void PrevBtn() {
            GameManager.Instance.songNumber--;
        }

        public void NextBtn() {
            GameManager.Instance.songNumber++;
        }
    }
}