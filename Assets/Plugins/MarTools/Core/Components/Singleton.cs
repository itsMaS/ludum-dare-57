using System;
using UnityEngine;

namespace MarTools
{
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        public virtual bool AddToDontDestroyOnLoad { get; }


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void Cleanup()
        {
            _instance = null;
        }

        public static T Instance
        {
            get
            {
                if(!_instance)
                {
                    _instance = FindFirstObjectByType<T>(FindObjectsInactive.Include);
                    if(_instance)
                    {
                        _instance.Initialize();
                    }
                }
                if(!_instance)
                {
                    Debug.LogWarning("Not a single instance of the singleton class exists in the scene");
                }

                return _instance;
            }
        }

        private void Awake()
        {
            if(_instance && _instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this as T;
                Initialize();
            }
        }

        protected virtual void Initialize()
        {
            if(AddToDontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private static T _instance;
    }
}
