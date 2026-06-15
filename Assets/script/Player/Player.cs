using UnityEngine;

namespace Script.Player {
    public class Player : MonoBehaviour {
        [SerializeField] private Vector3[] corners = new Vector3[4];
        [SerializeField] private int startingCornerIndex = 3; 

        [SerializeField] private float speed = 5.0f;

        private double startTime;

        public Vector3[] Corners => corners;
        public int StartingCornerIndex => startingCornerIndex;

        public float Speed {
            get => speed;
            set => speed = value;
        }

        public double StartTime {
            get => startTime;
            set => startTime = value;
        }

        private void Awake() {
            if (corners != null && corners.Length > 0) {
                int bestIndex = 0;
                float minVal = float.MaxValue;
                for (int i = 0; i < corners.Length; i++) {
                    float val = corners[i].x + corners[i].y;
                    if (Mathf.Abs(corners[i].y) < 0.001f && Mathf.Abs(corners[i].z) > 0.001f) {
                        val = corners[i].x + corners[i].z;
                    }
                    if (val < minVal) {
                        minVal = val;
                        bestIndex = i;
                    }
                }
                startingCornerIndex = bestIndex;
            }
        }

        private void Start() {
            startTime = AudioSettings.dspTime;
        }

        private void Update() {
            var elapsedTime = AudioSettings.dspTime - startTime;
            var progress = (float)(elapsedTime * speed) % 4.0f;

            var currentIndex = (Mathf.FloorToInt(progress) + startingCornerIndex) % 4;
            var nextIndex = (currentIndex + 1) % 4;

            var t = progress - Mathf.FloorToInt(progress);

            transform.position = Vector3.Lerp(corners[currentIndex], corners[nextIndex], t);
        }
    }
}