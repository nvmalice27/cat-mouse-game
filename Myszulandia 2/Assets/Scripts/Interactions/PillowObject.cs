using UnityEngine;

public class PillowObject : ClickableObject
{
    [SerializeField] GameObject confirmPanel;

    protected override void OnInteract() => confirmPanel.SetActive(true);

    public void ConfirmSleep()
    {
        confirmPanel.SetActive(false);
        DayNightManager.Instance.Sleep();
    }

    public void CancelSleep() => confirmPanel.SetActive(false);
}
