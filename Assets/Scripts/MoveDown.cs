using UnityEngine;

public class MoveDown : MonoBehaviour
{
    public float speed = 0.2f; // Speed at which the object moves down
    Vector3 position; // Position of the object
    private PlayerController playerController; // Reference to the PlayerController script
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        position = transform.position; // Initialize the position variable
        playerController = GameObject.Find("Player").GetComponent<PlayerController>(); // Find the PlayerController in the scene
    }

    // Update is called once per frame
    void Update()
    {
        if(!playerController.gameOver) // Check if the game is over
        {
            transform.Translate(Vector3.down * speed * Time.deltaTime); // Move the object down at a constant speed
        }    }
}
