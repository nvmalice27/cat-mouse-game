using System;

public static class GameEvents
{
    public static event Action<MouseState> OnMouseStateChanged;
    public static event Action<int>        OnCoinsChanged;
    public static event Action<int>        OnDayChanged;
    public static event Action             OnDayEnded;
    public static event Action             OnNewDayStarted;
    public static event Action<int>        OnCrumbsTotalChanged;
    public static event Action<int>        OnSocksChanged;
    public static event Action             OnInventoryChanged;
    public static event Action<int, bool>  OnMouseTypeUnlocked;
    public static event Action             OnGameOver;
    public static event Action<string>     OnCutsceneRequested;

    public static void RaiseMouseStateChanged(MouseState s)      => OnMouseStateChanged?.Invoke(s);
    public static void RaiseCoinsChanged(int v)                  => OnCoinsChanged?.Invoke(v);
    public static void RaiseDayChanged(int v)                    => OnDayChanged?.Invoke(v);
    public static void RaiseDayEnded()                           => OnDayEnded?.Invoke();
    public static void RaiseNewDayStarted()                      => OnNewDayStarted?.Invoke();
    public static void RaiseCrumbsTotalChanged(int v)            => OnCrumbsTotalChanged?.Invoke(v);
    public static void RaiseSocksChanged(int v)                  => OnSocksChanged?.Invoke(v);
    public static void RaiseInventoryChanged()                   => OnInventoryChanged?.Invoke();
    public static void RaiseMouseTypeUnlocked(int i, bool first) => OnMouseTypeUnlocked?.Invoke(i, first);
    public static void RaiseGameOver()                           => OnGameOver?.Invoke();
    public static void RaiseCutsceneRequested(string k)          => OnCutsceneRequested?.Invoke(k);
}
