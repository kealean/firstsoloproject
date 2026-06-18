using UnityEngine;

namespace script.Lib.Sound {
    [CreateAssetMenu(fileName = "SoundClip", menuName = "SO/SoundClip", order = 0)]
    public class SoundClipSO : ScriptableObject {
        public enum AudioType {
            Sfx,
            Music
        }

        public AudioType audioType;
        public AudioClip clip;
        public bool randomizePitch;

        [Range(0f, 1f)] public float randomPitchRange = .1f;
        [Range(0.1f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float basePitch = 1f;
    }
}