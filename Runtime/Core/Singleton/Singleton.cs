using UnityEngine;

namespace GrimTools.Runtime.Core
{
    public enum SingletonInitializationStatus
    {
        None,
        Initializing,
        Initialized
    }
    public interface ISingleton
    {
        public void InitializeSingleton();
        public void DestroySingleton();
    }


    public class Singleton<T> : ISingleton where T : Singleton<T>, new()
    {
        #region Fields
        private static T _instance;

        private static object _lock = new object();

        private SingletonInitializationStatus initializationStatus = SingletonInitializationStatus.None;
        #endregion

        #region Properties
        public virtual bool IsInitialized => this.initializationStatus == SingletonInitializationStatus.Initialized;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new T();
                            _instance.InitializeSingleton();
                        }
                    }
                }

                return _instance;

            }
        }
        #endregion

        #region Protected Methods
        protected virtual void OnInitializing()
        {

        }
        protected virtual void OnInitialized()
        {

        }
        #endregion

        #region Public Methods

        public virtual void InitializeSingleton()
        {
            if (this.initializationStatus != SingletonInitializationStatus.None)
            {
                return;
            }
            this.initializationStatus = SingletonInitializationStatus.Initializing;
            OnInitializing();
            this.initializationStatus = SingletonInitializationStatus.Initialized;
            OnInitialized();
        }

        public virtual void DestroySingleton()
        {

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