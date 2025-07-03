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
        startPosition = transform.position;
        direction = 1;

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
}
