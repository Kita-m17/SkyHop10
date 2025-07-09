using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class SpawnManager : MonoBehaviour
{
    #region Spawn Settings
    public GameObject[] platforms;
    public GameObject[] clouds;
    public Transform player;
    #endregion

    #region Spawn Parameters
    public float spawnDistance = 5f;
    private float spawnY = 7f;
    private float spawnInterval = 2f;
    private float startDelay = 2f;
    public float verticalSpacing = 2f; // Distance between platform layers
    public float maxHorizontalJump = 3f; // test in scene view how far player can jump
    #endregion

    #region Cleanup Parameters
    public float cleanupDistance = 20f; // Distance from the player to start cleaning up platforms
    #endregion

    public GameObject[] jewelPrefabs;
    public float minSpawnDelay = 1f, maxSpawnDelay = 3f;
    public int totalJewelsToSpawn = 15;

    private int jewelsSpawned = 0;
    public float spawnOffsetY = 1f; // Offset above the platform to spawn jewels

    public GameObject coinPrefab;
    public float coinChance = 0.3f;

    // Track recently spawned platforms for jewel spawning
    private System.Collections.Generic.List<GameObject> recentPlatforms = new System.Collections.Generic.List<GameObject>();

    #region Game Speed Parameters
    public float minSpawnInterval = 1f, maxSpawnInterval = 3f;
    public float spawnSpeed = 1f; // Speed at which platforms are spawned

    private float currentSpawnInterval;
    private float nextSpawnY = 0f; // Y position for the next platform spawn
    private PlayerController playerController;
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Initialize();
    }

    void Initialize()
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

        currentSpawnInterval = maxSpawnInterval;
        StartCoroutine(WaitForPoolAndStartSpawning());
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
        if (player != null)
        {
            nextSpawnY = player.position.y + spawnY;
        }
        else
        {
            nextSpawnY = spawnY; // Fallback if player is not set
        }

        //InvokeRepeating("SpawnPlatform", startDelay, spawnInterval);
        //InvokeRepeating("SpawnRandomPlatform", 3f, 5f);
        StartCoroutine(SpawnJewelRoutine());
    }

    void SpawnPlatform()
    {

        if (playerController != null && playerController.gameOver) return; // Don't spawn if game is over

        if (platforms == null || platforms.Length == 0)
        {
            return;
        }
        // Clamp max increase in Y
        verticalSpacing = Mathf.Clamp(verticalSpacing + 0.01f * Time.timeSinceLevelLoad, 2f, 4f);

        //calculate next spawn position
        float newY = nextSpawnY + verticalSpacing;


        float playerX = player != null ? player.position.x : 0f;

        float maxGap = maxHorizontalJump;

        float safeX = Mathf.Clamp(playerX + Random.Range(-maxGap * 0.5f, maxGap * 0.5f), -spawnDistance, spawnDistance);

        SpawnSinglePlatform(safeX, newY); // Spawn the first platform at a safe X position

        float gapBetweenPlatforms = 3f;
        float baseX = Mathf.Clamp(playerX + Random.Range(-maxHorizontalJump, maxHorizontalJump), -spawnDistance, spawnDistance);
        float offset = Random.Range(gapBetweenPlatforms / 2f, gapBetweenPlatforms);

        float[] platformPositions = new float[2]
        {
            Mathf.Clamp(baseX - offset, -spawnDistance, spawnDistance),
            Mathf.Clamp(baseX + offset, -spawnDistance, spawnDistance)
        };

        foreach (float newX in platformPositions)
        {
            SpawnSinglePlatform(newX, newY);
        }
        //==============================================

        //spawn a third platform far from the thers
        float minGap = 1.5f;
        

        // Try to spawn 1-2 additional platforms with controlled gaps
        int additionalPlatforms = Random.Range(1, 3);
        for (int i = 0; i < additionalPlatforms; i++)
        {
            float candidateX;
            bool validPosition = false;
            int attempts = 0;

            do
            {
                candidateX = Random.Range(-spawnDistance, spawnDistance);
                // Check if this position is reachable from the safe platform or player
                float distFromSafe = Mathf.Abs(candidateX - safeX);
                float distFromPlayer = Mathf.Abs(candidateX - playerX);

                validPosition = (distFromSafe >= minGap && distFromSafe <= maxGap) ||
                               (distFromPlayer >= minGap && distFromPlayer <= maxGap);
                attempts++;
            } while (!validPosition && attempts < 10);

            if (validPosition)
            {
                SpawnSinglePlatform(candidateX, newY);
            }
        }

        //--------------------------------------------------------


        if (Random.value < 0.5f) // 50% chance to spawn a cloud
        {
            SpawnCloud(newY + Random.Range(1f, 2f));
        }

        nextSpawnY = newY;
    }

    void SpawnSinglePlatform(float x, float y)
    {
        int randomIndex = Random.Range(0, platforms.Length);
        GameObject prefab = platforms[randomIndex];

        if (prefab == null) return;

        GameObject platform = ObjectPooling.Instance.GetPooledObject(prefab);
        placePlatform(x, y, prefab, platform);
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

    void SpawnSinglePlatform(int newX, int newY)
    {
        int randomIndex = Random.Range(0, platforms.Length);
        GameObject prefab = platforms[randomIndex];
        if (prefab == null)
        {
            Debug.LogWarning("Selected platform prefab is null.");
            return;
        }
        // Get a pooled object from the ObjectPooling system
        GameObject platform = ObjectPooling.Instance.GetPooledObject(prefab);

        placePlatform(newX, newY, prefab, platform);
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

        float randomX = Random.Range(-spawnDistance, spawnDistance);
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

            // Add to recent platforms list for jewel spawning
            if (!recentPlatforms.Contains(platform))
            {
                recentPlatforms.Add(platform);
                // Keep only the last 5 platforms for jewel spawning
                if (recentPlatforms.Count > 5)
                {
                    recentPlatforms.RemoveAt(0);
                }
            }
        }

        VerticalPlatformMover mover = platform.GetComponent<VerticalPlatformMover>();
        if (mover != null)
        {
            bool shouldMove = Random.value < 0.1f;
            mover.movementType = shouldMove
                ? (VerticalPlatformMover.MovementType.Horizontal)
                : VerticalPlatformMover.MovementType.None;

            if (shouldMove)
            {
                mover.ResetPlatform();
                SpriteRenderer sr = platform.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = Color.cyan;
            }
        }


        SpawnCoin(platform.transform); // Spawn a coin on the platform
    }

    private void OnDisable()
    {
        //isSpawning = false;
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

        int randomIndex = Random.Range(0, jewelPrefabs.Length);
        GameObject jewelPrefab = jewelPrefabs[randomIndex];
        GameObject jewel = ObjectPooling.Instance.GetPooledObject(jewelPrefab);

        if (jewel != null)
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

    #region Spawn Coin

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