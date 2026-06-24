using UnityEngine;
using TMPro;

public class PhoneUI : MonoBehaviour
{
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject foodSubPanel;
    [SerializeField] GameObject alarmSubPanel;
    [SerializeField] TMP_Text   coinsPreview;

    public void Open()
    {
        mainPanel.SetActive(true);
        foodSubPanel.SetActive(false);
        alarmSubPanel.SetActive(false);
        if (coinsPreview != null)
            coinsPreview.text = $"Monety: {EconomyManager.Instance.Coins}";
    }

    public void Close() => mainPanel.SetActive(false);

    public void ShowFoodMenu()  => foodSubPanel.SetActive(true);
    public void ShowAlarmMenu() => alarmSubPanel.SetActive(true);

    public void OrderGoodFood()
    {
        if (!EconomyManager.Instance.TrySpend(50)) return;
        InventoryManager.Instance.AddMeal(true);
        Close();
    }

    public void OrderBadFood()
    {
        if (!EconomyManager.Instance.TrySpend(50)) return;
        InventoryManager.Instance.AddMeal(false);
        Close();
    }

    public void OrderRose()
    {
        if (!EconomyManager.Instance.TrySpend(40)) return;
        MouseStateManager.Instance.TriggerRose();
        GameEvents.RaiseCutsceneRequested("Date");
        Close();
    }

    public void OrderVacation()
    {
        if (!EconomyManager.Instance.TrySpend(300)) return;
        MouseStateManager.Instance.TriggerVacation();
        GameEvents.RaiseCutsceneRequested("Vacation");
        Close();
    }

    public void SetAlarmEarly()  { DayNightManager.Instance.SetAlarm(AlarmType.Early);  Close(); }
    public void SetAlarmNormal() { DayNightManager.Instance.SetAlarm(AlarmType.Normal); Close(); }
    public void SetAlarmNone()   { DayNightManager.Instance.SetAlarm(AlarmType.None);   Close(); }
}
