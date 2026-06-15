using Script.Player;
using TMPro;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace script.Managers {
    public class JudgementManager : MonoBehaviour {
        [Header("References")]
        [SerializeField] private InputActionAsset inputActionsAsset;
        [SerializeField] private NoteManager noteManager;
        [SerializeField] private TextMeshProUGUI judgementText;
        [SerializeField] private TextMeshProUGUI rateText;
        [SerializeField] private TextMeshProUGUI scoreText;
        
        private int _countNotes;
        private float _totalRate;
        private float _totalScore;
        
        private readonly float[] _scoreMultipliers = { 0f, 1.1f, 1.0f, 0.6f, 0.3f, 0f };

        private InputAction _upAction;
        private InputAction _downAction;
        private InputAction _leftAction;
        private InputAction _rightAction;

        private readonly System.Collections.Generic.Queue<Note> _noteQueue = new();

        private void Awake() {
            _countNotes = 0;
            _totalRate = 0;
            _totalScore = 0;
            if (noteManager == null) {
                noteManager = FindFirstObjectByType<NoteManager>();
            }

            if (inputActionsAsset != null) {
                var playerMap = inputActionsAsset.FindActionMap("Player");
                if (playerMap != null) {
                    _upAction = playerMap.FindAction("Up");
                    _downAction = playerMap.FindAction("Down");
                    _leftAction = playerMap.FindAction("Left");
                    _rightAction = playerMap.FindAction("Right");
                } 
            } 
        }

        private void OnEnable() {
            if (_upAction != null) {
                _upAction.Enable();
                _upAction.performed += OnUp;
            }
            if (_downAction != null) {
                _downAction.Enable();
                _downAction.performed += OnDown;
            }
            if (_leftAction != null) {
                _leftAction.Enable();
                _leftAction.performed += OnLeft;
            }
            if (_rightAction != null) {
                _rightAction.Enable();
                _rightAction.performed += OnRight;
            }
        }

        private void OnDisable() {
            if (_upAction != null) {
                _upAction.performed -= OnUp;
                _upAction.Disable();
            }
            if (_downAction != null) {
                _downAction.performed -= OnDown;
                _downAction.Disable();
            }
            if (_leftAction != null) {
                _leftAction.performed -= OnLeft;
                _leftAction.Disable();
            }
            if (_rightAction != null) {
                _rightAction.performed -= OnRight;
                _rightAction.Disable();
            }
        }

        private void OnUp(InputAction.CallbackContext context) => ProcessJudgement(context.time, 1);
        private void OnDown(InputAction.CallbackContext context) => ProcessJudgement(context.time, 2);
        private void OnLeft(InputAction.CallbackContext context) => ProcessJudgement(context.time, 4);
        private void OnRight(InputAction.CallbackContext context) => ProcessJudgement(context.time, 8);
        
        public void RegisterNote(Note note) {
            _noteQueue.Enqueue(note);
        }

        private void Update() {
            if (noteManager == null) return;

            double currentDspTime = AudioSettings.dspTime;
            
            while (_noteQueue.Count > 0) {
                Note oldestNote = _noteQueue.Peek();
                if (oldestNote == null || oldestNote.IsHit) {
                    _noteQueue.Dequeue();
                    continue;
                }

                if (currentDspTime - oldestNote.TargetDspTime > 0.100) {
                    _noteQueue.Dequeue();
                    
                    oldestNote.IsHit = true; 
                    
                    oldestNote.OnMiss();
                    ApplyResult(5);
                } else {
                    break;
                }
            }
        }

        private void CleanQueue() {
            while (_noteQueue.Count > 0) {
                Note oldest = _noteQueue.Peek();
                if (oldest == null || oldest.IsHit) {
                    _noteQueue.Dequeue();
                } else {
                    break;
                }
            }
        }

        private void ProcessJudgement(double inputTime, int inputDir) {
            if (noteManager == null) return;

            CleanQueue();

            if (_noteQueue.Count == 0) return;

            double inputDspTime = noteManager.StartTime + (inputTime - noteManager.InputSystemStartTime);

            Note targetNote = _noteQueue.Peek();

            double diff = inputDspTime - targetNote.TargetDspTime;
            double absDiff = diff < 0 ? -diff : diff;

            if (absDiff <= 0.100) {
                bool directionMatches = (inputDir & targetNote.Data.dir) != 0;

                if (directionMatches) {
                    _noteQueue.Dequeue();

                    var targetTimes = new NativeArray<double>(1, Allocator.TempJob);
                    var results = new NativeArray<int>(1, Allocator.TempJob);

                    targetTimes[0] = targetNote.TargetDspTime;

                    var job = new JudgementJob {
                        TargetDspTimes = targetTimes,
                        InputTimestamp = inputDspTime,
                        Results = results
                    };

                    var handle = job.ScheduleParallel(1, 64, default);
                    handle.Complete();

                    int judgeType = results[0];
                    ApplyResult(judgeType);

                    targetNote.OnHit();

                    targetTimes.Dispose();
                    results.Dispose();
                }
            }
        }

        private void ApplyResult(int judgeType) {
            _countNotes++;
            if (judgeType == 0) return;

            var multiplier = _scoreMultipliers[judgeType];
            _totalScore += 1000 * multiplier;
            
            _totalRate += multiplier * 100;
            scoreText.SetText($"{_totalScore}");
            judgementText.SetText(GetJudgeName(judgeType));
            rateText.SetText($"{_totalRate/_countNotes:F1}%");
        }

        private string GetJudgeName(int type) {
            return type switch {
                1 => "PERFECT+",
                2 => "PERFECT",
                3 => "GOOD",
                4 => "POOR",
                _ => "MISS"
            };
        }
    }
}