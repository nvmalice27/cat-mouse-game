using UnityEngine;

public class CowObject : ClickableObject
{
    SpriteRenderer _sr;
    Collider2D     _col;

    void Awake()
    {
        _sr  = GetComponent<SpriteRenderer>();
        _col = GetComponent<Collider2D>();
    }

    void OnEnable()
    {
        GameEvents.OnNewDayStarted += OnNewDay;
        bool harvested = InventoryManager.Instance != null && InventoryManager.Instance.IsCowHarvested;
        _sr.enabled  = !harvested;
        _col.enabled = !harvested;
    }

    void OnDisable() => GameEvents.OnNewDayStarted -= OnNewDay;

    void OnNewDay()
    {
        _sr.enabled  = true;
        _col.enabled = true;
    }

    protected override void OnInteract()
    {
        _sr.enabled  = false;
        _col.enabled = false;
        InventoryManager.Instance.CollectCow();
    }
}
