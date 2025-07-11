using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class SpawnManager2 : MonoBehaviour
{
    public GameObject[] platforms;
    public GameObject[] clouds;
    public GameObject gems;
    public GameObject coinPrefab;
    public GameObject mushroomPrefab;
    public float mushroomSpawnChance = 0.1f; // 20% chance to spawn a mushroom

    public PlayerController playerController; // Reference to the PlayerController script
    public Transform player;
    private float xRange = 14f; // Range for spawning platforms on the x-axis
    private float spawnPosY = 9f;

    public float startDelay = 2f; // Delay before the first spawn
    public float spawnInterval = 1.5f; // Interval between spawns

    private float verticalSpacing = 2f;
    private float fixedSpawnY;
    private float lastPlayerY;

    public int maxPlatforms = 30; // cap
    private Queue<GameObject> activePlatforms = new Queue<GameObject>();

    private bool canSpawn = true;
    private float spawnCooldown = 0f;
    private float spawnCooldownTime = 1f; // Minimum time between spawns

    private float coinChance = 0.3f; // 30% chance to spawn a coin

    public int totalJewelsToSpawn = 15;
    public float minSpawnDelay = 1f, maxSpawnDelay = 3f;
    private int jewelsSpawned = 0;
    private List<GameObject> recentPlatforms = new List<GameObject>();
    public float spawnOffsetY = 1f; // make sure this exists if using it

    public GameObject fan;
    private float fanSpawnChance = 0.1f; // 10% chance to spawn a fan
    
    void Start()
    {
        Initialize();
    }

    void Update()
    {
        if(!GameManager.Instance.isGameActive) return;
    }

    public void Initialize()
    {
        // Find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        // Get player controller
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
        }

        InvokeRepeating(nameof(SpawnPlatform), startDelay, spawnInterval);
        InvokeRepeating(nameof(SpawnRandomPlatform), 4f, 5f); // Random platform spawner
        InvokeRepeating(nameof(SpawnPowerup), 5f, 8f); // Powerup spawner
        StartCoroutine(SpawnJewelRoutine());
        InvokeRepeating(nameof(SpawnFan), 6f, spawnInterval*4); // Fan spawner
    }
    void SpawnPlatform()
    {
        if (!GameManager.Instance.isGameActive) return;

        if (playerController != null && playerController.gameOver) return; // Don't spawn if game is over

        if (platforms == null || platforms.Length == 0)
        {
            return;
        }

        // Additional safety check - don't spawn if too close to player
        float playerY = player.position.y;
        float playerX = playerController != null ? playerController.transform.position.x : 0f;

        float newY = spawnPosY + 2;

        int platformCount = Random.Range(2, 4); // spawn 2 or 3 platforms

        List<float> usedX = new List<float>();

        float minGap = 3f; // Minimum gap between platforms
        float maxGap = 4.5f; // Maximum gap between platforms

        float safeX = Mathf.Clamp(playerX + Random.Range(-maxGap * 0.5f, maxGap * 0.5f), -xRange, xRange);

        usedX.Add(safeX); // Add the safe platform position
        SpawnSinglePlatform(safeX, newY); // Spawn the first platform at a safe X position
        bool spawned = true;

        int attempts = 0;
        float[] xRegions;

        if (platformCount == 2)
            xRegions = new float[] { -xRange * 0.5f, xRange * 0.5f };
        else // 3
            xRegions = new float[] { -xRange * 0.75f, 0, xRange * 0.75f };

        usedX.Clear();

        foreach (float baseX in xRegions)
        {
            float jitter = Random.Range(-1.5f, 1.5f); // small randomness
            float x = Mathf.Clamp(baseX + jitter, -xRange, xRange);
            usedX.Add(x);
            SpawnSinglePlatform(x, newY);
        }


        if (Random.value < 0.5f) // 50% chance to spawn a cloud
        {
            SpawnCloud(newY + Random.Range(1f, 3f));
        }

        spawnPosY += verticalSpacing;
        
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
        float x = Mathf.Clamp(playerX + Random.Range(-4, 4), -xRange, xRange);

        Vector3 pos = new Vector3(x, y, 0);

        Instantiate(prefab, pos, prefab.transform.rotation);
    }

    void SpawnSinglePlatform(float x, float y)
    {
        int randomIndex = Random.Range(0, platforms.Length);
        GameObject prefab = platforms[randomIndex];

        if (prefab == null) return;

        GameObject platform = Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity);
        activePlatforms.Enqueue(platform);

        VerticalPlatformMover mover = platform.GetComponent<VerticalPlatformMover>();
        if (mover != null)
        {
            float random = Random.value;

            if (random < 0.1f)
            {
                // 10% chance to move horizontally
                mover.movementType = VerticalPlatformMover.MovementType.Horizontal;
            }
            else
            {
                mover.movementType = VerticalPlatformMover.MovementType.None;
            }

            if (mover.movementType != VerticalPlatformMover.MovementType.None)
            {
                mover.ResetPlatform();

                SpriteRenderer sr = platform.GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.color = Color.cyan; // visually mark it
            }
        }

        SpawnCoin(platform.transform); // Spawn a coin on the platform

        if (!recentPlatforms.Contains(platform))
        {
            recentPlatforms.Add(platform);
            if (recentPlatforms.Count > 5) // Optional cap
                recentPlatforms.RemoveAt(0);
        }
    }

    void SpawnRandomPlatform(float y)
    {
        if (!GameManager.Instance.isGameActive) return;

        if (platforms == null || platforms.Length == 0) return;

        int randomIndex = Random.Range(0, platforms.Length);
        GameObject prefab = platforms[randomIndex];
        if (prefab == null)
        {
            return;
        }

        float randomX = Random.Range(-xRange, xRange);
        float randomY;
        if (player != null)
        {
            bool spawnBelow = Random.value < 0.5f; // 50% chance to spawn below the player
            if (spawnBelow)
            {
                randomY = player.position.y - Random.Range(1f, 2.5f); // Spawn below the player
            }
            else
            {
                randomY = player.position.y + Random.Range(1f, 2.5f); // Spawn above the player
            }
        }
        else
        {
            randomY = Random.Range(4f, 15f); // fallback
        }

        SpawnSinglePlatform(randomX, randomY);

    }

    public void RemovePlatform(GameObject platform)
    {
        if (activePlatforms.Contains(platform))
        {
            Queue<GameObject> updatedQueue = new Queue<GameObject>();
            foreach (var p in activePlatforms)
            {
                if (p != platform)
                    updatedQueue.Enqueue(p);
            }
            activePlatforms = updatedQueue;
        }
    }

    public void SpawnPowerup()
    {
        if (mushroomPrefab == null)
        {
            return;
        }

        if (Random.value < mushroomSpawnChance)
        {
            float randomX = Random.Range(-xRange, xRange);
            float y = 9f;
            GameObject powerUp = Instantiate(mushroomPrefab, new Vector3(randomX, y, 0), Quaternion.identity);

            PowerUpManager powerUpManager = powerUp.GetComponent<PowerUpManager>();
            if (powerUpManager != null)
            {
                // Randomly select one of the 4 types (excluding None)
                int randomIndex = Random.Range(1, 5); // 1 to 4 inclusive
                powerUpManager.powerUpType = (PowerUpManager.PowerUpType)randomIndex;

            }
        }
    }

    public void SpawnFan()
    {
        if (fan == null)
        {
            return;
        }

        if (Random.value < fanSpawnChance )
        {
            float randomX = Random.Range(-xRange, xRange);
            float y = 9f;
            Instantiate(fan, new Vector3(randomX, y, 0), Quaternion.identity);

        }
    }

    public void SpawnCoin(Transform parentPlatform)
    {
        if (Random.value < coinChance)
        {
            Vector3 spawnPosition = parentPlatform.position + Vector3.up * 0.8f; // Spawn above the platfor8

            Collider2D[] hits = Physics2D.OverlapCircleAll(spawnPosition, 0.5f);
            foreach (var hit in hits)
            {
                if (hit != null && (hit.CompareTag("Gem") || hit.CompareTag("Coin")))
                {
                    return; // skip this coin spawn
                }
            }

            Instantiate(coinPrefab, spawnPosition, Quaternion.identity, parentPlatform.transform);
        }
    }

    private void OnDisable()
    {
        //isSpawning = false;
        CancelInvoke("SpawnPlatform"); // Stop spawning platforms when this script is disabled
        CancelInvoke("SpawnRandomPlatform"); // Stop spawning random platforms
        StopAllCoroutines(); // Stop all coroutines including jewel spawning
    }

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
        GameObject lastPlatform = GetRandomRecentPlatform();
        if (lastPlatform == null || !lastPlatform.activeInHierarchy)
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
                return; // skip this spawn
            }
        }

        Transform g = Instantiate(gems, worldSpawnPos, Quaternion.identity, lastPlatform.transform).transform;
        g.localRotation = Quaternion.identity;
        g.localScale = Vector3.one; 
        jewelsSpawned++;
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

    GameObject GetRandomRecentPlatform()
    {
        // Clean up inactive platforms from the list
        recentPlatforms.RemoveAll(platform => platform == null || !platform.activeInHierarchy);

        if (recentPlatforms.Count == 0)
        {
            return FindLastPlatform(); // Fallback to old method
        }

        return recentPlatforms[Random.Range(0, recentPlatforms.Count)];
    }
    #endregion region



}
