using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;

namespace script.Lib.Pooling {
    public class PoolManager : MonoSingleton<PoolManager> {
        [SerializeField] private PoolingListSO poolingList;
        private Dictionary<string, Pool> _poolDict;

        protected override void Awake() {
            base.Awake();
            _poolDict = new Dictionary<string, Pool>();

            foreach (var pair in poolingList.list) CreatePool(pair.ItemName, pair.Prefab, pair.Count);
        }

        private void CreatePool(string itemName, GameObject prefab, int count) {
            var poolable = prefab.GetComponent<IPoolable>();
            Debug.Assert(poolable != null, $"GameObject must have an IPoolable component : {prefab.gameObject}");
            var pool = new Pool(poolable, transform, count);
            _poolDict.Add(itemName, pool);
        }

        public IPoolable Pop(string itemName) {
            if (_poolDict.TryGetValue(itemName, out var pool)) {
                var item = pool.Pop();
                item.ResetItem();
                return item;
            }
            
            return null;
        }

        public void Push(IPoolable item) {
            if(_poolDict.TryGetValue(item.Item.ItemName, out var pool)) pool.Push(item);
        }
    }
}