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

        private void Update() {
            prev.interactable = GameManager.Instance.songNumber != 0;
            next.interactable = GameManager.Instance.songNumber != 2;
            if (GameManager.Instance.songNumber != 0 && Keyboard.current.rightArrowKey.wasPressedThisFrame)
                GameManager.Instance.songNumber--;

            if (GameManager.Instance.songNumber != 2 && Keyboard.current.leftArrowKey.wasPressedThisFrame)
                GameManager.Instance.songNumber++;

            if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
                SceneManager.LoadSceneAsync(2);
        }
    }
}