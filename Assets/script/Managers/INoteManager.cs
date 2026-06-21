using Script.Player;

namespace script.Managers {
    /// <summary>
    ///     노트 매니저와 캘리브레이션 매니저가 공유하는 핵심 타이밍 정보 및 데이터를 정의하는 공통 인터페이스입니다.
    /// </summary>
    public interface INoteManager {
        /// <summary>
        ///     오디오 재생 시작 시점의 DSP 시간 (AudioSettings.dspTime 기준, 초 단위)
        /// </summary>
        double StartTime { get; set; }

        /// <summary>
        ///     게임이 실행된 이후 경과한 시작 시점의 실제 시간 (Time.realtimeSinceStartup 기준, 초 단위)
        /// </summary>
        double InputSystemStartTime { get; set; }

        /// <summary>
        ///     현재 로드된 음악의 맵 데이터 (BPM, 노트 목록 정보 등 포함)
        /// </summary>
        SongMapData MapData { get; }

        /// <summary>
        ///     현재 씬이 타이밍 교정을 위한 캘리브레이션 모드로 동작하고 있는지 여부
        /// </summary>
        bool IsCalibrationMode { get; }

        /// <summary>
        ///     일시정지 되었을 때 호출됩니다.
        /// </summary>
        void OnPause();

        /// <summary>
        ///     일시정지가 해제되었을 때 호출되어 싱크 보정을 처리합니다.
        /// </summary>
        void OnResume(double pauseDspDuration, double pauseRealtimeDuration);
    }
}