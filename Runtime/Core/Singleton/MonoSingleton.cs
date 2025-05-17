using UnityEngine;

namespace GrimTools.Runtime.Core
{
    public abstract class MonoSingleton<T> : MonoBehaviour, ISingleton where T : MonoSingleton<T>
    {
        #region Fields
        private static T _instance;

        private SingletonInitializationStatus _initializationStatus = SingletonInitializationStatus.None;
        #endregion

        #region Properties
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
#if UNITY_6000
                    _instance = FindAnyObjectByType<T>();
#else
                    _instance = FindObjectOfType<T>();
#endif
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(T).Name;
                        _instance = obj.AddComponent<T>();
                        _instance.OnMonoSingletonCreated();
                    }
                }
                return _instance;
            }
        }
        public virtual bool IsInitialized => this._initializationStatus == SingletonInitializationStatus.Initialized;
        #endregion

        #region Unity API 
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;

                // Initialize existing instance
                InitializeSingleton();
            }
            else
            {

                // Destory duplicates
                if (Application.isPlaying)
                {
                    Destroy(gameObject);
                }
                else
                {
                    DestroyImmediate(gameObject);
                }
            }
        }
        #endregion

        #region Protected Methods
        protected virtual void OnMonoSingletonCreated()
        {
            // Called when the singleton is created
        }
        protected virtual void OnInitializing()
        {

        }
        protected virtual void OnInitialized()
        {

        }
        #endregion

        #region Public Methods
        public void InitializeSingleton()
        {
            if (this._initializationStatus != SingletonInitializationStatus.None)
            {
                return;
            }

            this._initializationStatus = SingletonInitializationStatus.Initializing;
            OnInitializing();
            this._initializationStatus = SingletonInitializationStatus.Initialized;
            OnInitialized();
        }
        public void DestroySingleton()
        {
            throw new System.NotImplementedException();
        }
        public static void CreateInstance()
        {
            DestroyInstance();
            _instance = Instance;
        }
        public static void DestroyInstance()
        {
            if (_instance == null)
            {
                return;
            }

            _instance.DestroySingleton();
            _instance = default(T);
        }
        #endregion
    }
}
