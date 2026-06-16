using UnityEngine;

namespace Script.Player {
    public class Player : MonoBehaviour {
        [SerializeField] private Vector3[] corners = new Vector3[4];
        [SerializeField] private int startingCornerIndex = 3;

        [SerializeField] private float speed = 5.0f;

        public Vector3[] Corners => corners;
        public int StartingCornerIndex => startingCornerIndex;

        public float Speed {
            get => speed;
            set => speed = value;
        }

        public double StartTime { get; set; }

        private void Awake() {
            if (corners == null || corners.Length <= 0) return;
            var bestIndex = 0;
            var minVal = float.MaxValue;
            for (var i = 0; i < corners.Length; i++) {
                var val = corners[i].x + corners[i].y;
                if (Mathf.Abs(corners[i].y) < 0.001f && Mathf.Abs(corners[i].z) > 0.001f)
                    val = corners[i].x + corners[i].z;
                if (val < minVal) {
                    minVal = val;
                    bestIndex = i;
                }
            }

            startingCornerIndex = bestIndex;
        }

        private void Update() {
            var elapsedTime = AudioSettings.dspTime - StartTime;
            var progress = (float)(elapsedTime * speed) % 4.0f;

            var currentIndex = (Mathf.FloorToInt(progress) + startingCornerIndex) % 4;
            var nextIndex = (currentIndex + 1) % 4;

            var t = progress - Mathf.FloorToInt(progress);

            transform.position = Vector3.Lerp(corners[currentIndex], corners[nextIndex], t);
        }
    }
}