using System.Collections;
using System.Collections.Generic;
using Script.Player;
using TMPro;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// 씬 전환을 위해 네임스페이스 추가

namespace script.Managers {
    public class JudgementManager : MonoBehaviour {
        [Header("References")] [SerializeField]
        private InputActionAsset inputActionsAsset;

        // 유니티 인스펙터 연동을 위한 기존 NoteManager 참조
        [SerializeField] private NoteManager noteManager;
        [SerializeField] private TextMeshProUGUI judgementText;
        [SerializeField] private TextMeshProUGUI rateText;
        [SerializeField] private TextMeshProUGUI scoreText;

        private readonly Queue<Note> _noteQueue = new();

        private readonly float[] _scoreMultipliers = { 0f, 1.1f, 1.0f, 0.6f, 0.3f, 0f };

        private int _countNotes;
        private InputAction _downAction;
        private InputAction _leftAction;

        // NoteManager와 CalibrationManager를 공통으로 담기 위한 인터페이스 참조 변수
        private INoteManager _noteManager;
        private InputAction _rightAction;
        private float _totalRate;
        private float _totalScore;

        private InputAction _upAction;

        private void Awake() {
            _countNotes = 0;
            _totalRate = 0;
            _totalScore = 0;

            // 1순위: 인스펙터에 지정된 NoteManager를 참조합니다.
            if (noteManager != null) _noteManager = noteManager;

            // 2순위: 인스펙터에 없을 경우 씬 전체에서 NoteManager를 자동 검색합니다.
            if (_noteManager == null) _noteManager = FindFirstObjectByType<NoteManager>();

            // 3순위: 캘리브레이션 씬 등에서 CalibrationManager가 단독 동작할 경우 이를 자동 바인딩합니다.
            if (_noteManager == null) _noteManager = FindFirstObjectByType<CalibrationManager>();

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

        private void Update() {
            if (_noteManager == null) return;

            var currentDspTime = AudioSettings.dspTime;

            while (_noteQueue.Count > 0) {
                var oldestNote = _noteQueue.Peek();
                if (oldestNote == null || oldestNote.IsHit) {
                    _noteQueue.Dequeue();
                    continue;
                }

                if (currentDspTime - oldestNote.TargetDspTime > 0.100) {
                    _noteQueue.Dequeue();

                    oldestNote.IsHit = true;

                    oldestNote.OnMiss();
                    ApplyResult(5, 0.1);
                }
                else {
                    break;
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

        private void OnUp(InputAction.CallbackContext context) {
            ProcessJudgement(context.time, 1);
        }

        private void OnDown(InputAction.CallbackContext context) {
            ProcessJudgement(context.time, 2);
        }

        private void OnLeft(InputAction.CallbackContext context) {
            ProcessJudgement(context.time, 4);
        }

        private void OnRight(InputAction.CallbackContext context) {
            ProcessJudgement(context.time, 8);
        }

        public void RegisterNote(Note note) {
            _noteQueue.Enqueue(note);
        }

        private void CleanQueue() {
            while (_noteQueue.Count > 0) {
                var oldest = _noteQueue.Peek();
                if (oldest == null || oldest.IsHit)
                    _noteQueue.Dequeue();
                else
                    break;
            }
        }

        private void ProcessJudgement(double inputTime, int inputDir) {
            if (_noteManager == null) return;

            CleanQueue();

            if (_noteQueue.Count == 0) return;

            // 오디오 시작 DSP 시간과 기기 실행 후 경과 입력 시간 차이를 이용해 정밀한 입력 시점을 구합니다.
            var inputDspTime = _noteManager.StartTime + (inputTime - _noteManager.InputSystemStartTime);

            var targetNote = _noteQueue.Peek();

            // 정박(TargetDspTime) 대비 실제 사용자가 키를 입력한 시점(inputDspTime)의 오차를 구합니다.
            var diff = inputDspTime - targetNote.TargetDspTime;
            var absDiff = diff < 0 ? -diff : diff;

            if (!(absDiff <= 0.100)) return;
            var directionMatches = (inputDir & targetNote.Data.dir) != 0;

            if (!directionMatches) return;
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

            var judgeType = results[0];
            ApplyResult(judgeType, diff); // 판정 처리 시 실제 오차값(diff)을 인자로 넘깁니다.

            targetNote.OnHit();

            targetTimes.Dispose();
            results.Dispose();

            // 캘리브레이션 씬 전용 동작: 입력 오차를 CalibrationManager에 누적 전달합니다.
            if (_noteManager.IsCalibrationMode && _noteManager is CalibrationManager calibMgr)
                calibMgr.RecordOffset(diff);
        }

        /// <summary>
        ///     판정 등급에 맞춰 점수 및 정확도를 누적하고, 씬 모드에 맞게 UI 결과를 갱신합니다.
        /// </summary>
        private void ApplyResult(int judgeType, double diff) {
            _countNotes++;
            if (judgeType == 0) return;

            var multiplier = _scoreMultipliers[judgeType];
            _totalScore += 1000 * multiplier;

            _totalRate += multiplier * 100;
            GameManager.Instance.score = (int)_totalScore;
            GameManager.Instance.rate = _totalRate / _countNotes;
            scoreText.SetText($"{_totalScore}");

            // 캘리브레이션 모드와 일반 게임 모드 출력 분기 처리
            if (_noteManager is { IsCalibrationMode: true }) {
                // 초 단위 오차를 ms 단위로 환산 (1초 = 1000ms)
                var diffMs = diff * 1000.0;
                // 소수점 첫째 자리까지 ms 단위로 표기 (부호 포함)
                judgementText.SetText($"{(diffMs >= 0 ? "+" : "")}{diffMs:F1}ms");
            }
            else {
                // 일반 플레이 씬에서는 기존대로 판정 이름(PERFECT 등) 출력
                judgementText.SetText(GetJudgeName(judgeType));
                switch (judgeType) {
                    case 1:
                        GameManager.Instance.perfectPlus++;
                        break;
                    case 2:
                        GameManager.Instance.perfect++;
                        break;
                    case 3:
                        GameManager.Instance.good++;
                        break;
                    case 4:
                        GameManager.Instance.poor++;
                        break;
                    case 5:
                        GameManager.Instance.miss++;
                        break;
                }
            }

            rateText.SetText($"{_totalRate / _countNotes:F1}%");

            // 모든 노트가 판정 처리(입력 완료)되었는지 검사합니다.
            CheckSongCompletion();
        }

        /// <summary>
        ///     곡에 포함된 모든 노트가 완수(판정 처리)되었는지 체크하여 씬 이동 프로세스를 밟습니다.
        /// </summary>
        private void CheckSongCompletion() {
            if (_noteManager == null || _noteManager.MapData == null || _noteManager.MapData.notes == null) return;

            var totalNotes = _noteManager.MapData.notes.Count;
            // 판정된 노트 갯수가 총 노트 갯수 이상일 때 완수로 판단합니다.
            if (_countNotes < totalNotes) return;
            if (_noteManager.IsCalibrationMode) {
                // 캘리브레이션 모드: 판정 텍스트 위치에 최종 조율된 캘리브레이션 결과 노출
                if (GameManager.Instance != null) {
                    var finalCalibMs = GameManager.Instance.calibrationTime * 1000.0;
                    judgementText.SetText($"Result: {(finalCalibMs >= 0 ? "+" : "")}{finalCalibMs:F1}ms");
                }

                // 2초 딜레이 후 3번 씬으로 이동
                StartCoroutine(LoadSceneWithDelay(1, 2.0f));
            }
            else {
                // 일반 게임 모드: 2초 딜레이 후 4번 씬으로 이동
                StartCoroutine(LoadSceneWithDelay(4, 2.0f));
            }
        }

        /// <summary>
        ///     지정한 시간(초)만큼 대기한 후 다음 씬으로 전환합니다.
        /// </summary>
        private IEnumerator LoadSceneWithDelay(int sceneIndex, float delay) {
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene(sceneIndex);
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