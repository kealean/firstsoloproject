using UnityEngine;
using UnityEngine.SceneManagement;

namespace script.Managers {
    public class LoadManager : MonoBehaviour {
        private void OnEnable() {
            SceneManager.LoadSceneAsync(3);
        }
    }
}