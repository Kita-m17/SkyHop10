using UnityEngine;

public class MoveDown : MonoBehaviour
{
    SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component

    public GameObject player; // Reference to the player GameObject
    private float speed = 3f; // Speed at which the object moves down
    Vector3 position; // Position of the object
    private PlayerController playerController; // Reference to the PlayerController script
    private float lowerBound = -10f; // Lower bound for the object's position
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        position = transform.position; // Initialize the position variable
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (!playerController.gameOver)
        //{
        //transform.Translate(Vector3.down - player.transform.position);
        //}

        if (player.transform.position.y - transform.position.y > spriteRenderer.sprite.bounds.size.y /**&& (gameObject.CompareTag("Ground_Platform") || gameObject.CompareTag("Cloud_Platform"))**/)// must include code checking the gamer tag, otherwise, it deletes anything
        {
            position.y += speed * spriteRenderer.sprite.bounds.size.y * Time.deltaTime; // Move the object down based on its sprite size
            transform.position = position; // Update the object's position
            //Destroy(gameObject); // destroy the obstacle if it goes out of bounds
        }
    }
}
