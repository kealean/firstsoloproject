using System.Collections.Generic;
using UnityEngine;

namespace script.Lib.Pooling {
    [CreateAssetMenu(fileName = "Pooling list", menuName = "SO/Pool/list", order = 0)]
    public class PoolingListSO : ScriptableObject {
        public List<PoolItemSO> list;
    }
}