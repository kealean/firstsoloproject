using System;
using script.Lib;
using UnityEditor;
using UnityEngine;

namespace script.Managers {
    public class GameManager : MonoSingleton<GameManager> {
        public int songNumber;
        
        public TextAsset[] jsonMapFile;

        private void Update() {
            var clamp = Mathf.Clamp(songNumber, 0, jsonMapFile.Length);
            songNumber = clamp;
        }
    }
}