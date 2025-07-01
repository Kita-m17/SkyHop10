using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5;
    public float jumpForce = 5;
    public bool isOnGround = true;
    //public float accelaration = 12;
    //public float targetSpeed, currentSpeed;
    //public float gravity = 20;

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
    }

    // Update is called once per frame
    void Update()
    {
        rb2d.AddForce(transform.right * speed * Input.GetAxis("Horizontal"), ForceMode2D.Force);
        if(rb2d.linearVelocity.magnitude > 0.01f)
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

        if (Input.GetKeyDown(KeyCode.Space) && isOnGround)
        {
            
            rb2d.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            animator.SetTrigger("jump");
            isOnGround = false;
            //dirtParticle.Stop(); // Stop the dirt particle effect when jumping
            //playerAudio.PlayOneShot(jumpSound, 1.0f); // Play the jump sound effect
        }
        isOnGround = true;

        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground_Platform"))
        {
            isOnGround = true;

            float y = Random.Range(5, 8);
            float x = Random.Range(-4, 3);

            Instantiate(platform, new Vector3(x, transform.position.y + y, 0), Quaternion.identity);
            //Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Cloud_Platform"))
        {
            isOnGround = true;
        }
    }

}
