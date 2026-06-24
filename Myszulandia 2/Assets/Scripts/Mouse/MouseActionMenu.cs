using UnityEngine;

public class MouseActionMenu : MonoBehaviour
{
    [SerializeField] GameObject panel;

    public void Show()   => panel.SetActive(true);
    public void Hide()   => panel.SetActive(false);
    public void Toggle() => panel.SetActive(!panel.activeSelf);

    public void OnHug()  { MouseStateManager.Instance.ApplyDirectAction(0); Hide(); }
    public void OnKiss() { MouseStateManager.Instance.ApplyDirectAction(1); Hide(); }
    public void OnPet()  { MouseStateManager.Instance.ApplyDirectAction(2); Hide(); }
    public void OnPlay() { MouseStateManager.Instance.ApplyDirectAction(3); Hide(); }
}
