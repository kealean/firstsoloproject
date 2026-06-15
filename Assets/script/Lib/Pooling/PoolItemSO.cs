using UnityEngine;

namespace script.Lib.Pooling {
    [CreateAssetMenu(fileName = "PoolItem", menuName = "SO/Pool/Item", order = 0)]
    public class PoolItemSO : ScriptableObject {
        public string ItemName;
        public GameObject Prefab;
        public int Count;

        private void OnValidate() {
            if (Prefab != null) {
                var item = Prefab.GetComponent<IPoolable>();
                if (item == null) {
                    Prefab = null;
                    Debug.LogWarning("Can not found IPoolable component");
                }
            }
        }
    }
}