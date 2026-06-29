using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MouseBallObject : ClickableObject
{
    protected override void OnInteract()
    {
        InventoryManager.Instance.CollectMouseBall();
    }
}
