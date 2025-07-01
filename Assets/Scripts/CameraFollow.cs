using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject player; // Reference to the player GameObject
    private float offset = 5f; // Offset from the player position
    Vector3 position;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        position = transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        position.y = player.transform.position.y;
        transform.position = position;
    }
}
