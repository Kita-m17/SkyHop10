using UnityEngine;
using UnityEngine.UIElements;

public class SpawnManager : MonoBehaviour
{
    public GameObject[] platforms;
    public GameObject[] clouds;

    public float spawnDistance = 14f;
    private float spawnY = 8;
    private float spawnInterval = 2f;
    private float startDelay = 2f;
    public float verticalSpacing = 2f; // Distance between platform layers

    public float maxHorizontalJump = 6f; // test in scene view how far player can jump

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InvokeRepeating("SpawnPlatform", startDelay, spawnInterval); // Spawn platforms every second
    }

    // Update is called once per frame
    void Update()
    {
    }

    void SpawnPlatform()
    {
        int randomIndex = Random.Range(0, platforms.Length);
        float x = Random.Range(-spawnDistance, spawnDistance);
        //float height = Random.Range(0, spawnHeight);
        Vector3 pos = new Vector3(x, spawnY, 0);
        Instantiate(platforms[randomIndex], pos, platforms[randomIndex].transform.rotation);

        if (Random.value < 0.5f) // 50% chance to spawn a cloud
        {
            SpawnCloud(spawnY + Random.Range(1f, 2.5f));
        }
        spawnY += verticalSpacing;
    }

    void SpawnCloud(float y)
    {
        int randomIndex = Random.Range(0, clouds.Length);
        float x = Mathf.Clamp(Random.Range(-spawnDistance, spawnDistance), -maxHorizontalJump, maxHorizontalJump);

        //float x = Random.Range(-spawnDistance, spawnDistance);
        //float height = Random.Range(0, spawnHeight);
        Vector3 pos = new Vector3(x, y, 0);
        GameObject cloud = Instantiate(clouds[randomIndex], pos, clouds[randomIndex].transform.rotation);
    }
}
