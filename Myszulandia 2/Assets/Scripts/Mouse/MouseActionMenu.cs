using UnityEngine;

public class MouseActionMenu : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] GameObject bumpButton;

    public void Show()   => panel.SetActive(true);
    public void Hide()   => panel.SetActive(false);
    public void Toggle() => panel.SetActive(!panel.activeSelf);

    void OnEnable()  => GameEvents.OnMouseStateChanged += OnStateChanged;
    void OnDisable() => GameEvents.OnMouseStateChanged -= OnStateChanged;

    void Start() => RefreshBumpButton(MouseStateManager.Instance.CurrentState);

    void OnStateChanged(MouseState state)
    {
        RefreshBumpButton(state);
        // Zamknij menu jeśli wychodzi ze stanu Rozochocona (bumpcorzenie już niemożliwe)
        if (state != MouseState.Rozochocona && panel.activeSelf)
            Hide();
    }

    void RefreshBumpButton(MouseState state)
    {
        if (bumpButton != null)
            bumpButton.SetActive(state == MouseState.Rozochocona);
    }

    public void OnHug()  { Debug.Log("[OnHug] wywołane"); MouseStateManager.Instance.TriggerHug();  Hide(); }
    public void OnKiss() { MouseStateManager.Instance.TriggerKiss(); Hide(); }
    public void OnBump() { MouseStateManager.Instance.TriggerBump(); Hide(); }
}
