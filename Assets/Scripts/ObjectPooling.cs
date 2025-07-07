using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling : MonoBehaviour
{
    public static ObjectPooling Instance;

    public List<PoolItem> itemsToPool;

    private Dictionary<GameObject, List<GameObject>> pooledObjects;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        pooledObjects = new Dictionary<GameObject, List<GameObject>>();
        foreach (PoolItem item in itemsToPool)
        {
            if(item.objectToPool == null)
            {
                Debug.LogWarning("Object to pool is null, skipping pooling for this item.");
                continue;
            }

            List<GameObject> objectList = new List<GameObject>();
            for (int i = 0; i < item.amountToPool; i++)
            {
                GameObject obj = Instantiate(item.objectToPool, transform);
                obj.SetActive(false); // Initially inactive
                objectList.Add(obj);
            }
            pooledObjects[item.objectToPool] = objectList;
            Debug.Log($"Pooled {item.amountToPool} instances of {item.objectToPool.name}.");
        }
    }

    public GameObject GetPooledObject(GameObject prefab)
    {

        if (prefab == null)
        {
            Debug.LogWarning("Prefab is null, cannot get pooled object.");
            return null;
        }

        if (pooledObjects.TryGetValue(prefab, out List<GameObject> pool))
        {
            foreach (var obj in pool)
            {
                if (!obj.activeInHierarchy)
                {
                    return obj;
                }
            }

            // If no inactive object is found, instantiate a new one
            PoolItem item = itemsToPool.Find(i => i.objectToPool == prefab);

            if(item == null || !item.canGrow)
            {
                Debug.LogWarning($"No inactive object found in pool for {prefab.name} and cannot grow the pool.");
                return null;
            }
            GameObject newObj = Instantiate(prefab);
            newObj.SetActive(false);
            pool.Add(newObj);
            return newObj;
        }
        else
        {
            Debug.LogWarning($"No pool found for prefab: {prefab.name}");

        }

        return null;
    }

    public void ReturnToPool(GameObject obj)
    {
        if (obj != null)
        {
            obj.SetActive(false);
            ResetObject(obj);
        }
    }

    private void ResetObject(GameObject obj)
    {
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // Reset velocity
            rb.angularVelocity = 0f; // Reset angular velocity
        }

        //reset renderer if it exists (faded clouds)
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            Color color = renderer.material.color;
            color.a = 1f; // Reset alpha to fully opaque
            renderer.material.color = color;
        }

        Collider2D collider = obj.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true; // Ensure collider is enabled
        }

        if (renderer != null)
        {
            renderer.enabled = true; // Ensure renderer is enabled
        }

        // Reset any other components or properties as needed
        MoveCloud moveCloud = obj.GetComponent<MoveCloud>();
        if (moveCloud != null)
        {
            moveCloud.enabled = true; // Re-enable the MoveCloud script
        }
    }

    public void PrewarmPool(GameObject prefab, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject obj = GetPooledObject(prefab);
            if (obj != null)
            {
                ReturnToPool(obj);
            }
        }
    }

    public void ResetPool()
    {
        foreach (var pool in pooledObjects.Values)
        {
            foreach (var obj in pool)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                    ResetObject(obj);
                }
            }
        }
    }
}
