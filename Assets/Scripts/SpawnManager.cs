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
    public float spawnDistance = 8f;
    private float spawnY = 5;
    private float spawnInterval = 2f;
    private float startDelay = 2f;
    public float verticalSpacing = 2f; // Distance between platform layers
    
    public float maxHorizontalJump = 6f; // test in scene view how far player can jump
    #endregion

    #region Cleanup Parameters
    public float cleanupDistance = 20f; // Distance from the player to start cleaning up platforms
    #endregion

    private float nextSpawnY = 0f; // Y position for the next platform spawn
    private PlayerController playerController;


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
        StartCoroutine(WaitForPoolAndStartSpawning());

        
    }

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
    }

    void SpawnPlatform()
    {

        if(playerController != null && playerController.gameOver) return; // Don't spawn if game is over

        if(platforms == null || platforms.Length == 0)
        {
            Debug.LogWarning("No platforms available to spawn.");
            return;
        }

        /**
        int randomIndex = Random.Range(0, platforms.Length);
        GameObject prefab = platforms[randomIndex];
        if (prefab == null)
        {
            Debug.LogWarning("Selected platform prefab is null.");
            return;
        }
        **/

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

        if (Random.value < 0.3f) // 30% chance
        {
            SpawnRandomPlatform(newY);
        }

        nextSpawnY = newY;
    }

    void SpawnCloud(float y)
    {

        if (clouds == null || clouds.Length == 0)
        {
            Debug.LogWarning("No clouds available to spawn.");
            return;
        }

        int randomIndex = Random.Range(0, clouds.Length);
        GameObject prefab = clouds[randomIndex];
        if (prefab == null)
        {
            Debug.LogWarning("Selected cloud prefab is null.");
            return;
        }
        
        float playerX = player != null ? player.position.x : 0f;
        // Calculate a random horizontal position within the spawn distance
        float x = Mathf.Clamp(playerX + Random.Range(-maxHorizontalJump, maxHorizontalJump), -spawnDistance, spawnDistance);
        //float x = Mathf.Clamp(Random.Range(-spawnDistance, spawnDistance), -maxHorizontalJump, maxHorizontalJump);
        
        Vector3 pos = new Vector3(x, y, 0);
        
        // Get a pooled object from the ObjectPooling system
        GameObject cloud = ObjectPooling.Instance.GetPooledObject(prefab);

        if (cloud != null)
        {
            cloud.transform.position = pos;
            cloud.transform.rotation = clouds[randomIndex].transform.rotation;
            cloud.SetActive(true);

            Debug.Log($"Spawned cloud at {pos} from prefab {prefab.name}");
        }
        else
        {
            Debug.LogWarning($"No pooled object available for cloud prefab: {prefab.name}");
        }
    }

    void SpawnRandomPlatform(float y)
    {
        if (platforms == null || platforms.Length == 0) return;

        int randomIndex = Random.Range(0, clouds.Length);
        GameObject prefab = platforms[randomIndex];

        float randomX = Random.Range(-spawnDistance, spawnDistance);
        GameObject platform = ObjectPooling.Instance.GetPooledObject(prefab);
        
        placePlatform(randomX, y, prefab, platform);

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
    }

    private void OnDisable()
    {
        CancelInvoke("SpawnPlatform"); // Stop spawning platforms when this script is disabled
    }
}
