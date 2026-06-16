using System;
using script.Lib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace script.Managers {
    public class GameManager : MonoSingleton<GameManager> {
        public int songNumber;
        public double calibrationTime = 0.1;
        public int perfectPlus;
        public int perfect;
        public int good;
        public int poor;
        public int miss;

        public float rate;
        public int score;

        public TextAsset[] jsonMapFile;

        public string titleText;

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