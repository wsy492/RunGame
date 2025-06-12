using UnityEngine;

public class smashers_spikes : MonoBehaviour
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
        // Make the machine move back and forth along the rod
        float offset = Mathf.PingPong(Time.time * moveSpeed, moveDistance);
        transform.localPosition = startPos + Vector3.up * offset;
    }
}