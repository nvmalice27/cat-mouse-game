using UnityEngine;
using System.Collections.Generic;

public enum ItemType { Crumb, Sock, MealGood, MealBad, Rose, Ticket }

[System.Serializable]
public class InventoryItem
{
    public ItemType type;
    public int      quantity;
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    int _crumbsTotal;
    int _crumbsInInventory;
    int _socksCollected;
    readonly List<InventoryItem> _items = new();

    public int CrumbsTotal       => _crumbsTotal;
    public int CrumbsInInventory => _crumbsInInventory;
    public int SocksCollected    => _socksCollected;
    public IReadOnlyList<InventoryItem> Items => _items;

    void Awake()
    {
        if (Instance != null) { Destroy(this); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void CollectCrumb()
    {
        _crumbsTotal++;
        _crumbsInInventory++;
        GameEvents.RaiseCrumbsTotalChanged(_crumbsTotal);
        GameEvents.RaiseInventoryChanged();
        if (_crumbsTotal >= 100)
            MouseStateManager.Instance.TriggerCzonstkowa();
    }

    public void UseCrumbOnMouse()
    {
        if (_crumbsInInventory <= 0) return;
        _crumbsInInventory--;
        GameEvents.RaiseInventoryChanged();
        MouseStateManager.Instance.ApplyNegativeAction(true);
    }

    public void CollectSock()
    {
        if (_socksCollected >= 3) return;
        _socksCollected++;
        GameEvents.RaiseSocksChanged(_socksCollected);
    }

    public bool TryUseSocksOnMouse()
    {
        if (_socksCollected < 3) return false;
        _socksCollected = 0;
        GameEvents.RaiseSocksChanged(0);
        GameEvents.RaiseInventoryChanged();
        MouseStateManager.Instance.TriggerMakapaka();
        return true;
    }

    public void AddMeal(bool isGood) => AddItem(isGood ? ItemType.MealGood : ItemType.MealBad);
    public void AddRose()            => AddItem(ItemType.Rose);
    public void AddTicket()          => AddItem(ItemType.Ticket);

    public bool TryUseMealOnMouse(bool isGood)
    {
        ItemType t = isGood ? ItemType.MealGood : ItemType.MealBad;
        if (!RemoveItem(t)) return false;
        MouseStateManager.Instance.Feed(isGood);
        return true;
    }

    public bool UseRoseOnMouse()
    {
        if (!RemoveItem(ItemType.Rose)) return false;
        MouseStateManager.Instance.TriggerRose();
        GameEvents.RaiseCutsceneRequested("Date");
        return true;
    }

    public bool UseTicketOnMouse()
    {
        if (!RemoveItem(ItemType.Ticket)) return false;
        MouseStateManager.Instance.TriggerVacation();
        GameEvents.RaiseCutsceneRequested("Vacation");
        return true;
    }

    void AddItem(ItemType t)
    {
        var existing = _items.Find(i => i.type == t);
        if (existing != null) existing.quantity++;
        else _items.Add(new InventoryItem { type = t, quantity = 1 });
        GameEvents.RaiseInventoryChanged();
    }

    bool RemoveItem(ItemType t)
    {
        var item = _items.Find(i => i.type == t && i.quantity > 0);
        if (item == null) return false;
        item.quantity--;
        if (item.quantity <= 0) _items.Remove(item);
        GameEvents.RaiseInventoryChanged();
        return true;
    }

    public void ResetDailyItems()
    {
        _socksCollected = 0;
        GameEvents.RaiseSocksChanged(0);
        GameEvents.RaiseInventoryChanged();
    }

    public void ApplySaveData(SaveData d)
    {
        _crumbsTotal      = d.crumbsTotal;
        _crumbsInInventory = d.crumbsInInventory;
        _socksCollected   = d.socksCollected;
    }

    public void WriteSaveData(SaveData d)
    {
        d.crumbsTotal      = _crumbsTotal;
        d.crumbsInInventory = _crumbsInInventory;
        d.socksCollected   = _socksCollected;
    }
}
