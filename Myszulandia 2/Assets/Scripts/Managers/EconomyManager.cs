using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    EconomyLogic _logic = new(10);

    public int Coins => _logic.Coins;

    void Awake()
    {
        if (Instance != null) { Destroy(this); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool TrySpend(int amount)
    {
        bool ok = _logic.TrySpend(amount);
        if (ok) GameEvents.RaiseCoinsChanged(_logic.Coins);
        return ok;
    }

    public void AddCoins(int amount)
    {
        _logic.AddCoins(amount);
        GameEvents.RaiseCoinsChanged(_logic.Coins);
    }

    public void ApplySaveData(SaveData d) { _logic.SetCoins(d.coins); GameEvents.RaiseCoinsChanged(d.coins); }
    public void WriteSaveData(SaveData d)  => d.coins = _logic.Coins;
}
