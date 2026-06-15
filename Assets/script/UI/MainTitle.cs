using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace script.UI {
    public class MainTitle : MonoBehaviour {
        [SerializeField] private GameObject loadingText;
        
        private void Update() {
            if (Keyboard.current.anyKey.wasPressedThisFrame) {
                SceneManager.LoadSceneAsync(1);
                loadingText.transform.DOMoveX(0, 1f).SetEase(Ease.OutBack);
            }
        }
    }
}