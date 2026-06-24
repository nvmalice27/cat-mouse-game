using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject slotPrefab;
    [SerializeField] Transform  container;
    [SerializeField] Sprite     crumbSprite;
    [SerializeField] Sprite     sockSprite;
    [SerializeField] Sprite     mealGoodSprite;
    [SerializeField] Sprite     mealBadSprite;

    void OnEnable()
    {
        GameEvents.OnInventoryChanged += Refresh;
        GameEvents.OnSocksChanged     += OnSocksChanged;
        Refresh();
    }

    void OnDisable()
    {
        GameEvents.OnInventoryChanged -= Refresh;
        GameEvents.OnSocksChanged     -= OnSocksChanged;
    }

    void OnSocksChanged(int _) => Refresh();

    void Refresh()
    {
        foreach (Transform c in container) Destroy(c.gameObject);

        var mgr = InventoryManager.Instance;
        if (mgr == null) return;

        if (mgr.CrumbsInInventory > 0)
            AddSlot(ItemType.Crumb, crumbSprite, mgr.CrumbsInInventory);

        if (mgr.SocksCollected >= 3)
            AddSlot(ItemType.Sock, sockSprite, 3);

        foreach (var item in mgr.Items)
        {
            Sprite icon = item.type == ItemType.MealGood ? mealGoodSprite : mealBadSprite;
            AddSlot(item.type, icon, item.quantity);
        }
    }

    void AddSlot(ItemType type, Sprite icon, int count)
    {
        var slot  = Instantiate(slotPrefab, container);
        slot.GetComponentInChildren<Image>().sprite = icon;
        var label = slot.GetComponentInChildren<TMP_Text>();
        if (label != null) label.text = count > 1 ? $"x{count}" : "";
        var dd = slot.GetComponent<DragDropItem>();
        if (dd != null) dd.SetItemType(type);
    }
}
