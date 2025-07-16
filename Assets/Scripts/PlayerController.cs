using System;
using UnityEngine;
using System.Collections;
using static PowerUpManager;

public class PlayerController : MonoBehaviour
{
    public float speed = 8;
    public float jumpForce = 50;
    public int maxJumps = 2;      // Total allowed jumps (1 = single jump, 2 = double jump)
    
    // Variables to track player state
    public bool isOnGround = false;
    private float jumpsRemaining; // Number of jumps remaining
    public bool gameOver = false; // Variable to track if the game is over
    public bool win = false; // Variable to track if the player has won
    
    private float xRange = 14;
    private float yRange = 15;

    public float knockBackForce = 30f;// Force applied when player is knocked back
    public float knockBackUpwardForce = 20f; // Upward force applied when player is knocked back

    private Rigidbody2D rb2d;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    public GameObject platform; // Reference to the platform prefab
    public GameManager gameManager;
    //public VerticalPlatformMover mover;
    private Vector2 moveDirection;
    private bool hasTriggeredSpikes = false; // Flag to prevent multiple spike triggers

    public AudioSource playerAudio;
    public AudioClip jumpSound;
    public AudioClip coinCollection;
    public AudioClip gemCollection;
    public AudioClip deathSound;
    public AudioClip winSound;
    public AudioClip damageSound;
    public AudioClip powerUpSound;
    public static PlayerController Instance;

    public GameObject dash;
    private Animator dashwindAnimator;
    private SpriteRenderer dashSpriteRenderer; // Reference to the dash sprite renderer

    public ParticleSystem damageParticle; // Particle effect for damage
    public ParticleSystem powerUpParticle;

    public int lives = 3;
    public int maxLives = 3;

    public bool isInvincible = false;
    private float originalSpeed;
    private float originalJump;
    private Coroutine powerUpRoutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        if (dash != null)
        {
            dashwindAnimator = dash.GetComponent<Animator>();
        }
        if (dashwindAnimator != null)
        {
            dashwindAnimator.SetBool("isMoving", false); // Set dash animation to not moving initially
        }
        if(dashSpriteRenderer != null)
        {
            dashSpriteRenderer = dash.GetComponent<SpriteRenderer>();
        }
         // Ensure the player is not destroyed on scene load

