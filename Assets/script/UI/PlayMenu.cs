using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using script.Managers;

namespace script.UI {
    public class PlayMenu : MonoBehaviour {
        [SerializeField] private GameObject escMenu;
        [SerializeField] private Button caliBtn;

        private double _pauseStartDspTime;
        private double _pauseStartRealtime;
        private bool _isPaused;
        
        private void Start() {
            escMenu.SetActive(false);
            if (SceneManager.GetActiveScene().buildIndex == 5) {
                caliBtn.interactable = false;
            }
        }

        private void Update() {
            if (Keyboard.current.escapeKey.wasPressedThisFrame && !_isPaused) {
                PauseGame();
            }
        }

        private void PauseGame() {
            _isPaused = true;
            escMenu.SetActive(true);
            Time.timeScale = 0;
            _pauseStartDspTime = AudioSettings.dspTime;
            _pauseStartRealtime = Time.realtimeSinceStartup;
            AudioListener.pause = true;

            INoteManager activeManager = FindFirstObjectByType<NoteManager>();
            if (activeManager == null) {
                activeManager = FindFirstObjectByType<CalibrationManager>();
            }

            if (activeManager != null) {
                activeManager.OnPause();
            }
        }

        public void CaliBtn() {
            SceneManager.LoadSceneAsync(5);
            Time.timeScale = 1;
            AudioListener.pause = false;
        }

        public void ContinueBtn() {
            ResumeGame();
        }

        private void ResumeGame() {
            _isPaused = false;
            escMenu.SetActive(false);
            Time.timeScale = 1;
            AudioListener.pause = false;

            double pauseDspDuration = AudioSettings.dspTime - _pauseStartDspTime;
            double pauseRealtimeDuration = Time.realtimeSinceStartup - _pauseStartRealtime;

            INoteManager activeManager = FindFirstObjectByType<NoteManager>();
            if (activeManager == null) {
                activeManager = FindFirstObjectByType<CalibrationManager>();
            }

            if (activeManager != null) {
                activeManager.OnResume(pauseDspDuration, pauseRealtimeDuration);
            }
        }

        public void ExitBtn() {
            SceneManager.LoadSceneAsync(1);
            AudioListener.pause = false;
            Time.timeScale = 1;
        }
    }
}