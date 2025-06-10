using UnityEngine;

public class hammer : MonoBehaviour
{
    public float moveDistance = 3f; // 伸缩最大距离
    public float moveSpeed = 2f;    // 伸缩速度

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        // 让锤子在Y轴上来回伸缩，根部不动
        float offset = Mathf.PingPong(Time.time * moveSpeed, moveDistance);
        transform.localPosition = startPos + Vector3.up * offset;
    }
}