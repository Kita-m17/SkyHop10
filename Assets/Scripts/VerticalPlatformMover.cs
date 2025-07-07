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

    private void OnEnable()
    {
        // Only reset once — useful if pooling
        if (!isInitialized)
        {
            ResetPlatform();
        }
        direction = 1; // Always reset direction when reused
    }

    

    // Update is called once per frame
    void Update()
    {
        if (!isInitialized || (movementType == MovementType.None)) return;

        Vector3 moveAxis = (movementType == MovementType.Horizontal) ? Vector3.right : Vector3.up;
        transform.Translate(moveAxis * direction * moveSpeed * Time.deltaTime);

        // Check if the platform has reached the end of its movement range
        float offset = Vector3.Dot(transform.position - startPosition, moveAxis); // Calculate offset in the movement direction
        if (Mathf.Abs(offset) >= moveRange)
        {
            direction *= -1; // Reverse direction
        }
    }

    void ResetPlatform()
    {
        startPosition = transform.position; // Reset the start position
        direction = 1; // Reset the direction to forward
        isInitialized = true;

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = true; // Ensure the platform is visible
        }

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true; // Ensure the collider is enabled
        }
    }

    public void DisableMovement()
    {
        enabled = false; // Disable the movement script
    }

    public void EnableMovement()
    {
        enabled = true; // Enable the movement script
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground_Platform") || collision.gameObject.CompareTag("Cloud_Platform"))
        {
            direction *= -1;
        }
    }

    void OnDisable()
    {
        isInitialized = false;
    }
}
