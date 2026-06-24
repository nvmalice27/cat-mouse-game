using UnityEngine;
using TMPro;

public class HUDController : MonoBehaviour
{
    [SerializeField] TMP_Text coinsText;
    [SerializeField] TMP_Text dayText;

    void OnEnable()
    {
        GameEvents.OnCoinsChanged += SetCoins;
        GameEvents.OnDayChanged   += SetDay;
    }

    void OnDisable()
    {
        GameEvents.OnCoinsChanged -= SetCoins;
        GameEvents.OnDayChanged   -= SetDay;
    }

    void Start()
    {
        SetCoins(EconomyManager.Instance.Coins);
        SetDay(DayNightManager.Instance.DayNumber);
    }

    void SetCoins(int v) => coinsText.text = $"Monety: {v}";
    void SetDay(int v)   => dayText.text   = $"Dzień {v}";
}
