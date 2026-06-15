using System;
using System.Collections;
using script.Lib.Pooling;
using UnityEngine;

namespace Script.Player {
    public class OnClickParticle : MonoBehaviour, IPoolable {
        [SerializeField] private ParticleSystem vfx;

        private void OnEnable() {
            vfx.Play();
            StartCoroutine(ReturnPool());
        }

        private IEnumerator ReturnPool() {
            yield return new WaitForSeconds(0.25f);
            PoolManager.Instance.Push(this);
        }

        [field:SerializeField]public PoolItemSO Item { get; private set; }
        public GameObject GameObject => gameObject;
        public void ResetItem() { }
    }
}