using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace script.UI {
    public class MainTitle : MonoBehaviour {
        [SerializeField] private GameObject loadingText;
        private Tween _tween;

        private void Update() {
            if (!Keyboard.current.anyKey.wasPressedThisFrame) return;
            SceneManager.LoadSceneAsync(1);
            _tween = loadingText.transform.DOMoveX(0, 1f).SetEase(Ease.OutBack);
        }

        private void OnDestroy() {
            _tween?.Kill();
        }
    }
}