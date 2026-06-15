using System.Collections;
using script.Lib.Pooling;
using UnityEngine;

namespace Script.Player {
    public class OnClickParticle : MonoBehaviour, IPoolable {
        [SerializeField] private ParticleSystem vfx;

        [field: SerializeField] public PoolItemSO Item { get; private set; }
        public GameObject GameObject => gameObject;
        public void ResetItem() { }

        public void Initialize(Transform parent) {
            transform.position = parent.position;
            StartCoroutine(ReturnPool());
        }

        private IEnumerator ReturnPool() {
            vfx.Clear();
            vfx.Play();
            yield return new WaitForSeconds(0.5f);
            PoolManager.Instance.Push(this);
        }
    }
}