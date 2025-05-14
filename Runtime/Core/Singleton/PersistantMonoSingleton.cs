using UnityEngine;

namespace GrimTools.Runtime.Core
{
    public class PersistantMonoSingleton<T> : MonoSingleton<T> where T : MonoSingleton<T>
    {

        [SerializeField] private bool UnparentOnAwake = true;

        #region Protected Methods

        protected override void OnInitializing()
        {
            if (UnparentOnAwake)
            {
                transform.SetParent(null);
            }
            base.OnInitializing();
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        #endregion
    }
}