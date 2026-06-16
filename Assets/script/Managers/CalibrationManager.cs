using System.Collections.Generic;
using System.IO;
using Script.Player;
using UnityEngine;

namespace script.Managers {
    /// <summary>
    /// 캘리브레이션(타이밍 조정) 씬에서 사용되는 전용 매니저 클래스입니다.
    /// 싱글톤이 아니며, INoteManager 인터페이스를 구현하여 JudgementManager 등과 연동됩니다.
    /// </summary>
    public class CalibrationManager : MonoBehaviour, INoteManager {
        [Header("References")]
        [SerializeField] private Player player;
        [SerializeField] private JudgementManager judgementManager;
        [SerializeField] private AudioSource audioSource;

        [Header("Note Customization")]
        [SerializeField] private GameObject singleArrowPrefab;
        [SerializeField] private GameObject doubleArrowPrefab;

        // 생성되어 트랙 위에 활성화된 노트 리스트
        private readonly List<Note> _activeNotes = new();

        /// <summary>
        /// 캘리브레이션 모드 여부 플래그 (true 반환)
        /// </summary>
        public bool IsCalibrationMode => true;

        /// <summary>
        /// 오디오 재생 시작 시점의 DSP 시간
        /// </summary>
        public double StartTime { get; private set; }

        /// <summary>
        /// 게임이 실행된 이후 경과한 시작 시점의 실제 시간
        /// </summary>
        public double InputSystemStartTime { get; private set; }

        /// <summary>
        /// 현재 캘리브레이션 씬에서 로드한 맵 데이터
        /// </summary>
        public SongMapData MapData { get; private set; }

        /// <summary>
        /// 플레이어가 입력한 타이밍의 오차들을 누적 기록하는 리스트
        /// </summary>
        private readonly List<double> _offsets = new();

        private void Start() {
            // 현재 GameManager에 등록된 곡 번호에 맞춰 맵 데이터를 로드하고 파싱합니다.
            if (GameManager.Instance != null && GameManager.Instance.jsonMapFile.Length > GameManager.Instance.songNumber) {
                MapData = SongMapData.FromJson(GameManager.Instance.jsonMapFile[GameManager.Instance.songNumber].text);
            }

            if (MapData == null) {
                Debug.LogError("CalibrationManager: MapData 로딩에 실패했습니다.");
                return;
            }

            // 노트를 음악 비트 도달 시점 순서로 오름차순 정렬합니다.
            MapData.notes.Sort((a, b) => {
                var beatA = (a.measure - 1) * 4f + (a.position - 1f) / MapData.snapDivision * 4f;
                var beatB = (b.measure - 1) * 4f + (b.position - 1f) / MapData.snapDivision * 4f;
                return beatA.CompareTo(beatB);
            });

            // 음악 BPM에 맞춰 플레이어 캐릭터의 이동 속도를 동기화합니다.
            var speed = MapData.bpm / 60.0f;
            if (player != null) {
                player.Speed = speed;
            }

            // 기준 시간들을 기록합니다.
            StartTime = AudioSettings.dspTime;
            InputSystemStartTime = Time.realtimeSinceStartup;

            if (player != null) {
                player.StartTime = StartTime;
            }

            // 오디오 소스가 설정되어 있으면 재생 예약 설정을 진행합니다.
            if (audioSource != null) {
                if (!string.IsNullOrEmpty(MapData.songFile)) {
                    var clipName = Path.GetFileNameWithoutExtension(MapData.songFile);
                    var loadedClip = Resources.Load<AudioClip>(clipName);
                    if (loadedClip != null) {
                        audioSource.clip = loadedClip;
                    }
                }

                if (audioSource.clip != null) {
                    var secondsPerBeat = 60.0 / MapData.bpm;
                    // GameManager에 설정된 calibrationTime(보정치) 만큼 당겨서 재생합니다.
                    var delaySeconds = 16.0 * secondsPerBeat - GameManager.Instance.calibrationTime;
                    audioSource.PlayScheduled(StartTime + delaySeconds);
                }
            }

            // 캘리브레이션 교정용 노트들을 스폰합니다.
            SpawnNotes();
        }

        private void Update() {
            if (MapData == null) return;

            // 오디오 DSP 시간에 연동하여 실시간 비트를 산출합니다.
            var elapsedTime = AudioSettings.dspTime - StartTime;
            var currentBeat = (float)(elapsedTime * (MapData.bpm / 60.0f));

            // 생성된 노트들이 플레이어 다가오기 4박자 전부터 나타나도록 조절합니다.
            foreach (var t in _activeNotes) {
                if (t != null && !t.IsHit) {
                    t.CheckAndScaleUp(currentBeat, 0.5f, DG.Tweening.Ease.OutBack);
                }
            }
        }

        /// <summary>
        /// 판정 성공 시 호출되어 사용자의 입력 오차를 수집하고 GameManager의 보정 시간을 갱신합니다.
        /// </summary>
        /// <param name="offset">실제 입력 시간 - 목표 시간 차이 (초 단위, +는 느리게 입력됨 / -는 빠르게 입력됨)</param>
        public void RecordOffset(double offset) {
            _offsets.Add(offset);
            
            // GameManager의 캘리브레이션 시간을 조율합니다.
            if (GameManager.Instance != null) {
                // 입력이 목표보다 늦었을 때(+ offset)는 calibrationTime(당겨 재생하는 시간)을 증가시키고,
                // 입력이 목표보다 빨랐을 때(- offset)는 calibrationTime을 감소시켜 줍니다.
                // 튐 방지를 위해 감쇠 가중치(0.5)를 적용하여 점진적으로 수렴하게 만듭니다.
                GameManager.Instance.calibrationTime += offset * 0.5;
                Debug.Log($"[Calibration] Offset: {offset * 1000.0:F1}ms, New calibrationTime: {GameManager.Instance.calibrationTime * 1000.0:F1}ms");
            }
        }

        /// <summary>
        /// 캘리브레이션용 노트를 월드 상에 소환하고 JudgementManager 큐에 등록합니다.
        /// </summary>
        private void SpawnNotes() {
            const float startDelayBeats = 16.0f;
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

        /// <summary>
        /// 특정 비트 시점의 궤적 좌표를 계산해 반환합니다.
        /// </summary>
        private Vector3 GetPositionAtBeat(float beat) {
            if (player == null || player.Corners == null || player.Corners.Length < 4) return Vector3.zero;

            var progress = beat % 4.0f;
            var currentIndex = (Mathf.FloorToInt(progress) + player.StartingCornerIndex) % 4;
            var nextIndex = (currentIndex + 1) % 4;
            var t = progress - Mathf.FloorToInt(progress);

            return Vector3.Lerp(player.Corners[currentIndex], player.Corners[nextIndex], t);
        }

        /// <summary>
        /// 노트 방향 비트 플래그에 부합하는 회전 각도를 반환합니다.
        /// </summary>
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
