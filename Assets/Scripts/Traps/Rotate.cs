using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private int direction = 1; // 1为顺时针，-1为逆时针

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, 0, 360 * speed * direction * Time.deltaTime);
    }
}
