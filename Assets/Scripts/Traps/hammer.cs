using UnityEngine;

public class hammer : MonoBehaviour
{
    public float moveDistance = 3f; // Maximum extension distance
    public float moveSpeed = 2f;    // Extension speed

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        // Make the hammer move back and forth along the Y axis, with the root fixed
        float offset = Mathf.PingPong(Time.time * moveSpeed, moveDistance);
        transform.localPosition = startPos + Vector3.up * offset;
    }
}