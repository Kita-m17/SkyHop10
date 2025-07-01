using UnityEngine;
using UnityEngine.UIElements;

public class SpawnManager : MonoBehaviour
{
    public GameObject[] platforms;
    public GameObject[] clouds;
    public GameObject player;
    public GameObject backgroundPrefab;
    private Vector3 backgroundStart;
    private float repeatHeight = 50f;
    private float lastBackgroundY = 0f;

    //public float spawnDistance;
    public float spawnHeight = 3;
    private float lastSpawnY;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        backgroundStart = backgroundPrefab.transform.position;
        lastSpawnY = player.transform.position.y;

    }

    // Update is called once per frame
    void Update()
    {
        /**
        float playerY = player.transform.position.y;

        if(playerY + Camera.main.orthographicSize > backgroundStart.y + repeatHeight)
        {
            SpawnBackground(lastBackgroundY+repeatHeight);
            lastBackgroundY += repeatHeight;
        }

        if(playerY > lastSpawnY)
        {
            SpawnPlatform(lastSpawnY);
            SpawnCloud(lastSpawnY);
            lastSpawnY += spawnHeight;
        }
        **/
    }

    void SpawnBackground(float y)
    {
        Vector3 position = new Vector3(0, y, 0); // Change X/Z if needed
        Instantiate(backgroundPrefab, position, Quaternion.identity);

    }

    void SpawnPlatform(float y)
    {
        int randomIndex = Random.Range(0, platforms.Length);
        float x = Random.Range(-6f,6f);
        //float height = Random.Range(0, spawnHeight);
        Vector3 pos = new Vector3(x, y, 0);
        Instantiate(platforms[randomIndex], pos, Quaternion.identity);
    }

    void SpawnCloud(float y)
    {
        int randomIndex = Random.Range(0, clouds.Length);
        float x = Random.Range(-7f, 7f);
        //float height = Random.Range(0, spawnHeight);
        Vector3 pos = new Vector3(x, y, 0);
        GameObject cloud = Instantiate(clouds[randomIndex], pos, Quaternion.identity);
    }
}
