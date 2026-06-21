using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using Script.Player;
using script.UI;
using UnityEngine;

namespace script.Managers {
    public class NoteManager : MonoBehaviour, INoteManager {
        [Header("References")] [SerializeField]
        private Player player;

        [SerializeField] private JudgementManager judgementManager;
        [SerializeField] private AudioSource audioSource;

        [Header("Note Customization (Optional)")] [SerializeField]
        private GameObject singleArrowPrefab;

        [SerializeField] private GameObject doubleArrowPrefab;

        [Header("Spawn Settings")] [SerializeField]
        private float scaleUpDuration = 0.5f;

        [SerializeField] private Ease scaleUpEase = Ease.OutBack;

        [Header("Init")] [SerializeField] private Title title;

        private readonly List<Note> _activeNotes = new();

        private void Start() {
            MapData = SongMapData.FromJson(GameManager.Instance.jsonMapFile[GameManager.Instance.songNumber].text);

            title.Initialize();

            Debug.Log($"Song: {MapData.songTitle} - {MapData.composer} BPM: {MapData.bpm}");

            MapData.notes.Sort((a, b) => {
                var beatA = (a.measure - 1) * 4f + (a.position - 1f) / MapData.snapDivision * 4f;
                var beatB = (b.measure - 1) * 4f + (b.position - 1f) / MapData.snapDivision * 4f;
                return beatA.CompareTo(beatB);
            });

            var speed = MapData.bpm / 60.0f;
            player.Speed = speed;

            StartTime = AudioSettings.dspTime;
            InputSystemStartTime = Time.realtimeSinceStartup;

            player.StartTime = StartTime;

            if (audioSource != null) {
                if (!string.IsNullOrEmpty(MapData.songFile)) {
                    var clipName = Path.GetFileNameWithoutExtension(MapData.songFile);
                    var loadedClip = Resources.Load<AudioClip>(clipName);
                    if (loadedClip != null) audioSource.clip = loadedClip;
                }

                if (audioSource.clip != null) {
                    var secondsPerBeat = 60.0 / MapData.bpm;
                    var delaySeconds = 16.0 * secondsPerBeat - GameManager.Instance.calibrationTime -
                                       MapData.offset / 1000f;
                    audioSource.PlayScheduled(StartTime + delaySeconds);
                }
            }

            SpawnNotes();
        }

        private void Update() {
            if (MapData == null) return;
            if (Time.timeScale == 0) return;

            var elapsedTime = AudioSettings.dspTime - StartTime;
            var currentBeat = (float)(elapsedTime * (MapData.bpm / 60.0f));

            foreach (var t in _activeNotes)
                if (t != null && !t.IsHit)
                    t.CheckAndScaleUp(currentBeat, scaleUpDuration, scaleUpEase);
        }
        
        public double StartTime { get; set; }
        
        public double InputSystemStartTime { get; set; }
        
        public SongMapData MapData { get; private set; }
        
        public bool IsCalibrationMode => false;

        public void OnPause() {
            if (audioSource != null && audioSource.isPlaying) {
                audioSource.Pause();
            }
        }

        public void OnResume(double pauseDspDuration, double pauseRealtimeDuration) {
            StartTime += pauseDspDuration;
            InputSystemStartTime += pauseRealtimeDuration;

            if (player != null) {
                player.StartTime = StartTime;
            }

            foreach (var note in _activeNotes) {
                if (note != null) {
                    note.AdjustTargetDspTime(pauseDspDuration);
                }
            }

            if (audioSource != null) {
                if (audioSource.clip != null) {
                    var secondsPerBeat = 60.0 / MapData.bpm;
                    var delaySeconds = 16.0 * secondsPerBeat - GameManager.Instance.calibrationTime -
                                       MapData.offset / 1000f;
                    
                    if (AudioSettings.dspTime < StartTime + delaySeconds) {
                        audioSource.Stop();
                        audioSource.PlayScheduled(StartTime + delaySeconds);
                    } else {
                        audioSource.UnPause();
                    }
                }
            }
        }

        private void SpawnNotes() {
            var startDelayBeats = 16.0f;
            var secondsPerBeat = 60.0 / MapData.bpm;

            foreach (var noteData in MapData.notes) {
                var noteBeat = startDelayBeats + (noteData.measure - 1) * 4f +
                               (noteData.position - 1f) / MapData.snapDivision * 4f;
                var targetDspTime = StartTime + noteBeat * secondsPerBeat;

                var isDoubleArrow = noteData.dir == 3 || noteData.dir == 12;

                var notePos = GetPositionAtBeat(noteBeat);

                var rotationAngle = GetRotationAngle(noteData.dir);

                GameObject noteObj = null;
                if (isDoubleArrow && doubleArrowPrefab != null)
                    noteObj = Instantiate(doubleArrowPrefab, transform);
                else if (!isDoubleArrow && singleArrowPrefab != null)
                    noteObj = Instantiate(singleArrowPrefab, transform);

                var noteComp = noteObj?.GetComponent<Note>();
                if (noteComp == null) noteComp = noteObj?.AddComponent<Note>();
                noteComp?.Init(noteData, noteBeat, targetDspTime, isDoubleArrow, notePos, rotationAngle);

                _activeNotes.Add(noteComp);
                if (judgementManager != null) judgementManager.RegisterNote(noteComp);
            }
        }

        private Vector3 GetPositionAtBeat(float beat) {
            if (player == null || player.Corners == null || player.Corners.Length < 4) return Vector3.zero;

            var progress = beat % 4.0f;
            var currentIndex = (Mathf.FloorToInt(progress) + player.StartingCornerIndex) % 4;
            var nextIndex = (currentIndex + 1) % 4;
            var t = progress - Mathf.FloorToInt(progress);

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