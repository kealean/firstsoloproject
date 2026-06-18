using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace script.UI {
    public class PlayMenu : MonoBehaviour {
        [SerializeField] private GameObject escMenu;
        [SerializeField] private Button caliBtn;
        
        private void Start() {
            escMenu.SetActive(false);
            if (SceneManager.GetActiveScene().buildIndex == 5) {
                caliBtn.interactable = false;
            }
        }

        private void Update() {
            if (Keyboard.current.escapeKey.wasPressedThisFrame) {
                escMenu.SetActive(true);
                Time.timeScale = 0;
                AudioListener.pause = true;
            }
        }

        public void CaliBtn() {
            SceneManager.LoadSceneAsync(5);
        }

        public void ContinueBtn() {
            escMenu.SetActive(false);
            Time.timeScale = 1;
            AudioListener.pause = false;
        }

        public void ExitBtn() {
            SceneManager.LoadSceneAsync(1);
        }
    }
}