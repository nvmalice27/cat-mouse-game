using UnityEngine;

public class GalleryUI : MonoBehaviour
{
    [SerializeField] GallerySlot[] slots;
    [SerializeField] MouseTypeSO[] mouseTypes;

    void OnEnable()
    {
        GameEvents.OnMouseTypeUnlocked += OnUnlocked;
        RefreshAll();
    }

    void OnDisable() => GameEvents.OnMouseTypeUnlocked -= OnUnlocked;

    void OnUnlocked(int idx, bool _) => RefreshSlot(idx);

    void RefreshAll()
    {
        if (MouseStateManager.Instance == null) return;
        for (int i = 0; i < slots.Length; i++) RefreshSlot(i);
    }

    void RefreshSlot(int i)
    {
        if (i >= slots.Length || i >= mouseTypes.Length) return;
        slots[i].SetState(MouseStateManager.Instance.UnlockedTypes[i], mouseTypes[i]);
    }

    public void Close() => GameManager.Instance.NavigateTo("MainMenu");
}
