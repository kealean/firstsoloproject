using script.Lib;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

namespace script.Managers {
    public class BGMManager : MonoSingleton<BGMManager> {
        [SerializeField] private string menuBgmResourceName = "Neon Pulse [p7OtXArhjy4]";
        [SerializeField] private float maxVolume = 0.5f;
        [SerializeField] private float fadeDuration = 1.0f;

        private AudioSource _audioSource;
        private AudioClip _menuBgmClip;
        private Tween _fadeTween;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeOnLoad() {
            // Accessing the Instance getter creates the singleton instance automatically
            var instance = Instance;
        }

        protected override void Awake() {
            base.Awake();
            DontDestroyOnLoad(gameObject);

            _audioSource = gameObject.GetComponent<AudioSource>();
            if (_audioSource == null) {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }

            _audioSource.loop = true;
            _audioSource.playOnAwake = false;
            _audioSource.volume = 0f;

            // Load menu BGM from Resources folder
            _menuBgmClip = Resources.Load<AudioClip>(menuBgmResourceName);
            if (_menuBgmClip == null) {
                Debug.LogWarning($"[BGMManager] BGM clip '{menuBgmResourceName}' not found in Resources!");
            }

            // In case the scene was already loaded at the time of Awake (e.g. in editor)
            var currentScene = SceneManager.GetActiveScene();
            CheckAndPlayBGMForScene(currentScene);
        }

        private void OnEnable() {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            CheckAndPlayBGMForScene(scene);
        }

        private void CheckAndPlayBGMForScene(Scene scene) {
            if (scene.name == "Title" || scene.name == "MenuScene" || scene.name == "Result") {
                PlayMenuBGM();
            } else if (scene.name == "GameScene" || scene.name == "CaliScene" || scene.name == "LoadScene") {
                FadeOutBGM();
            }
        }

        public void PlayMenuBGM() {
            if (_menuBgmClip == null) return;

            if (_audioSource.clip != _menuBgmClip) {
                _audioSource.clip = _menuBgmClip;
                _audioSource.volume = 0f;
                _audioSource.Play();
            } else if (!_audioSource.isPlaying) {
                _audioSource.Play();
            }

            // Fade in to maxVolume using DOTween
            _fadeTween?.Kill();
            _fadeTween = _audioSource.DOFade(maxVolume, fadeDuration).SetUpdate(true);
        }

        public void FadeOutBGM() {
            if (!_audioSource.isPlaying) return;

            _fadeTween?.Kill();
            _fadeTween = _audioSource.DOFade(0f, fadeDuration)
                .SetUpdate(true)
                .OnComplete(() => {
                    _audioSource.Stop();
                });
        }
    }
}
