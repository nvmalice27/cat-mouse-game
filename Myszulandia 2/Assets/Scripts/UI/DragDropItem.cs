using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragDropItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public ItemType itemType;

    Image    _icon;
    Canvas   _canvas;
    GameObject _ghost;

    void Awake()
    {
        _icon   = GetComponentInChildren<Image>();
        _canvas = GetComponentInParent<Canvas>();
    }

    public void SetItemType(ItemType t) => itemType = t;

    public void OnBeginDrag(PointerEventData data)
    {
        _ghost = new GameObject("DragGhost");
        _ghost.transform.SetParent(_canvas.transform, false);
        var img = _ghost.AddComponent<Image>();
        img.sprite        = _icon.sprite;
        img.raycastTarget = false;
        (_ghost.transform as RectTransform).sizeDelta = new Vector2(60, 60);
    }

    public void OnDrag(PointerEventData data)
    {
        if (_ghost == null) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            data.position, _canvas.worldCamera, out Vector2 local);
        (_ghost.transform as RectTransform).localPosition = local;
    }

    public void OnEndDrag(PointerEventData data)
    {
        if (_ghost != null) { Destroy(_ghost); _ghost = null; }

        Vector3 world = Camera.main.ScreenToWorldPoint(data.position);
        world.z = 0;
        var hit = Physics2D.OverlapPoint(new Vector2(world.x, world.y));
        if (hit != null && hit.GetComponent<MouseController>() != null)
            UseOnMouse();
    }

    void UseOnMouse()
    {
        switch (itemType)
        {
            case ItemType.Crumb:     InventoryManager.Instance.UseCrumbOnMouse();        break;
            case ItemType.Sock:      InventoryManager.Instance.TryUseSocksOnMouse();     break;
            case ItemType.MealGood:  InventoryManager.Instance.TryUseMealOnMouse(true);  break;
            case ItemType.MealBad:   InventoryManager.Instance.TryUseMealOnMouse(false); break;
            case ItemType.Rose:      InventoryManager.Instance.UseRoseOnMouse();         break;
            case ItemType.Ticket:    InventoryManager.Instance.UseTicketOnMouse();       break;
            case ItemType.Garlic:    InventoryManager.Instance.UseGarlicOnMouse();       break;
            case ItemType.Drink:     InventoryManager.Instance.UseDrinkOnMouse();        break;
            case ItemType.MouseBall: InventoryManager.Instance.UseMouseBallOnMouse();    break;
        }
    }
}
