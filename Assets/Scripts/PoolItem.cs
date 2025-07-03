using UnityEngine;

[System.Serializable]
public class PoolItem
{

    public GameObject objectToPool; // The prefab to pool
    public int amountToPool; // The number of instances to pool
    public bool canGrow = true; // Whether the pool can grow beyond the initial amount
}
