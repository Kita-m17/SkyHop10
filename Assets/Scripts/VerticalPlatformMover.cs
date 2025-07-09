using UnityEngine;
using static UnityEngine.UI.ScrollRect;

public class VerticalPlatformMover : MonoBehaviour
{

    public enum MovementType
    {
        None,
        Vertical,
        Horizontal
    }

    public MovementType movementType = MovementType.None;

    public float moveRange = 2f; // Distance the platform will move in the specified direction
    public float moveSpeed = 1.5f; // Speed of the platform movement
    
    private Vector3 startPosition;
    private int direction = 1; // 1 for forward, -1 for backward
    private bool isInitialized = false;
    private bool movementEnabled = true;
    public float speed = 1f;
    private Rigidbody2D rb;

    private void OnEnable()
    {
        // Only reset once — useful if pooling
        if (movementType != MovementType.None)
        {
            ResetPlatform();
        }
        
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }


    void Update()
    {
        if (!isInitialized)
        {
            return;
        }

        //transform.Translate(Vector3.right * direction * speed * Time.deltaTime);
        float distanceFromStart;
        if (movementType == MovementType.Horizontal) {
            transform.Translate(Vector3.right * direction * speed * Time.deltaTime);
            distanceFromStart = transform.position.x - startPosition.x;
            if (Mathf.Abs(distanceFromStart) >= moveRange)
            {
                direction *= -1; //reverse direction
            }
        }
        else if (movementType == MovementType.Vertical)
        {
            transform.Translate(Vector3.up * direction * speed * Time.deltaTime);
            distanceFromStart = transform.position.y - startPosition.y;
            if (Mathf.Abs(distanceFromStart) >= moveRange)
            {
                direction *= -1; //reverse direction
            }
        }


        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground_Platform") || collision.gameObject.CompareTag("Cloud_Platform"))
        {
            direction *= -1;
        }
    }

    // Method to disable the cloud (called from PlayerController)
    public void DisableMovement()
    {
        enabled = false;
    }

    // Method to enable the cloud movement
    public void EnableMovement()
    {
        enabled = true;
    }

    public void ResetPlatform()
    {
        startPosition = transform.position;
        direction = 1;
        isInitialized = true;

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = true;
            if (renderer.material != null)
            {
                Color colour = renderer.material.color;
                renderer.material.color = new Color(colour.r, colour.g, colour.b, 1f);
            }
        }

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true;
        }
    }

    void OnDisable()
    {
        isInitialized = false;
    }
}