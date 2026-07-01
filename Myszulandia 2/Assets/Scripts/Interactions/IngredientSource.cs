using UnityEngine;

public class IngredientSource : ClickableObject
{
    [SerializeField] int          sourceIndex;
    [SerializeField] KitchenScene kitchen;

    void Awake()     => GameEvents.OnNewDayStarted += Respawn;
    void OnDestroy() => GameEvents.OnNewDayStarted -= Respawn;

    void Start()
    {
        if (InventoryManager.Instance != null && InventoryManager.Instance.IsIngredientHarvested(sourceIndex))
            gameObject.SetActive(false);
    }

    protected override void OnInteract()
    {
        kitchen.CollectIngredient(sourceIndex);
        gameObject.SetActive(false);
    }

    void Respawn() => gameObject.SetActive(true);
}
