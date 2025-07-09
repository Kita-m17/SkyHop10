using UnityEngine;

public class DestroyOutOfBounds : MonoBehaviour
{
    public float lowerBoundY = -15f; // Y position below which the object will be destroyed
    public bool usePlayerReference = true; // Whether to use the player's position to determine when to destroy the object
    public float offsetFromPlayer = 5f; // Offset from the player's position to determine when to destroy the object

    private Transform playerTransform; // Reference to the player's transform
    private bool isPooledObject = false; // Flag to check if this object is part of a pool
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isPooledObject = gameObject.CompareTag("Ground_Platform") || gameObject.CompareTag("Cloud_Platform");
        
    }

    // Update is called once per frame
    void Update()
    {
        float bottomOfScreenY = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;
        float cleanUpY = lowerBoundY;

        if (transform.position.y < cleanUpY)
        {
           CleanUpObject(); // Destroy the object if it goes out of bounds
        }
    }

    void CleanUpObject()
    {
        // Notify the spawn manager BEFORE destroying
        SpawnManager2 spawnManager = FindObjectOfType<SpawnManager2>();
        if (spawnManager != null)
        {
            spawnManager.RemovePlatform(this.gameObject);
        }

        if (isPooledObject && ObjectPooling.Instance != null)
        {
            // Return to pool instead of destroying
            Destroy(gameObject); // Uncomment this line if you want to use object pooling
            //ObjectPooling.Instance.ReturnToPool(gameObject);
        }
        else
        {
            // Destroy normally for non-pooled objects
            Destroy(gameObject);
        }
    }

    public void ForceCleanup()
    {
        CleanUpObject();
    }
}
