namespace script.Managers {
    public interface INoteManager {
        double StartTime { get; set; }

        double InputSystemStartTime { get; set; }

        SongMapData MapData { get; }

        bool IsCalibrationMode { get; }

        void OnPause();

        void OnResume(double pauseDspDuration, double pauseRealtimeDuration);
    }
}