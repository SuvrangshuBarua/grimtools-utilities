using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace GrimTools.Runtime
{
    public class ObjectPool : MonoBehaviour
    {
        public static ObjectPool Instance
        {
            get;
            private set;
        }

        private Dictionary<GameObject, ObjectPool<GameObject>> m_Pools = new Dictionary<GameObject, ObjectPool<GameObject>>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        public GameObject GetObject(GameObject prefab)
        {
            if (prefab == null)
            {
                Debug.LogError("Prefab is null. Cannot get object from pool.");
                return null;
            }

            if (!m_Pools.TryGetValue(prefab, out ObjectPool<GameObject> pool))
            {
                pool = CreatePool(prefab);
                m_Pools[prefab] = pool;
            }

            return pool.Get();
        }
        public void ReturnToPool(GameObject obj)
        {
            if (obj == null)
            {
                Debug.LogError("Object is null. Cannot return to pool.");
                return;
            }
            if (obj.TryGetComponent<PoolObject>(out var poolObject))
            {
                poolObject.ReturnToPool();
            }
            else
            {
                Debug.LogWarning($"Object {obj.name} lacks PoolObject componant. Destroying it");
                Destroy(obj);
            }
        }
        public void PrewarnPool(GameObject prefab, int count)
        {
            if (prefab == null)
            {
                Debug.LogError("Prefab is null. Cannot prewarm pool.");
                return;
            }

            if (!m_Pools.TryGetValue(prefab, out ObjectPool<GameObject> pool))
            {
                pool = CreatePool(prefab);
                m_Pools[prefab] = pool;
            }

            var objects = new List<GameObject>(count);
            for (int i = 0; i < count; i++)
            {
                var obj = pool.Get();
                objects.Add(obj);
                pool.Release(obj);
            }
        }
        private ObjectPool<GameObject> CreatePool(GameObject prefab)
        {
            ObjectPool<GameObject> pool = null;

            pool = new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    GameObject obj = Instantiate(prefab);
                    PoolObject poolObject = obj.AddComponent<PoolObject>();
                    poolObject.SetPool(pool);
                    return obj;
                },
                actionOnGet: obj => obj.SetActive(true),
                actionOnRelease: obj => obj.SetActive(false),
                actionOnDestroy: obj => Destroy(obj),
                defaultCapacity: 10,
                maxSize: 100
            );

            return pool;
        }
    }
}
