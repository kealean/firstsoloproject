using script.Lib;
using UnityEngine;

namespace script.Managers {
    public class GameManager : MonoSingleton<GameManager> {
        public int songNumber;
        public double calibrationTime = 0.1;

        public TextAsset[] jsonMapFile;

        protected override void Awake() {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }

        private void Update() {
            var clamp = Mathf.Clamp(songNumber, 0, jsonMapFile.Length);
            songNumber = clamp;
        }
    }
}