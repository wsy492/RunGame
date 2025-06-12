using UnityEngine;

public class WaterScroll : MonoBehaviour
{
    public Vector2 scrollSpeed = new Vector2(0f, 0.0001f); // Vertical flow
    private Material mat;
    private Vector2 offset;

    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            mat = Instantiate(sr.material);
            sr.material = mat;
        }
    }

    void Update()
    {
        if (mat != null)
        {
            offset += scrollSpeed * Time.deltaTime;
            mat.SetVector("_Offset", new Vector4(offset.x, offset.y, 0, 0));

            // Let SpriteRenderer's Color affect transparency
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                mat.SetColor("_RendererColor", sr.color);
                //   Debug.Log("Current alpha: " + sr.color.a);
            }
        }

    }
}