        playerAudio = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        jumpsRemaining = maxJumps; // Initialize jumps remaining to max jumps
        lives = maxLives; // Initialize lives to max lives
        if(gameManager == null)
        {
            gameManager = GameManager.Instance;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (!GameManager.Instance.CanAcceptInput())
        {
            return;
        }
        if (gameOver)
        {
            win = false;
            GameManager.Instance.GameOver();
        }

        float topOfScreenY = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0)).y;
        if (transform.position.x < -xRange)
        {
            transform.position = new Vector3(-xRange, transform.position.y, transform.position.z);
        }
        else if (transform.position.x > xRange)
        {
            transform.position = new Vector3(xRange, transform.position.y, transform.position.z);
        }
        else if (transform.position.y > topOfScreenY)
        {
            transform.position = new Vector3(transform.position.x, topOfScreenY, transform.position.z);
        }

        float horizontalInput = Input.GetAxisRaw("Horizontal");

        // Handle movement
        transform.Translate(Vector3.right * speed * horizontalInput * Time.deltaTime);
        /**
        Vector2 velocity = rb2d.velocity;
        velocity.x = Input.GetAxis("Horizontal") * speed;
        rb2d.velocity = velocity;
        **/
        if (Mathf.Abs(rb2d.linearVelocity.x) > 0.01f || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            animator.SetBool("isWalking", true);
            if (isOnGround)
            { // Only play dash animation if on ground
                dashwindAnimator.SetBool("isMoving", true); // Set dash animation to moving
                dashwindAnimator.gameObject.SetActive(true);
            }
        }
        else
        {
            animator.SetBool("isWalking", false);
            dashwindAnimator.SetBool("isMoving", false); // Set dash animation to not moving
            dashwindAnimator.gameObject.SetActive(false);
        }

        // Flip sprite ONLY if Left Arrow is explicitly pressed
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            spriteRenderer.flipX = true;
            if (dash != null)
            {
                Vector3 scale = dash.transform.localScale;
                scale.x = -Mathf.Abs(scale.x); // Face left
                dash.transform.localScale = scale;

            }

            // Update dashwind position every frame
            if (dash != null)
            {
                float offsetX = 1.0f; // Adjust how far behind the player
                float offsetY = -0.8f; // Optional vertical offset

                Vector3 dashPos = transform.position;
                if (spriteRenderer.flipX)
                {
                    dashPos.x += offsetX; // Behind if facing left
                    dash.transform.localScale = new Vector3(-Mathf.Abs(dash.transform.localScale.x), dash.transform.localScale.y, dash.transform.localScale.z);
                }
                else
                {
                    dashPos.x -= offsetX; // Behind if facing right
                    dash.transform.localScale = new Vector3(Mathf.Abs(dash.transform.localScale.x), dash.transform.localScale.y, dash.transform.localScale.z);
                }

                dashPos.y += offsetY;
                dash.transform.position = dashPos;
            }

        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            spriteRenderer.flipX = false;
            if (dash != null)
            {
                Vector3 scale = dash.transform.localScale;
                scale.x = Mathf.Abs(scale.x); // Face right
                dash.transform.localScale = scale;
            }


            // Update dashwind position every frame
            if (dash != null)
            {
                float offsetX = 1.0f; // Adjust how far behind the player
                float offsetY = -0.8f; // Optional vertical offset

                Vector3 dashPos = transform.position;
                if (spriteRenderer.flipX)
                {
                    dashPos.x += offsetX; // Behind if facing left
                    dash.transform.localScale = new Vector3(-Mathf.Abs(dash.transform.localScale.x), dash.transform.localScale.y, dash.transform.localScale.z);
                }
                else
                {
                    dashPos.x -= offsetX; // Behind if facing right
                    dash.transform.localScale = new Vector3(Mathf.Abs(dash.transform.localScale.x), dash.transform.localScale.y, dash.transform.localScale.z);
                }

                dashPos.y += offsetY;
                dash.transform.position = dashPos;
            }

        }


        if (Input.GetKeyDown(KeyCode.Space) && jumpsRemaining > 0)
        {
            rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, 0); // Reset vertical velocity
            rb2d.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

            animator.SetTrigger("jump");
            isOnGround = false;
            jumpsRemaining--;
            playerAudio.PlayOneShot(jumpSound, 1f); // Play jump sound
            dashwindAnimator.SetBool("isMoving", false); // Set dash animation to not moving
            dashwindAnimator.gameObject.SetActive(false);
        }

    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log("Duplicate Player detected � destroying");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        // Ensure the player is not destroyed on scene load
        DontDestroyOnLoad(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.CompareTag("Ground_Platform"))
        {

            isOnGround = true;
            float averageY = 0f;

            // Calculate the average Y value of the contact normals
            foreach (var contact in collision.contacts)
            {
                averageY += contact.normal.y;
            }
            averageY /= collision.contactCount;

            if (averageY > 0.2) // Only reset if touching from the bottom
            {
                jumpsRemaining = maxJumps;
            }

            // Attach to platform so player moves with it
            transform.SetParent(collision.transform);
        }
        else if (collision.gameObject.CompareTag("Cloud_Platform"))
        {
            isOnGround = true;
            jumpsRemaining = maxJumps;
            StartCoroutine(DestroyCloudAfterDelay(collision.gameObject, 1.5f, 5f));
            // Attach to platform so player moves with it
            transform.SetParent(collision.transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!gameObject.activeInHierarchy) return;

        if (collision.gameObject.CompareTag("Ground_Platform") || collision.gameObject.CompareTag("Cloud_Platform"))
        {
            StartCoroutine(DetachFromPlatform());
        }
    }

    private IEnumerator DetachFromPlatform()
    {
        yield return new WaitForSeconds(0.1f); // Small delay to ensure smooth detachment
        isOnGround = false; // Reset ground state
        transform.SetParent(null);
    }

    private IEnumerator DestroyCloudAfterDelay(GameObject cloud, float fadeDuration, float delay)
    {
        // Find the particle system on the cloud (assumes it's a child)
        ParticleSystem poof = cloud.GetComponentInChildren<ParticleSystem>();

        Renderer renderer = cloud.GetComponent<Renderer>();
        PolygonCollider2D collider = cloud.GetComponent<PolygonCollider2D>();
        Material material = renderer.material;
        if (renderer == null || collider == null)
        {
            Debug.LogWarning("Missing Renderer or Collider2D on cloud: " + cloud.name);
            yield break;
        }
        Color colour = material.color;

        float time = 0f;

        while (time < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, time / fadeDuration);
            material.color = new Color(colour.r, colour.g, colour.b, alpha);
            time += Time.deltaTime;
            
            yield return null;
        }

        material.color = new Color(colour.r, colour.g, colour.b, 0f);
        cloud.GetComponent<MoveCloud>().enabled = false; // Disable the cloud's movement script
        renderer.enabled = false; // Hide the cloud sprite
        collider.enabled = false; // Disable the collider to prevent further interactions
        if (poof != null)
        {
            poof.Play(); // Play the particle effect if it exists
        }

        yield return new WaitForSeconds(delay); // Wait for a moment before destroying

        renderer.enabled = true; // Re-enable the sprite renderer
        collider.enabled = true; // Re-enable the collider

        cloud.GetComponent<MoveCloud>().enabled = true; // Re-enable the cloud's movement script

        float fadeInTime = 0f;

        while (fadeInTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, fadeInTime / fadeDuration);
            material.color = new Color(colour.r, colour.g, colour.b, alpha);
            
            fadeInTime += Time.deltaTime;
            yield return null;

        }
        material.color = colour; // Ensure the sprite color is reset to original

        

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Gem"))
        {
            other.gameObject.SetActive(false);
            playerAudio.PlayOneShot(gemCollection, 1f); // Play gem collection sound
        }else if (other.CompareTag("Coin"))
        {
            other.gameObject.SetActive(false);
            playerAudio.PlayOneShot(coinCollection, 1f); // Play coin collection sound
        }else if(other.CompareTag("DeathZone"))
        {
            gameOver = true;
            win = false;
            GameManager.Instance.GameOver();
        }else if (other.CompareTag("Spikes"))
        {
            hasTriggeredSpikes = true; // Set flag to prevent multiple triggers
            playerAudio.PlayOneShot(damageSound, 1f); // Play damage sound

            SetHurt(); // Set hurt animation
            GameManager.Instance.TakeDamage();
            damageParticle.Play(); // Play damage particle effect
            ApplyKnowckback(other.transform.position); // Apply knockback effect
            StartCoroutine(ResetSpikesTrigger()); // Reset the spike trigger after a delay
        }else if (other.CompareTag("Powerup")){
            ActivatePowerUp(other.GetComponent<PowerUpManager>().powerUpType, other.GetComponent<PowerUpManager>().duration);
            other.gameObject.SetActive(false);
            playerAudio.PlayOneShot(powerUpSound, 1f);
        }
        
    }

    public void SetHurt()
    {
        animator.SetBool("hurt", true); // Set hurt animation
        animator.SetBool("isDead", false); // Ensure dead animation is not active
    }
    public void ResetHurt()
    {
        animator.SetBool("hurt", false); // Reset hurt animation
    }

    public void Dead()
    {
        animator.SetBool("win", false);
        animator.SetBool("isDead", true); // Trigger death animation
    }

    public void Win()
    {
        animator.SetTrigger("win"); // Trigger win animation
        playerAudio.PlayOneShot(winSound, 1f); // Play win sound
    }

    private void ApplyKnowckback(Vector3 spikePosition)
    {
        Vector2 knockbackDirection = (transform.position - spikePosition).normalized; // Calculate direction away from spike
        // rb2d.velocity =  new Vector2(knockbackDirection.x * knockBackForce, knockBackUpwardForce); // Apply angular velocity for rotation
        rb2d.AddForce(knockbackDirection * knockBackForce, ForceMode2D.Impulse); // Apply force in the opposite direction of the spike
    }

    private System.Collections.IEnumerator ResetSpikesTrigger()
    {
        yield return new WaitForSeconds(1f);
        hasTriggeredSpikes = false;
        ResetHurt(); // Reset hurt animation after a delay
    }

    public void ActivatePowerUp(PowerUpType type, float duration)
    {
        if (powerUpRoutine != null)
        {
            StopCoroutine(powerUpRoutine); // Only one at a time (optional)
        }

        powerUpParticle.transform.SetParent(transform); // Attach to player

        powerUpRoutine = StartCoroutine(ApplyPowerUp(type, duration));
    }

    private IEnumerator ApplyPowerUp(PowerUpType type, float duration)
    {
        originalSpeed = speed;
        originalJump = jumpForce;
        
        powerUpParticle.Play();

        switch (type)
        {
            case PowerUpType.ExtraJump:
                maxJumps = 3;
                break;

            case PowerUpType.SpeedBoost:
                speed *= 1.5f;
                break;

            case PowerUpType.Invincibility:
                isInvincible = true;
                break;

            case PowerUpType.SlowMotion:
                Time.timeScale = 0.5f;
                break;
        }

        yield return new WaitForSecondsRealtime(duration);
        powerUpParticle.Stop();

        // Revert effects
        maxJumps = 2;
        speed = originalSpeed;
        jumpForce = originalJump;
        isInvincible = false;
        Time.timeScale = 1f;
    }


}
