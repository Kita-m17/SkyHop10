using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int jewelsCollected = 0;
    public int totalJewels = 10;
    public bool win = false;
    public static GameManager Instance;
    public int score = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this instance across scenes
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CollectJewel()
    {
        jewelsCollected++;
        if(jewelsCollected >= totalJewels)
        {
            win = true;
        }
    }

    public void AddScore(int value)
    {
        score += value;
        Debug.Log("Score: " + score);
    }
}
