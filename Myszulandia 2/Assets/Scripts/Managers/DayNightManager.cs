using UnityEngine;

public class DayNightManager : MonoBehaviour
{
    public static DayNightManager Instance { get; private set; }

    int       _dayNumber = 1;
    AlarmType _alarm     = AlarmType.None;
    bool      _dayActive = true;

    void Awake()
    {
        if (Instance != null) { Destroy(this); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public int       DayNumber => _dayNumber;
    public AlarmType Alarm     => _alarm;
    public bool      DayActive => _dayActive;

    public void SetAlarm(AlarmType alarm) => _alarm = alarm;

    public void Sleep()
    {
        if (!_dayActive) return;
        _dayActive = false;
        MouseStateManager.Instance.PauseTimers();

        bool isMakapaka = MouseStateManager.Instance.CurrentState == MouseState.Makapaka;
        string key = isMakapaka ? "NightMakapaka" : "Night";

        SaveManager.Instance.Save();
        GameEvents.RaiseDayEnded();
        GameEvents.RaiseCutsceneRequested(key);
    }

    public void OnNightCutsceneComplete()
    {
        _dayNumber++;
        EconomyManager.Instance.AddCoins(10);
        InventoryManager.Instance.ResetDailyItems();
        MouseStateManager.Instance.ResetForNewDay();
        MouseStateManager.Instance.TriggerAlarmWakeup(_alarm);
        _alarm    = AlarmType.None;
        _dayActive = true;
        GameEvents.RaiseDayChanged(_dayNumber);
        GameEvents.RaiseNewDayStarted();
    }

    public void ResetDay()
    {
        _dayActive = true;
        MouseStateManager.Instance.ResumeTimers();
        MouseStateManager.Instance.ResetForNewDay();
    }

    public void ApplySaveData(SaveData d)
    {
        _dayNumber = d.dayNumber;
        _alarm     = (AlarmType)d.alarmTypeValue;
        GameEvents.RaiseDayChanged(_dayNumber);
    }

    public void WriteSaveData(SaveData d)
    {
        d.dayNumber      = _dayNumber;
        d.alarmTypeValue = (int)_alarm;
    }
}
