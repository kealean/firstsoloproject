using System;
using System.Collections;
using script.Lib.Pooling;
using script.Lib.Sound;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace script.Managers {
    public class SoundPlayer : MonoBehaviour, IPoolable {
        [SerializeField] private AudioMixerGroup sfxGroup, musicGroup;

        private AudioSource _audioSource;

        private void Awake() {
            _audioSource = GetComponent<AudioSource>();
        }

        [field: SerializeField] public PoolItemSO Item { get; private set; }
        public GameObject GameObject => this != null ? gameObject : null;

        public void ResetItem() { }
        public event Action<SoundPlayer> OnClipEnd;

        public void PlaySound(SoundClipSO clipData) {
            _audioSource.outputAudioMixerGroup = clipData.audioType switch {
                SoundClipSO.AudioType.Sfx => sfxGroup,
                SoundClipSO.AudioType.Music => musicGroup,
                _ => _audioSource.outputAudioMixerGroup
            };

            _audioSource.volume = clipData.volume;
            _audioSource.pitch = clipData.basePitch;

            if (clipData.randomizePitch)
                _audioSource.pitch += Random.Range(-clipData.randomPitchRange, clipData.randomPitchRange);

            _audioSource.clip = clipData.clip;

            var duration = _audioSource.clip.length + .2f;
            _audioSource.Play();
            StartCoroutine(DisableSoundTimer(duration));
        }

        private IEnumerator DisableSoundTimer(float duration) {
            yield return new WaitForSeconds(duration);
            _audioSource.Stop();
        }

        public void StopAndReturnToPool() {
            _audioSource.Stop();
            PoolManager.Instance.Push(this);
            OnClipEnd?.Invoke(this);
        }
    }
}