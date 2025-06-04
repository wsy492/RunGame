using UnityEngine;

public class Hint : MonoBehaviour
{
    private Renderer rend;
    private Animator animator;

    void Start()
    {
        rend = GetComponent<Renderer>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (rend != null && animator != null)
        {
            // 当Hint在摄像机视野内时，播放动画
            animator.SetBool("Play", rend.isVisible);
        }
    }
}