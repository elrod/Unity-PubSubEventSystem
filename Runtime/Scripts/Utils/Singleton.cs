using UnityEngine;

namespace com.elrod.pubsubeventsystem
{
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        [SerializeField]
        private bool _DestroyOnLoad = true;
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (!_instance)
                {
                    GameObject go = new GameObject(typeof(T).Name);
                    _instance = go.AddComponent<T>();
                }
                return _instance;           
            }
        }

        private void Awake()
        {
            if (!_instance)
            {
                _instance = this as T;
                InternalAwake();                
            }
            else
            {
                Debug.LogWarning($"[{nameof(Singleton<T>)}] Multiple singleton instances detected!");
                Destroy(_instance);
                _instance = this as T;
                InternalAwake();
            }
        }

        private void InternalAwake()
        {
            if (_DestroyOnLoad) DontDestroyOnLoad(gameObject);
            OnAwake();
        }
        
        protected virtual void OnAwake() {}
        
    }   
}
