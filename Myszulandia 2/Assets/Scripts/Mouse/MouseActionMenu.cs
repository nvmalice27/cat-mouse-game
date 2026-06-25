using UnityEngine;

public class MouseActionMenu : MonoBehaviour
{
    [SerializeField] GameObject panel;

    public void Show()   => panel.SetActive(true);
    public void Hide()   => panel.SetActive(false);
    public void Toggle() => panel.SetActive(!panel.activeSelf);

    public void OnHug()  { MouseStateManager.Instance.TriggerHug();  Hide(); }
    public void OnKiss() { MouseStateManager.Instance.TriggerKiss(); Hide(); }
}
