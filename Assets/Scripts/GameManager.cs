using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public bool isGameActive = false; // Start as false - game only active after button press
    public bool isGameStarted = false; // Track if game has been started at least once

    private int jewelsCollected = 0;
    public int totalJewels = 7;
    public float gameSpeedMultiplier = 1f;
    public int coinScore = 0;
    public float spawnRate;
    public float baseSpawnInterval = 1.5f; // Base spawn interval for enemies and items

    private int maxLives = 3; // Maximum number of lives the player can have
    private int lives = 3; // Current number of lives the player has

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

    private bool canTakeDamage = true;
    public float damageImmunityTime = 1f; // 1 second of immunity after taking damage
    private bool restart = false;

    public GameObject gameplayRoot; // Assign in Inspector - contains all gameplay elements (player, spawners, etc.)
    public GameObject uiRoot;       // Optional: non-title UI (score, lives, etc.)

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        titleScreen.SetActive(true);
        // Ensure game starts in inactive state
        isGameActive = false;
        isGameStarted = false;
        // Disable gameplay until difficulty is selected
        if (gameplayRoot != null) gameplayRoot.SetActive(false);
        if (uiRoot != null) uiRoot.SetActive(false);
        // Ensure title screen is visible
        //if (titleScreen != null) titleScreen.SetActive(true);

        spawnRate = 0.5f;
        // Hide game over elements initially
        if (gameOverText != null) gameOverText.gameObject.SetActive(false);
        if (restartButton != null) restartButton.gameObject.SetActive(false);

        if (coinScoreText != null)
        {
            coinScoreText.text = "Score: " + coinScore;
        }

        if(jewelsText != null)
        {
            jewelsText.text = "Gems: " + jewelsCollected + "/" + totalJewels;
        }

        UpdateLivesDisplay(); // Update the hearts display
    }

    private void Awake()
    {
        if (Instance == null)
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
        // Only check win condition if game is active and started
        if (isGameActive && isGameStarted && jewelsCollected >= totalJewels && !win)
        {
            win = true; // Set win to true when all jewels are collected
            WinGame(); // Call GameOver method to handle win condition
        }
    }

    public void CollectJewel()
    {
        // Only collect jewels if game is active
        if (!isGameActive || !isGameStarted) return;

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
        // Only collect jewels if game is active
        if (!isGameActive || !isGameStarted) return;

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
        PlayerController playerController = FindObjectOfType<PlayerController>();

        if (!canTakeDamage || !isGameActive) return; // Don't take damage if on cooldown or game not active
        if (playerController.isInvincible) return;
        if (lives > 0)
        {
            lives--;
            UpdateLivesDisplay();

            // Start damage immunity period
            canTakeDamage = false;
            StartCoroutine(DamageImmunityCoroutine());

            if (lives <= 0)
            {
                GameOver();
            }
        }
    }

    private System.Collections.IEnumerator DamageImmunityCoroutine()
    {
        yield return new WaitForSeconds(damageImmunityTime);
        canTakeDamage = true;
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

        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
            playerController.Win();

    }

    public void GameOver()
    {
        restartButton.gameObject.SetActive(true); // Show the restart button
        gameOverText.gameObject.SetActive(true); // Show the game over text
        win = false;
        isGameActive = false; // Set the game active flag to false


        VerticalPlatformMover[] movers = FindObjectsOfType<VerticalPlatformMover>();
        foreach (VerticalPlatformMover mover in movers)
        {
            mover.DisableMovement(); // Disable all vertical platform movers
        }

        MoveCloud[] clouds = FindObjectsOfType<MoveCloud>();
        foreach (var cloud in clouds)
        {
            cloud.DisableMovement(); // Disable all clouds
        }

        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.playerAudio.PlayOneShot(playerController.deathSound, 1.0f); // Play the game over sound
            playerController.Dead(); // Call the Die method on the player controller to handle death
        }
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
        isGameActive = false;
        isGameStarted = false; // Reset started flag
        canTakeDamage = true; // Reset damage immunity

        // Hide game over UI
        if (gameOverText != null) gameOverText.gameObject.SetActive(false);
        if (restartButton != null) restartButton.gameObject.SetActive(false);
        restart = true;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Reload the current scene
    }

    public void SpeedUp(float amount)
    {
        gameSpeedMultiplier *= amount; // Increase the game speed multiplier
        gameSpeedMultiplier = Mathf.Clamp(gameSpeedMultiplier, 1f, 3f); // Clamp the speed to a reasonable range
    }



    public void StartGame(int difficulty)
    {
        

        if (titleScreen != null)
            titleScreen.gameObject.SetActive(false); // Hide the title screen UI

        if (gameplayRoot != null)
            gameplayRoot.SetActive(true); // Enable gameplay

        if (uiRoot != null)
            uiRoot.SetActive(true); // Show HUD

        isGameActive = true; // Set the game active flag to true
        isGameStarted = true;
        jewelsCollected = 0; // Initialize jewels collected
        coinScore = 0; // Initialize score
        win = false;
        lives = maxLives; // Reset lives to maximum
        canTakeDamage = true; // Reset damage immunity
        totalJewels += difficulty; // Increase total jewels based on difficulty

        gameSpeedMultiplier = 1f + (difficulty - 1) * 0.5f;

        spawnRate = Mathf.Clamp(1f/difficulty, 0.3f, 1f); // Adjust spawn rate based on difficulty
        
        SpawnManager2 spawnManager = FindObjectOfType<SpawnManager2>();
        if (spawnManager != null)
        {
            spawnManager.minSpawnDelay *= difficulty;
            spawnManager.maxSpawnDelay *= difficulty;

            float adjustedSpawnInterval = baseSpawnInterval / (difficulty * GameManager.Instance.gameSpeedMultiplier);
            spawnManager.spawnInterval = Mathf.Clamp(adjustedSpawnInterval, 0.2f, 2f);

            spawnManager.Initialize();
        }

        UpdateCoinScore(); // Ensure the score is displayed correctly at the start
        UpdateJewel(); // Ensure the jewels collected text is displayed correctly at the start
        UpdateLivesDisplay();
        
        if (coinScoreText != null)
            coinScoreText.text = "Score: " + coinScore;

        if (jewelsText != null)
            jewelsText.text = "Gems: " + jewelsCollected + "/" + totalJewels;

        
    }

    // Helper method to check if game can accept input
    public bool CanAcceptInput()
    {
        return isGameActive && isGameStarted;
    }
}
