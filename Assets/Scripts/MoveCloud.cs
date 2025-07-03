using UnityEngine;
using UnityEngine.Timeline;

public class MoveCloud : MonoBehaviour
{
    public float speed = 1f;
    public float moveRange = 2f; // Distance left and right from the starting point

    private Vector3 startPosition;
    private Rigidbody2D rb;
    private int direction = 1;
    private bool isInitialized = false;
    

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        ResetCloud();
    }



    void Update()
    {
        if(!isInitialized)
        {
            return;
        }

        transform.Translate(Vector3.right * direction * speed * Time.deltaTime);
        float distanceFromStart = transform.position.x - startPosition.x;
        

        if (Mathf.Abs(distanceFromStart) >= moveRange)
        {
            direction *= -1; //reverse direction
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Ground_Platform") || collision.gameObject.CompareTag("Cloud_Platform"))
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

    void ResetCloud()
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

    
}
