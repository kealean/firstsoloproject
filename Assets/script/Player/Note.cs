using DG.Tweening;
using script;
using script.Lib.Pooling;
using UnityEngine;

namespace Script.Player {
    public class Note : MonoBehaviour {
        [SerializeField] private PoolItemSO poolItem;
        private bool _isScaledUp;

        private Vector3 _originalScale;
        private SpriteRenderer _spriteRenderer;

        public NoteData Data { get; private set; }
        public float TargetBeat { get; private set; }
        public double TargetDspTime { get; private set; }
        public bool IsDoubleArrow { get; private set; }
        public bool IsHit { get; set; }

        public void Init(NoteData data, float targetBeat, double targetDspTime, bool isDoubleArrow, Vector3 position,
            float rotationAngle) {
            Data = data;
            TargetBeat = targetBeat;
            TargetDspTime = targetDspTime;
            IsDoubleArrow = isDoubleArrow;

            transform.position = position;
            transform.rotation = Quaternion.Euler(0, 0, rotationAngle);
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            _originalScale = transform.localScale;
            if (_originalScale == Vector3.zero) _originalScale = Vector3.one;
            transform.localScale = Vector3.zero;
        }

        public void CheckAndScaleUp(float currentBeat, float duration, Ease easeType) {
            if (_isScaledUp || !(TargetBeat - currentBeat <= 4.0f)) return;
            _isScaledUp = true;
            transform.DOKill();
            transform.DOScale(_originalScale, duration).SetEase(easeType);
        }

        public void OnHit() {
            IsHit = true;
            var particle = PoolManager.Instance.Pop(poolItem.ItemName) as OnClickParticle;
            if (particle != null) particle.Initialize(transform);
            transform.DOKill();

            transform.DOScale(_originalScale * 1.5f, 0.15f).SetEase(Ease.OutQuad);
            if (_spriteRenderer != null)
                _spriteRenderer.DOFade(0f, 0.15f).OnComplete(() => Destroy(gameObject));
            else
                Destroy(gameObject, 0.15f);
        }

        public void OnMiss() {
            transform.DOKill();

            if (_spriteRenderer != null) {
                _spriteRenderer.DOColor(Color.red, 0.15f);
                _spriteRenderer.DOFade(0f, 0.15f).OnComplete(() => Destroy(gameObject));
            }
            else {
                Destroy(gameObject, 0.15f);
            }
        }

        public void AdjustTargetDspTime(double duration) {
            TargetDspTime += duration;
        }
    }
}