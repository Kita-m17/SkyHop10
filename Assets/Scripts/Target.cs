using UnityEngine;

public class Target : MonoBehaviour
{

    //private Rigidbody2D targetRB;
    public float rotationSpeed = 50f;
    private Vector3 startPosition;
    public float floatSpeed = 1;
    public float floatHeight = 0.5f;
    private float floatTimer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.CollectJewel();
            gameObject.SetActive(false);
            
        }
    }
}

