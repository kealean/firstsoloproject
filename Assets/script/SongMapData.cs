using System;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Player {
    [Serializable]
    public class NoteData {
        public int measure;
        public int position;
        public int dir;
    }

    [Serializable]
    public class SongMapData {
        public string songTitle;
        public string composer;
        public float bpm;
        public string songFile;
        public int snapDivision;
        public List<NoteData> notes;

        public static SongMapData FromJson(string jsonString) {
            return JsonUtility.FromJson<SongMapData>(jsonString);
        }

        public string ToJson(bool prettyPrint = false) {
            return JsonUtility.ToJson(this, prettyPrint);
        }
    }
}