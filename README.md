# GrimTools Utilities

A Unity Utilities package offers an object pooling system using `UnityEngine.Pool` for efficient GameObject management.

## Installation

Add the package via Unity Package Manager:

1. Open Window > Package Manager.
2. Click the "+" button and select "Add package from git URL".
3. Enter: `https://github.com/SuvrangshuBarua/grimtools-utilities.git#v1.0.1`

## Object Pool Usage

```csharp
using GrimTools.Runtime;

// Get an object
GameObject prefab = Resources.Load<GameObject>("MyPrefab");
GameObject obj = ObjectPool.Instance.GetObject(prefab);

// Return it
obj.GetComponent<PoolObject>().ReturnToPool();
// Or: ObjectPool.Instance.ReturnToPool(obj);

