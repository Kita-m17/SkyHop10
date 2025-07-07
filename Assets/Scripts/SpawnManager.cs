using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class SpawnManager : MonoBehaviour
{
    #region Spawn Settings
    public GameObject[] platforms;
    public GameObject[] clouds;
    public Transform player ;
    #endregion

    #region Spawn Parameters
    public float spawnDistance = 7f;
    private float spawnY = 8;
    private float spawnInterval = 3f;
    private float startDelay = 3f;
    public float verticalSpacing = 3.8f; // Distance between platform layers
    
    public float maxHorizontalJump = 4f; // test in scene view how far player can jump
    #endregion

    #region Cleanup Parameters
    public float cleanupDistance = 20f; // Distance from the player to start cleaning up platforms
    #endregion

    
    private float nextSpawnY = 0f; // Y position for the next platform spawn
    private PlayerController playerController;
    
    
    public GameObject[] jewelPrefabs;
    public float minSpawnDelay = 5f, maxSpawnDelay =12f;
    public int totalJewelsToSpawn = 7;

    private int jewelsSpawned = 0;
    private float lastPlayformY = 0f;
    public float spawnOffsetY = 1f; // Offset above the platform to spawn jewels
    
    public GameObject coinPrefab;
    public float coinChance = 0.3f;

    #region Game Speed Parameters
    public float minSpawnInterval = 1f, maxSpawnInterval = 4f;
    public float spawnSpeed = 1f; // Speed at which platforms are spawned
    private float currentSpawnInterval;
    private float spawnTimeElapsed = 0f; // Time elapsed since the last platform spawn

    public float gameSpeedIncreaseRate = 0.1f;
    public float maxGameSpeed = 5f; // Maximum speed limit for spawning platforms
    
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get reference to player controller
        if (player == null)
        {
            player = GameObject.Find("Player").transform;
        }

        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
        }

        currentSpawnInterval = maxSpawnInterval; // Initialize with max spawn interval
        StartCoroutine(WaitForPoolAndStartSpawning());
        StartCoroutine(SpawnJewelRoutine());
    }

    #region Platform spawner
    IEnumerator WaitForPoolAndStartSpawning()
    {
        while (ObjectPooling.Instance == null)
        {
            yield return null;
        }

        yield return null; //wait for one frame to ensure ObjectPooling is ready

        //initialise spawn position
        if(player != null)
        {
            nextSpawnY = player.position.y + spawnY;
        }
        else
        {
            nextSpawnY = spawnY; // Fallback if player is not set
        }

        InvokeRepeating("SpawnPlatform", startDelay, spawnInterval);
        InvokeRepeating("SpawnRandomPlatform", 4f, 6f);
    }

    void SpawnPlatform()
    {

        if(playerController != null && playerController.gameOver) return; // Don't spawn if game is over

        if(platforms == null || platforms.Length == 0)
        {
            Debug.LogWarning("No platforms available to spawn.");
            return;
        }

        //calculate next spawn position
        float newY = nextSpawnY + verticalSpacing;
        float playerX = player != null ? player.position.x : 0f;

        float gapBetweenPlatforms = 4f;
        float baseX = Mathf.Clamp(playerX + Random.Range(-maxHorizontalJump, maxHorizontalJump), -spawnDistance, spawnDistance);
        float offset = Random.Range(gapBetweenPlatforms / 2f, gapBetweenPlatforms);

        float[] platformPositions = new float[2]
        {
            Mathf.Clamp(baseX - offset, -spawnDistance, spawnDistance),
            Mathf.Clamp(baseX + offset, -spawnDistance, spawnDistance)
        };

        foreach(float newX in platformPositions)
        {
            int randomIndex = Random.Range(0, platforms.Length);
            GameObject prefab = platforms[randomIndex];
            if (prefab == null)
            {
                Debug.LogWarning("Selected platform prefab is null.");
                continue;
            }
            // Get a pooled object from the ObjectPooling system
            GameObject platform = ObjectPooling.Instance.GetPooledObject(prefab);

            placePlatform(newX, newY, prefab, platform);
        }

        //spawn a third platform far from the thers
        float minGap = 4f;
        bool placed = false;
        int maxTries = 10;

        for(int i = 0; i< maxTries && !placed; i++)
        {
            float candidateX = Random.Range(-spawnDistance, spawnDistance);
            float dist1 = Mathf.Abs(candidateX - platformPositions[0]);
            float dist2 = Mathf.Abs(candidateX - platformPositions[1]);

            if(dist1 >  minGap && dist2 > minGap)
            {
                int randomIndex = Random.Range(0, platforms.Length);
                GameObject prefab = platforms[randomIndex];
                GameObject platform = ObjectPooling.Instance.GetPooledObject(prefab);
                placePlatform(candidateX, newY, prefab, platform);
                placed = true;
            }
        }

        if (Random.value < 0.5f) // 50% chance to spawn a cloud
        {
            SpawnCloud(newY + Random.Range(1f, 2f));
        }

        nextSpawnY = newY;
    }

    void SpawnCloud(float y)
    {

        if (clouds == null || clouds.Length == 0)
        {
            return;
        }

        int randomIndex = Random.Range(0, clouds.Length);
        GameObject prefab = clouds[randomIndex];
        if (prefab == null)
        {
            return;
        }
        
        float playerX = player != null ? player.position.x : 0f;
        // Calculate a random horizontal position within the spawn distance
        float x = Mathf.Clamp(playerX + Random.Range(-maxHorizontalJump, maxHorizontalJump), -spawnDistance, spawnDistance);
        
        Vector3 pos = new Vector3(x, y, 0);
        
        // Get a pooled object from the ObjectPooling system
        GameObject cloud = ObjectPooling.Instance.GetPooledObject(prefab);

        if (cloud != null)
        {
            cloud.transform.position = pos;
            cloud.transform.rotation = clouds[randomIndex].transform.rotation;
            cloud.SetActive(true);
        }
    }

    void SpawnRandomPlatform(float y)
    {
        if (platforms == null || platforms.Length == 0) return;

        int randomIndex = Random.Range(0, platforms.Length);
        GameObject prefab = platforms[randomIndex];
        if(prefab == null)
        {
            return;
        }

        float randomX = Random.Range(-spawnDistance, spawnDistance);
        float randomY;
        if (player != null)
        {
            bool spawnBelow = Random.value < 0.5f; // 50% chance to spawn below the player
            if (spawnBelow)
            {
                randomY = player.position.y - Random.Range(1f, 3f); // Spawn below the player
            }
            else
            {
                randomY = player.position.y + Random.Range(1f, 3f); // Spawn above the player
            }
        }
        else
        {
            randomY = Random.Range(5f, 15f); // fallback
        }

        GameObject platform = ObjectPooling.Instance.GetPooledObject(prefab);
        
        placePlatform(randomX, randomY, prefab, platform);

        // Optional: Visual tweak to differentiate
        SpriteRenderer sr = platform.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.yellow; // Golden platform
        }
    }

    void placePlatform(float x, float y, GameObject prefab, GameObject platform)
    {
        Vector3 pos = new Vector3(x, y, 0);
        if (platform != null)
        {
            platform.transform.position = pos;
            platform.transform.rotation = prefab.transform.rotation;
            platform.SetActive(true);
        }

        VerticalPlatformMover mover = platform.GetComponent<VerticalPlatformMover>();
        if (mover != null)
        {
            mover.movementType = Random.value < 0.1f
                ? (Random.value < 0.5f ? VerticalPlatformMover.MovementType.Horizontal : VerticalPlatformMover.MovementType.Vertical)
                : VerticalPlatformMover.MovementType.None;
        }

        SpawnCoin(platform.transform); // Spawn a coin on the platform
    }

    private void OnDisable()
    {
        CancelInvoke("SpawnPlatform"); // Stop spawning platforms when this script is disabled
        CancelInvoke("SpawnRandomPlatform"); // Stop spawning random platforms
        StopAllCoroutines(); // Stop all coroutines including jewel spawning
    }

    #endregion

    #region Jewel Spawner
    
    IEnumerator SpawnJewelRoutine()
    {
        while (jewelsSpawned < totalJewelsToSpawn)
        {
            float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(delay);
            TrySpawnJewel();
        }
    }

    void TrySpawnJewel()
    {
        if (!GameManager.Instance.isGameActive)
        {
            OnDisable();
            return;
        }

        //use the most recent platform to spawn the jewel
        GameObject lastPlatform = FindLastPlatform();
        if(lastPlatform == null || !lastPlatform.activeInHierarchy)
        {
            return;
        }

        if (!lastPlatform.CompareTag("Ground_Platform"))
        {
            return;
        }

        Vector3 localSpawnOffset = new Vector3(0f, spawnOffsetY, 0f);
        Vector3 worldSpawnPos = lastPlatform.transform.position + localSpawnOffset;
        //Vector3 spawnPos = lastPlatform.transform.position + Vector3.up * spawnOffsetY;

        Collider2D[] hits = Physics2D.OverlapCircleAll(worldSpawnPos, 0.5f);
        foreach (var hit in hits)
        {
            if (hit != null && (hit.CompareTag("Gem") || hit.CompareTag("Coin")))
            {
                return; // skip this coin spawn
            }
        }

        int randomIndex = Random.Range(0, jewelPrefabs.Length);
        GameObject jewelPrefab = jewelPrefabs[randomIndex];
        GameObject jewel = ObjectPooling.Instance.GetPooledObject(jewelPrefab);

        if(jewel != null)
        {
            jewel.transform.SetParent(lastPlatform.transform, false);
            jewel.transform.localPosition = localSpawnOffset;
            jewel.transform.localRotation = Quaternion.identity; // Reset rotation
            jewel.transform.localScale = Vector3.one; // Reset scale
            
            jewel.SetActive(true);
            jewelsSpawned++;
        }
    }

    GameObject FindLastPlatform()
    {
        GameObject[] platforms = GameObject.FindGameObjectsWithTag("Ground_Platform");

        GameObject highest = null;
        float highestY = float.MinValue;
        foreach (GameObject platform in platforms)
        {
            if (platform.activeInHierarchy && platform.transform.position.y > highestY)
            {
                highestY = platform.transform.position.y;
                highest = platform;
            }
        }

        return highest;
    }
    #endregion region

    #region Spawn Coin
    
    public void SpawnCoin(Transform parentPlatform)
    {
        if (Random.value < coinChance)
        {
            Vector3 spawnPosition = parentPlatform.position + Vector3.up * 0.8f; // Spawn above the platfor8

            Collider2D[] hits = Physics2D.OverlapCircleAll(spawnPosition, 0.5f);
            foreach (var hit in hits)
            {
                if (hit != null &&  (hit.CompareTag("Gem") || hit.CompareTag("Coin")))
                {
                    return; // skip this coin spawn
                }
            }
            GameObject coin = ObjectPooling.Instance.GetPooledObject(coinPrefab);
            if (coin != null)
            {
                
                coin.transform.position = spawnPosition;
                coin.transform.rotation = Quaternion.identity; // Reset rotation
                coin.transform.SetParent(parentPlatform);
                coin.SetActive(true);
            }
        }
    }
    #endregion
}
