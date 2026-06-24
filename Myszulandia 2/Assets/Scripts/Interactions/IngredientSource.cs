using UnityEngine;

public class IngredientSource : ClickableObject
{
    [SerializeField] int          sourceIndex;
    [SerializeField] KitchenScene kitchen;

    protected override void OnInteract() => kitchen.CollectIngredient(sourceIndex);
}
