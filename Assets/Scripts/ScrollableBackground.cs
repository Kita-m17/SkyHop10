using UnityEngine;

public class ScrollableBackground : MonoBehaviour
{
    public float horizontalScrollSpeed = 0.5f; // Speed of the background scrolling
    public float verticalScrollSpeed = 0.5f; // Speed of the background scrolling
    private SpriteRenderer renderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 offset = new Vector3(Time.time * horizontalScrollSpeed, Time.time * verticalScrollSpeed, 0);
        renderer.material.mainTextureOffset = offset;
    }
}
