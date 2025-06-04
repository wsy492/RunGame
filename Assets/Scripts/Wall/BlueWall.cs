using UnityEngine;

public class BlueWall : AttractWallBase
{
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        // 不做任何处理，小人不会死
    }
}