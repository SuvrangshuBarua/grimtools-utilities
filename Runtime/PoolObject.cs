using UnityEngine;
using UnityEngine.Pool;

namespace GrimTools.Runtime
{
    public class PoolObject : MonoBehaviour
    {
        private ObjectPool<GameObject> pool = null;

        public void SetPool(ObjectPool<GameObject> pool)
        {
            this.pool = pool;
        }

        public void ReturnToPool()
        {
            if (pool != null)
            {
                pool.Release(gameObject);
            }
            else
            {
                Debug.LogWarning("Pool is null. Cannot return object to pool.");
                Destroy(gameObject);

            }
        }
    }
}

