using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public bool isGameActive = true; // Flag to check if the game is active

    private int jewelsCollected = 0;
    private int totalJewels = 7;
    public float gameSpeedMultiplier = 1f;
    public int coinScore = 0;
    public float spawnRate;

    private int maxLives = 3; // Maximum number of lives the player can have
    public int lives = 3; // Current number of lives the player has

    public bool win = false;

    public static GameManager Instance;

    public TextMeshProUGUI coinScoreText; // Reference to the UI Text component for displaying score
    public TextMeshProUGUI jewelsText; // Reference to the UI Text component for displaying jewels collected
    public TextMeshProUGUI gameOverText; // Reference to the UI Text component for displaying game over message
    public Button restartButton; // Reference to the UI Button for restarting the game
    public GameObject titleScreen; // Reference to the title screen UI
    public Image heart1;
    public Image heart2;
    public Image heart3;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnRate = 0.2f;
        if(coinScoreText != null)
        {
            coinScoreText.text = "Score: " + coinScore;
        }

        if(jewelsText != null)
        {
            jewelsText.text = "Gems: " + jewelsCollected + "/" + totalJewels;
        }
    }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if (!playerController.gameOver) // Check if the game is over
        //{
            //transform.Translate(Vector3.down * speed * GameManager.Instance.gameSpeedMultiplier * Time.deltaTime);
            // Remove the debug log as it's spamming
        //}

        if(jewelsCollected >= totalJewels && !win)
        {
            win = true; // Set win to true when all jewels are collected
            WinGame(); // Call GameOver method to handle win condition
        }
    }

    public void CollectJewel()
    {
        jewelsCollected++;
        UpdateJewel(); // Update the UI text with the new jewels collected

        if(jewelsCollected >= totalJewels)
        {
            win = true;
            WinGame();
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

    void UpdateLivesDisplay()
    {
        // Update heart display based on current lives
        if (heart1 != null)
            heart1.enabled = lives >= 1;
        if (heart2 != null)
            heart2.enabled = lives >= 2;
        if (heart3 != null)
            heart3.enabled = lives >= 3;
    }

    public void TakeDamage()
    {
        if (lives > 0)
        {
            lives--;
            UpdateLivesDisplay();

            if (lives <= 0)
            {
                GameOver();
            }
        }
    }

    public void WinGame()
    {
        isGameActive = false;
        if (gameOverText != null)
        {
            gameOverText.text = "You Win!"; // Update the game over text to indicate win
            gameOverText.gameObject.SetActive(true); // Show the game over text
        }

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(true); // Show the restart button
        }

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
        StopAllCoroutines();

        if (ObjectPooling.Instance != null)
        {
            ObjectPooling.Instance.ResetPool();//Clear object pools if they exist
        }

        // Make absolutely sure no ghost Player exists
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (var p in allPlayers)
        {
            Destroy(p);
        }

        // Reset game state
        Time.timeScale = 1f;
        gameSpeedMultiplier = 1f;
        jewelsCollected = 0;
        coinScore = 0;
        win = false;
        isGameActive = true;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Reload the current scene
    }

    public void StartGame(int difficulty)
    {
        if (titleScreen != null)
            titleScreen.gameObject.SetActive(false); // Hide the title screen UI
        
        isGameActive = true; // Set the game active flag to true
        jewelsCollected = 0; // Initialize jewels collected
        coinScore = 0; // Initialize score
        win = false;
        lives = maxLives; // Reset lives to maximum

        spawnRate /= difficulty; // Adjust spawn rate based on difficulty
        
        UpdateCoinScore(); // Ensure the score is displayed correctly at the start
        UpdateJewel(); // Ensure the jewels collected text is displayed correctly at the start
        UpdateLivesDisplay();
        
        if (coinScoreText != null)
            coinScoreText.text = "Score: " + coinScore;

        if (jewelsText != null)
            jewelsText.text = "Gems: " + jewelsCollected + "/" + totalJewels;

        
    }
}
