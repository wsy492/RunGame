using UnityEngine;
using UnityEngine.UI;

public class CherryManager : MonoBehaviour
{
    public static int totalCherries = 0;
    public Sprite cherryIcon; // Cherry icon
    public Text cherryCountText; // Text component to display the amount

    public static CherryManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Dynamically update Cherry count
        if (cherryCountText != null)
        {
            cherryCountText.text = totalCherries.ToString();
        }
    }
}