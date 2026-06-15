using System.Collections.Generic;
using DG.Tweening;
using Script.Player;
using script.UI;
using UnityEngine;

namespace script.Managers {
    public class NoteManager : MonoBehaviour {
        [Header("References")]
        [SerializeField] private Player player;
        [SerializeField] private JudgementManager judgementManager;
        [SerializeField] private AudioSource audioSource;

        [Header("Note Customization (Optional)")]
        [SerializeField] private GameObject singleArrowPrefab;
        [SerializeField] private GameObject doubleArrowPrefab;

        [Header("Spawn Settings")]
        [SerializeField] private float scaleUpDuration = 0.5f;
        [SerializeField] private Ease scaleUpEase = Ease.OutBack;

        [Header("Init")] 
        [SerializeField] private Title title;

        
        private SongMapData _mapData;
        private double _startTime;
        private double _inputSystemStartTime;
        private readonly List<Note> _activeNotes = new();

        public double StartTime => _startTime;
        public double InputSystemStartTime => _inputSystemStartTime;
        public SongMapData MapData => _mapData;
        
        private void Start() {
            _mapData = SongMapData.FromJson(GameManager.Instance.jsonMapFile[GameManager.Instance.songNumber].text);
            
            title.Initialize();

            Debug.Log($"Song: {_mapData.songTitle} - {_mapData.composer} BPM: {_mapData.bpm}");

            _mapData.notes.Sort((a, b) => {
                float beatA = (a.measure - 1) * 4f + (a.position - 1f) / _mapData.snapDivision * 4f;
                float beatB = (b.measure - 1) * 4f + (b.position - 1f) / _mapData.snapDivision * 4f;
                return beatA.CompareTo(beatB);
            });

            float speed = _mapData.bpm / 60.0f;
            player.Speed = speed;

            _startTime = AudioSettings.dspTime;
            _inputSystemStartTime = Time.realtimeSinceStartup;
            
            player.StartTime = _startTime;

            if (audioSource != null) {
                if (!string.IsNullOrEmpty(_mapData.songFile)) {
                    string clipName = System.IO.Path.GetFileNameWithoutExtension(_mapData.songFile);
                    AudioClip loadedClip = Resources.Load<AudioClip>(clipName);
                    if (loadedClip != null) {
                        audioSource.clip = loadedClip;
                    }
                }

                if (audioSource.clip != null) {
                    double secondsPerBeat = 60.0 / _mapData.bpm;
                    double delaySeconds = 16.0 * secondsPerBeat - 1.4f;
                    audioSource.PlayScheduled(_startTime + delaySeconds);
                }
            }

            SpawnNotes();
        }

        private void Update() {
            if (_mapData == null) return;

            double elapsedTime = AudioSettings.dspTime - _startTime;
            float currentBeat = (float)(elapsedTime * (_mapData.bpm / 60.0f));

            for (int i = 0; i < _activeNotes.Count; i++) {
                if (_activeNotes[i] != null && !_activeNotes[i].IsHit) {
                    _activeNotes[i].CheckAndScaleUp(currentBeat, scaleUpDuration, scaleUpEase);
                }
            }
        }

        private void SpawnNotes() {
            float startDelayBeats = 16.0f;
            double secondsPerBeat = 60.0 / _mapData.bpm;

            foreach (var noteData in _mapData.notes) {
                float noteBeat = startDelayBeats + (noteData.measure - 1) * 4f + (noteData.position - 1f) / _mapData.snapDivision * 4f;
                double targetDspTime = _startTime + noteBeat * secondsPerBeat;

                bool isDoubleArrow = (noteData.dir == 3 || noteData.dir == 12);

                Vector3 notePos = GetPositionAtBeat(noteBeat);

                float rotationAngle = GetRotationAngle(noteData.dir);

                GameObject noteObj = null;
                if (isDoubleArrow && doubleArrowPrefab != null) {
                    noteObj = Instantiate(doubleArrowPrefab, transform);
                } else if (!isDoubleArrow && singleArrowPrefab != null) {
                    noteObj = Instantiate(singleArrowPrefab, transform);
                }

                var noteComp = noteObj?.GetComponent<Note>();
                if (noteComp == null) {
                    noteComp = noteObj?.AddComponent<Note>();
                }
                noteComp?.Init(noteData, noteBeat, targetDspTime, isDoubleArrow, notePos, rotationAngle);

                _activeNotes.Add(noteComp);
                if (judgementManager != null) {
                    judgementManager.RegisterNote(noteComp);
                }
            }
        }

        private Vector3 GetPositionAtBeat(float beat) {
            if (player == null || player.Corners == null || player.Corners.Length < 4) {
                return Vector3.zero;
            }

            float progress = beat % 4.0f;
            int currentIndex = (Mathf.FloorToInt(progress) + player.StartingCornerIndex) % 4;
            int nextIndex = (currentIndex + 1) % 4;
            float t = progress - Mathf.FloorToInt(progress);

            return Vector3.Lerp(player.Corners[currentIndex], player.Corners[nextIndex], t);
        }

        private float GetRotationAngle(int dir) {
            return dir switch {
                8 => 0f,
                12 => 0f,
                1 => 90f,
                3 => 90f,
                4 => 180f,
                2 => 270f,
                _ => 0f
            };
        }
    }
}
