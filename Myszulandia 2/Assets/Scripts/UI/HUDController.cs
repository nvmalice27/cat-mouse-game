using UnityEngine;
using TMPro;

public class HUDController : MonoBehaviour
{
    [SerializeField] TMP_Text coinsText;
    [SerializeField] TMP_Text dayText;
    [SerializeField] TMP_Text hungerText;
    [SerializeField] TMP_Text attentionText;
    [SerializeField] TMP_Text dirtText;
    [SerializeField] TMP_Text stateText;

    void OnEnable()
    {
        GameEvents.OnCoinsChanged    += SetCoins;
        GameEvents.OnDayChanged      += SetDay;
        GameEvents.OnMouseStateChanged += SetState;
        InvokeRepeating(nameof(RefreshStats), 0f, 0.5f);
    }

    void OnDisable()
    {
        GameEvents.OnCoinsChanged    -= SetCoins;
        GameEvents.OnDayChanged      -= SetDay;
        GameEvents.OnMouseStateChanged -= SetState;
        CancelInvoke(nameof(RefreshStats));
    }

    void Start()
    {
        SetCoins(EconomyManager.Instance.Coins);
        SetDay(DayNightManager.Instance.DayNumber);
        SetState(MouseStateManager.Instance.CurrentState);
    }

    void SetCoins(int v)           => coinsText.text = $"Monety: {v}";
    void SetDay(int v)             => dayText.text   = $"Dzień {v}";
    void SetState(MouseState s)    { if (stateText != null) stateText.text = $"Stan: {s}"; }

    void RefreshStats()
    {
        var mgr = MouseStateManager.Instance;
        if (mgr == null) return;
        if (hungerText    != null) hungerText.text    = $"Głód:  {mgr.Hunger:F1}/100";
        if (attentionText != null) attentionText.text = $"Uwaga: {mgr.Attention:F1}/100";
        if (dirtText      != null) dirtText.text      = $"Brud:  {mgr.Dirt:F1}/100";
    }
}
