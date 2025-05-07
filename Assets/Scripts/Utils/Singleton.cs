using UnityEngine;

namespace Utils
{
    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>(true);
                    if (instance == null)
                    {
                        var go = new GameObject();
                        instance = go.AddComponent<T>();
                    }

                }
                return instance;
            }
        }

        [SerializeField] bool dontDestroyOnLoad;
        private void Awake()
        {
            if (instance == null)
            {
                instance = (T)this;
                if (dontDestroyOnLoad)
                {
                    DontDestroyOnLoad(instance);
                }
                Init();
            }
            else if (instance != this)
            {
                Debug.Log("SIngleton destroy: " + gameObject.name);
                Destroy(gameObject);
            }
            else
            {
                if (dontDestroyOnLoad)
                {
                    DontDestroyOnLoad(instance);
                }
            }
        }

        public virtual void Init() { }
    }

    public class Singleton<T> where T : Singleton<T>, new()
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new T();
                }
                return instance;
            }
        }
    }
}
