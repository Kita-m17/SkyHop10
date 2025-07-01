using System;
using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float speed = 8;
    public float jumpForce = 35;
    public bool isOnGround = false;
    public int maxJumps = 2;      // Total allowed jumps (1 = single jump, 2 = double jump)
    private float jumpsRemaining; // Number of jumps remaining
    private float xRange = 14;
    private float yRange = 15;
    public bool gameOver = false; // Variable to track if the game is over

    private Rigidbody2D rb2d;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    public GameObject platform; // Reference to the platform prefab

    private Vector2 moveDirection;

    private PlayerPhysics playerPhysics;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        jumpsRemaining = maxJumps; // Initialize jumps remaining to max jumps
    }

    // Update is called once per frame
    void Update()
    {

        if (transform.position.y < -10)
        {
            gameOver = true;
        }


        if (transform.position.x < -xRange)
        {
            transform.position = new Vector3(-xRange, transform.position.y, transform.position.z);
        }
        else if (transform.position.x > xRange)
        {
            transform.position = new Vector3(xRange, transform.position.y, transform.position.z);
        }


        transform.Translate(Vector3.right * speed * Input.GetAxis("Horizontal") * Time.deltaTime);
        if (rb2d.linearVelocity.magnitude > 0.01f)
        {
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }

        // Flip sprite ONLY if Left Arrow is explicitly pressed
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            spriteRenderer.flipX = true;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            spriteRenderer.flipX = false;
        }

        if (Input.GetKeyDown(KeyCode.Space) && jumpsRemaining > 0)
        {
            rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, 0); // Reset vertical velocity
            rb2d.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            animator.SetTrigger("jump");
            isOnGround = false;
            jumpsRemaining--;
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.CompareTag("Ground_Platform"))
        {
            isOnGround = true;
            if (collision.contacts[0].normal.y > 0.5f) // Only reset if touching from the bottom
            {
                jumpsRemaining = maxJumps;
            }
        }
        else if (collision.gameObject.CompareTag("Cloud_Platform"))
        {
            isOnGround = true;
            if (collision.contacts[0].normal.y > 0.5f) // Only reset if touching from the bottom
            {
                jumpsRemaining = maxJumps;
                // Start coroutine to destroy the cloud after 5 seconds
                StartCoroutine(DestroyCloudAfterDelay(collision.gameObject, 5f));
            }
        }
    }

    private IEnumerator DestroyCloudAfterDelay(GameObject cloud, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (cloud != null)
        {
            Destroy(cloud);
        }
    }


}
