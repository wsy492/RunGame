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
            // Play animation when Hint is visible in the camera view
            animator.SetBool("Play", rend.isVisible);
        }
    }
}