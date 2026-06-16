using UnityEngine;
using UnityEngine.SceneManagement;

namespace script.Managers {
    public class LoadManager : MonoBehaviour {
        private void OnEnable() {
            SceneManager.LoadSceneAsync(3);
            GameManager.Instance.perfectPlus = 0;
            GameManager.Instance.perfect = 0;
            GameManager.Instance.good = 0;
            GameManager.Instance.poor = 0;
            GameManager.Instance.miss = 0;
            GameManager.Instance.score = 0;
            GameManager.Instance.rate = 0f;
        }
    }
}