using UnityEngine;
using UnityEngine.UI;

public class CherryManager : MonoBehaviour
{
    public static int totalCherries = 0;
    public Sprite cherryIcon; // Cherry 图标
    public Text cherryCountText; // 显示数量的 Text 组件

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
        // 动态更新 Cherry 数量
        if (cherryCountText != null)
        {
            cherryCountText.text = totalCherries.ToString();
        }
    }
}