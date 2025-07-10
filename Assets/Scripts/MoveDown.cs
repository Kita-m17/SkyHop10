using UnityEngine;

public class MoveDown : MonoBehaviour
{
    private float speed = 0.8f; // Speed at which the object moves down
    private PlayerController playerController; // Reference to the PlayerController script
    private bool isInitialized = false; // Flag to check if references are initialized
    private bool isMoving = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeReferences(); // Initialize references to other components
    }

    void OnEnable()
    {
        InitializeReferences();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Try to initialize if not done yet
        if (!isInitialized)
        {
            InitializeReferences();
            if (!isInitialized) return;
        }

        // Check if game is active and player is not in game over state
        if (GameManager.Instance == null || !GameManager.Instance.isGameActive) return;
        if (!isMoving||playerController == null || playerController.gameOver) return;

        // Move object down
        float effectiveSpeed = speed * GameManager.Instance.gameSpeedMultiplier;
        transform.Translate(Vector3.down * effectiveSpeed * Time.deltaTime);
    }

    void InitializeReferences()
    {
        isMoving = true;
        // Find player controller
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerController = playerObj.GetComponent<PlayerController>();
            isInitialized = true;
            isMoving = true;
        }
    }

    public void StopMoving()
    {
        isMoving = false;
    }

    public void ResumeMoving()
    {
        isMoving = true;
    }
}
