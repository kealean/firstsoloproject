using UnityEngine;

namespace script.Lib {
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour {
        private static T _instance;

        public static T Instance {
            get {
                if(_instance == null) _instance = FindFirstObjectByType<T>();

                if (_instance is null) {
                    var objectName = typeof(T).ToString();
                    var instanceGo = new GameObject(objectName);
                    _instance = instanceGo.AddComponent<T>();
                }
                
                return _instance;
            }
        }

        protected virtual void Awake() {
            var managers = FindObjectsByType<T>(FindObjectsSortMode.None);
            if(managers.Length > 1)
                Destroy(gameObject);
        }

        protected virtual void OnDestroy() {
            if(_instance == this)
                _instance = null;
        }
    }
}