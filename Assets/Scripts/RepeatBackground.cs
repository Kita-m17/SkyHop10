using UnityEngine;

public class RepeatBackground : MonoBehaviour
{
    private Vector3 startPosition; // Initial position of the background
    private float repeatHeight; // Height at which the background should repeat
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPosition = transform.position;
        repeatHeight = 16; // Get the height of the background from its BoxCollider
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.y <startPosition.y - repeatHeight)
        {
            transform.position = startPosition; // Reset position to the start position
        }
    }
}
