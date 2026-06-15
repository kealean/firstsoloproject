using UnityEngine;

namespace script.Lib.Pooling {
    public interface IPoolable {
        public PoolItemSO Item { get; }
        public GameObject GameObject { get; }

        public void ResetItem();
    }
}