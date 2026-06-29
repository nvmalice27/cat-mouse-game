using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DrinkObject : ClickableObject
{
    protected override void OnInteract()
    {
        InventoryManager.Instance.CollectDrink();
    }
}
