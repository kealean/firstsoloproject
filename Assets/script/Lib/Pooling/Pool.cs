using System.Collections.Generic;
using UnityEngine;

namespace script.Lib.Pooling {
    public class Pool {
        private readonly Transform _parentTrm;
        private readonly Stack<IPoolable> _pool;
        private readonly IPoolable _poolable;
        private readonly GameObject _prefab;

        public Pool(IPoolable poolable, Transform parentTrm, int count) {
            _pool = new Stack<IPoolable>(count);
            _parentTrm = parentTrm;
            _poolable = poolable;
            _prefab = poolable.GameObject;

            for (var i = 0; i < count; i++) {
                var item = CreatePoolItem();
                _pool.Push(item);
            } 
        }

        private IPoolable CreatePoolItem() {
            var gameObject = Object.Instantiate(_prefab, _parentTrm);
            gameObject.SetActive(false);
            gameObject.name = _poolable.Item.ItemName;
            return gameObject.GetComponent<IPoolable>();
        }

        public IPoolable Pop() {
            IPoolable item = null;
            if (_pool.Count <= 0) {
                item = CreatePoolItem();
            }
            else {
                item = _pool.Pop();
            }
            item.GameObject.SetActive(true);
            return item;
        }

        public void Push(IPoolable item) {
            item.GameObject.SetActive(false);
            _pool.Push(item);
        }
    }
}