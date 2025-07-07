using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public bool isGameActive = true; // Flag to check if the game is active

    public int jewelsCollected = 0;
    public int totalJewels = 7;
    public bool win = false;

    public static GameManager Instance;
    public int coinScore = 0;

    public TextMeshProUGUI coinScoreText; // Reference to the UI Text component for displaying score
    public TextMeshProUGUI jewelsText; // Reference to the UI Text component for displaying jewels collected
    public TextMeshProUGUI gameOverText; // Reference to the UI Text component for displaying game over message
    public Button restartButton; // Reference to the UI Button for restarting the game
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isGameActive = true; // Set the game active flag to true
        coinScore = 0; // Initialize score
        coinScoreText.text = "Score: " + coinScore; // Update the UI text with initial score
        jewelsCollected = 0; // Initialize jewels collected
        jewelsText.text = "Gems: " + jewelsCollected + "/" + totalJewels; // Update the UI text with initial jewels collected
        
        if (coinScoreText != null)
            coinScoreText.text = "Score: " + coinScore;

        if (jewelsText != null)
            jewelsText.text = "Gems: " + jewelsCollected + "/" + totalJewels;

        UpdateCoinScore(); // Ensure the score is displayed correctly at the start
    }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // Keep this instance across scenes
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
        UpdateJewel(); // Update the UI text with the new jewels collected

        if(jewelsCollected >= totalJewels)
        {
            win = true;
        }
    }

    public void AddScore()
    {
        coinScore ++;
        UpdateCoinScore(); // Update the UI text with the new score
    }

    void UpdateCoinScore()
    {
       coinScoreText.text = "Score: " + coinScore; // Update the UI text with the new score
    }

    void UpdateJewel()
    {
        jewelsText.text = "Gems: " + jewelsCollected + "/" + totalJewels; // Update the UI text with initial jewels collected
    }

    public void GameOver()
    {
        restartButton.gameObject.SetActive(true); // Show the restart button
        gameOverText.gameObject.SetActive(true); // Show the game over text
        win = false;
        isGameActive = false; // Set the game active flag to false
    }

    public void RestartGame()
    {
        if(ObjectPooling.Instance != null)
        {
            ObjectPooling.Instance.ResetPool();//Clear object pools if they exist
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload the current scene
    }
}
