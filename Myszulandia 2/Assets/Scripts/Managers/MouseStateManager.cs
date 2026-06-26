using UnityEngine;

public class MouseStateManager : MonoBehaviour
{
    public static MouseStateManager Instance { get; private set; }

    // Stałe — zmień StatGrowRate dla balansu
    const float HungerGrowRate      = 1.00f;          // 0→100 w ~100s (drugi)
    const float AttentionGrowRate   = 1.30f;          // 0→100 w ~77s  (pierwszy)
    const float DirtGrowRate        = 100f / 120f;   // 0→100 w 2 minuty (~0.83/s, ostatni)
    const float NeedThreshold       = 60f;
    const float NeedMax             = 100f;
    const float InactivityTimeout   = 60f;
    const float BadStateTimeout     = 60f;
    const float ObrazedBlockDur     = 60f;
    const float ObrazedGraceDur     = 60f;
    const float CollectibleDuration = 60f;
    const int   UnlockedSize        = 27;

    // Staty
    float _hunger;
    float _attention;
    float _dirt;

    // Stan
    MouseState _state    = MouseState.Normal;
    bool[]     _unlocked = new bool[UnlockedSize];

    // Timery
    float _inactivityTimer;
    float _badStateTimer;
    float _obrazonaTimer;
    float _collectibleTimer;
    bool  _running = true;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Właściwości publiczne ─────────────────────────────────────────────

    public MouseState CurrentState  => _state;
    public float      Hunger        => _hunger;
    public float      Attention     => _attention;
    public float      Dirt          => _dirt;
    public bool[]     UnlockedTypes => _unlocked;

    public bool IsHungry()  => _hunger    >= NeedThreshold;
    public bool IsDirty()   => _dirt      >= NeedThreshold;
    public bool IsWanting() => _attention >= NeedThreshold;

    // ── Kontrola timerów ─────────────────────────────────────────────────

    public void PauseTimers()  => _running = false;
    public void ResumeTimers() => _running = true;

    public void ResetForNewDay()
    {
        _hunger = _attention = _dirt = 0f;
        _inactivityTimer = _badStateTimer = _obrazonaTimer = _collectibleTimer = 0f;
        SetState(MouseState.Normal);
        _running = true;
    }

    // ── Zapis/Odczyt ─────────────────────────────────────────────────────

    public void ApplySaveData(SaveData d)
    {
        _hunger    = d.hunger;
        _attention = d.attention;
        _dirt      = d.dirt;
        _state     = (MouseState)d.mouseStateValue;
        var saved  = d.unlockedMouseTypes;
        _unlocked  = new bool[UnlockedSize];
        for (int i = 0; i < Mathf.Min(saved.Length, _unlocked.Length); i++)
            _unlocked[i] = saved[i];
        GameEvents.RaiseMouseStateChanged(_state);
    }

    public void WriteSaveData(SaveData d)
    {
        d.hunger             = _hunger;
        d.attention          = _attention;
        d.dirt               = _dirt;
        d.mouseStateValue    = (int)_state;
        d.unlockedMouseTypes = (bool[])_unlocked.Clone();
    }

    // ── Update ───────────────────────────────────────────────────────────

    void Update()
    {
        if (!_running) return;
        float dt = Time.deltaTime;
        UpdateStats(dt);
        UpdateTimers(dt);
    }

    void UpdateStats(float dt)
    {
        if (IsStatsFrozen()) return;

        _hunger    = Mathf.Min(NeedMax, _hunger    + HungerGrowRate    * dt);
        _attention = Mathf.Min(NeedMax, _attention + AttentionGrowRate * dt);
        _dirt      = Mathf.Min(NeedMax, _dirt      + DirtGrowRate      * dt);

        // Którykolwiek stat = 100 → Złowroga
        if (_hunger >= NeedMax || _attention >= NeedMax || _dirt >= NeedMax)
        {
            ResetAllStats();
            EnterBadState(MouseState.Zlowroga);
            return;
        }

        // Aktywny stan potrzeby + drugi stat przekracza 60 → Złowroga
        if (IsNeedState(_state))
        {
            bool second = (_hunger    >= NeedThreshold && _state != MouseState.Hungry)   ||
                          (_attention >= NeedThreshold && _state != MouseState.Chcaca)    ||
                          (_dirt      >= NeedThreshold && _state != MouseState.Smrodliwa);
            if (second) { ResetAllStats(); EnterBadState(MouseState.Zlowroga); }
            return;
        }

        // Nowy stan potrzeby — priorytet: głód, potem uwaga, potem brud
        if (_hunger    >= NeedThreshold) { EnterNeedState(MouseState.Hungry);    return; }
        if (_attention >= NeedThreshold) { EnterNeedState(MouseState.Chcaca);    return; }
        if (_dirt      >= NeedThreshold) { EnterNeedState(MouseState.Smrodliwa); return; }
    }

    void UpdateTimers(float dt)
    {
        // Stany pozytywne (normalne + kolekcjonerskie + potrzeb)
        if (IsPositiveOrNeed(_state))
        {
            // Timer aktywności — tylko gdy nie w stanie potrzeby
            if (!IsNeedState(_state))
            {
                _inactivityTimer += dt;
                if (_inactivityTimer >= InactivityTimeout)
                {
                    _inactivityTimer = 0f;
                    EnterBadState(MouseState.Smutna);
                    return;
                }
            }

            // Timer kolekcjonerski — nie dla stanów nieskończonych
            if (IsTimedCollectible(_state))
            {
                _collectibleTimer += dt;
                if (_collectibleTimer >= CollectibleDuration)
                {
                    _collectibleTimer = 0f;
                    ReturnToBase();
                }
            }
            return;
        }

        // Złe stany
        if (IsBadState(_state))
        {
            _badStateTimer += dt;
            if (_badStateTimer >= BadStateTimeout)
            {
                _badStateTimer = 0f;
                AdvanceBadState();
            }
            return;
        }

        // Obrażona — dwufazowa
        if (_state == MouseState.Obrazona)
        {
            _obrazonaTimer += dt;
            // Faza 2 (łaska) przekroczyła czas → Złowroga przez brak aktywności
            if (_obrazonaTimer >= ObrazedBlockDur + ObrazedGraceDur)
            {
                _obrazonaTimer = 0f;
                EnterBadState(MouseState.Zlowroga);
            }
        }
    }

    // ── Klasyfikacja stanów ───────────────────────────────────────────────

    bool IsStatsFrozen()  => IsBadState(_state) || _state == MouseState.Obrazona;

    static bool IsNeedState(MouseState s) =>
        s == MouseState.Hungry || s == MouseState.Chcaca || s == MouseState.Smrodliwa;

    static bool IsBadState(MouseState s) =>
        s is MouseState.Smutna or MouseState.Zrozpaczona or
             MouseState.Zlowroga or MouseState.Sciekla or MouseState.ScieklaII;

    static bool IsPositiveOrNeed(MouseState s) => !IsBadState(s) && s != MouseState.Obrazona;

    static bool IsTimedCollectible(MouseState s) =>
        IsPositiveOrNeed(s) && !IsNeedState(s) &&
        s != MouseState.Normal && s != MouseState.Pirat && s != MouseState.Myszkujaca;

    // ── Przejścia stanów ──────────────────────────────────────────────────

    void EnterNeedState(MouseState s)
    {
        SetState(s);
        _inactivityTimer  = 0f;
        _collectibleTimer = 0f;
        TryUnlock(s);
    }

    void EnterBadState(MouseState bad)
    {
        ResetAllStats();
        SetState(bad);
        _badStateTimer    = 0f;
        _inactivityTimer  = 0f;
        _collectibleTimer = 0f;
        TryUnlock(bad);
        if (bad == MouseState.ScieklaII) TriggerGameOver();
    }

    void AdvanceBadState()
    {
        MouseState next = _state switch
        {
            MouseState.Smutna      => MouseState.Zrozpaczona,
            MouseState.Zrozpaczona => MouseState.Zlowroga,
            MouseState.Zlowroga    => MouseState.Sciekla,
            MouseState.Sciekla     => MouseState.ScieklaII,
            _                      => MouseState.ScieklaII
        };
        EnterBadState(next);
    }

    void RecoverBadState()
    {
        MouseState prev = _state switch
        {
            MouseState.Smutna      => MouseState.Normal,
            MouseState.Zrozpaczona => MouseState.Smutna,
            MouseState.Zlowroga    => MouseState.Zrozpaczona,
            MouseState.Sciekla     => MouseState.Zlowroga,
            _                      => MouseState.Normal
        };
        SetState(prev);
        _badStateTimer   = 0f;
        _inactivityTimer = 0f;
    }

    void EnterCollectible(MouseState s)
    {
        SetState(s);
        _collectibleTimer = 0f;
        _inactivityTimer  = 0f;
        TryUnlock(s);
    }

    void EnterObraziona()
    {
        SetState(MouseState.Obrazona);
        _obrazonaTimer   = 0f;
        _inactivityTimer = 0f;
        TryUnlock(MouseState.Obrazona);
    }

    void ReturnToBase()
    {
        MouseState target = IsHungry()  ? MouseState.Hungry    :
                            IsWanting() ? MouseState.Chcaca    :
                            IsDirty()   ? MouseState.Smrodliwa :
                                          MouseState.Normal;
        SetState(target);
        _collectibleTimer = 0f;
        _inactivityTimer  = 0f;
    }

    void ResetAllStats() => _hunger = _attention = _dirt = 0f;

    void SetState(MouseState s)
    {
        if (_state == s) return;
        _state = s;
        GameEvents.RaiseMouseStateChanged(s);
    }

    void TryUnlock(MouseState s)
    {
        int idx = CollectibleIndex(s);
        if (idx < 0 || idx >= UnlockedSize) return;
        bool isFirst = !_unlocked[idx];
        _unlocked[idx] = true;
        GameEvents.RaiseMouseTypeUnlocked(idx, isFirst);
    }

    void TriggerGameOver()
    {
        _running = false;
        GameEvents.RaiseGameOver();
        GameEvents.RaiseCutsceneRequested("GameOver");
    }

    void OnActivity() => _inactivityTimer = 0f;

    // Helpery do sprawdzenia fazy Obrażonej
    bool IsBlocked()      => _state == MouseState.Obrazona && _obrazonaTimer < ObrazedBlockDur;
    bool IsGracePeriod()  => _state == MouseState.Obrazona && _obrazonaTimer >= ObrazedBlockDur;

    // Podczas fazy łaski: złe akcje → Złowroga; dobre/neutralne → ignorowane (return true = przerwij)
    bool HandleGrace(bool isEvil = false)
    {
        if (!IsGracePeriod()) return false;
        if (isEvil) EnterBadState(MouseState.Zlowroga);
        return true;
    }

    // Rozochocona + zła akcja → Obrażona
    bool HandleRozochwana()
    {
        if (_state != MouseState.Rozochocona) return false;
        EnterObraziona();
        return true;
    }

    // ── Mapowanie galerii ────────────────────────────────────────────────

    public static int CollectibleIndex(MouseState s) => s switch
    {
        MouseState.Heppi           => 0,
        MouseState.Czonstkujaca    => 1,
        MouseState.Grobol          => 2,
        MouseState.Obrazona        => 3,
        MouseState.Paczurowa       => 4,
        MouseState.Smrodliwa       => 5,
        MouseState.Makapaka        => 6,
        MouseState.Pirat           => 7,
        MouseState.Niewyspana      => 8,
        MouseState.WesolaPoPobudce => 9,
        MouseState.Myszkujaca      => 10,
        MouseState.Tanczaca        => 11,
        MouseState.Pachnaca        => 12,
        MouseState.Czonstkowa      => 13,
        MouseState.Pumpuzka        => 14,
        MouseState.Roztopiona      => 15,
        MouseState.Spankowa        => 16,
        MouseState.Czosnkowa       => 17,
        MouseState.Zakochana       => 18,
        MouseState.Rozochocona     => 19,
        MouseState.Hungry          => 20,
        MouseState.Chcaca          => 21,
        MouseState.Smutna          => 22,
        MouseState.Zrozpaczona     => 23,
        MouseState.Zlowroga        => 24,
        MouseState.Sciekla         => 25,
        MouseState.ScieklaII       => 26,
        _                          => -1
    };

    // ── Akcje gracza ─────────────────────────────────────────────────────

    public void TriggerHug()
    {
        if (IsBlocked()) return;
        OnActivity();

        if (IsGracePeriod())        { _obrazonaTimer = 0f; ReturnToBase(); return; }
        if (HandleRozochwana())     return;
        if (IsBadState(_state))     { RecoverBadState(); return; }
        if (_state == MouseState.Hungry || _state == MouseState.Smrodliwa) return;
        if (_state == MouseState.Chcaca) { _attention = 0f; EnterCollectible(MouseState.Pumpuzka); return; }
        EnterCollectible(MouseState.Heppi);
    }

    public void TriggerKiss()
    {
        if (IsBlocked()) return;
        OnActivity();

        if (IsGracePeriod())        { _obrazonaTimer = 0f; ReturnToBase(); return; }
        if (HandleRozochwana())     return;
        if (IsBadState(_state))     { RecoverBadState(); return; }
        if (IsNeedState(_state))    return; // całowanie nie rozwiązuje żadnego stanu potrzeby
        EnterCollectible(MouseState.Heppi);
    }

    public void Feed(bool isGood)
    {
        if (IsBlocked()) return;
        OnActivity();

        if (HandleGrace(isEvil: !isGood)) return; // złe jedzenie w łasce → Złowroga; dobre → ignorowane
        if (HandleRozochwana()) return;
        if (IsBadState(_state)) return;

        if (!isGood) { EnterObraziona(); return; }

        if (_state == MouseState.Hungry)
        {
            _hunger = 0f;
            EnterCollectible(MouseState.Czonstkujaca);
            return;
        }
        if (IsNeedState(_state)) { ResetAllStats(); EnterBadState(MouseState.Zlowroga); return; }
        EnterCollectible(MouseState.Grobol);
    }

    public void TriggerWash()
    {
        if (IsBlocked()) return;
        OnActivity();

        if (HandleRozochwana()) return;
        if (IsBadState(_state)) return;

        if (_state == MouseState.Smrodliwa) { _dirt = 0f; EnterCollectible(MouseState.Pachnaca); return; }
        // czysta mysz — brak efektu
    }

    public void TriggerBike()
    {
        OnActivity();
        if (IsBlocked() || HandleGrace() || IsBadState(_state)) return;
        if (HandleRozochwana()) return;
        if (_dirt >= NeedThreshold) return; // już brudna — brak efektu

        _dirt = NeedThreshold;
        if (IsNeedState(_state)) { ResetAllStats(); EnterBadState(MouseState.Zlowroga); return; }
        EnterNeedState(MouseState.Smrodliwa);
    }

    public void TriggerCrumbs()
    {
        if (IsBlocked()) return;
        OnActivity();
        if (HandleGrace(isEvil: true)) return; // okruszki w łasce → Złowroga
        if (IsBadState(_state)) return;
        EnterObraziona();
    }

    public void TriggerMusic(bool isGood)
    {
        if (IsBlocked()) return;
        OnActivity();

        if (HandleGrace(isEvil: !isGood)) return; // zła muzyka w łasce → Złowroga; dobra → ignorowane
        if (!isGood) { if (!IsBadState(_state)) EnterObraziona(); return; }
        if (HandleRozochwana()) return;
        if (IsBadState(_state)) return;
        EnterCollectible(MouseState.Tanczaca);
    }

    public void TriggerCandles()
    {
        OnActivity();
        if (IsBlocked() || HandleGrace() || IsBadState(_state)) return;
        if (HandleRozochwana()) return;
        EnterCollectible(MouseState.Rozochocona);
    }

    public void TriggerBump()
    {
        OnActivity();
        if (IsBlocked() || HandleGrace() || IsBadState(_state)) return;
        // Bump NIE wywołuje HandleRozochwana — to jedyna poprawna akcja w Rozochocona
        if (_state == MouseState.Rozochocona) { EnterCollectible(MouseState.Roztopiona); return; }
        // z innego stanu — brak efektu
    }

    public void TriggerGarlic()
    {
        OnActivity();
        if (IsBlocked() || HandleGrace() || IsBadState(_state)) return;
        if (HandleRozochwana()) return;
        EnterCollectible(MouseState.Czosnkowa);
    }

    public void TriggerRose()
    {
        OnActivity();
        if (IsBlocked() || HandleGrace() || IsBadState(_state)) return;
        if (HandleRozochwana()) return;
        EnterCollectible(MouseState.Zakochana);
        GameEvents.RaiseCutsceneRequested("Rose");
    }

    public void TriggerVacation()
    {
        OnActivity();
        if (IsBlocked() || HandleGrace() || IsBadState(_state)) return;
        if (HandleRozochwana()) return;
        EnterCollectible(MouseState.Paczurowa);
        GameEvents.RaiseCutsceneRequested("Vacation");
    }

    public void TriggerSleep()
    {
        OnActivity();
        if (IsBlocked() || HandleGrace() || IsBadState(_state)) return;
        if (HandleRozochwana()) return;
        EnterCollectible(MouseState.Spankowa);
    }

    public void TriggerMakapaka()
    {
        OnActivity();
        if (IsBlocked() || HandleGrace() || IsBadState(_state)) return;
        if (HandleRozochwana()) return;
        EnterCollectible(MouseState.Makapaka);
    }

    public void TriggerCzonstkowa()
    {
        OnActivity();
        if (IsBlocked() || HandleGrace() || IsBadState(_state)) return;
        if (HandleRozochwana()) return;
        EnterCollectible(MouseState.Czonstkowa);
    }

    public void TriggerPirat()
    {
        OnActivity();
        if (IsBlocked() || HandleGrace() || IsBadState(_state)) return;
        if (HandleRozochwana()) return;
        if (_state != MouseState.Pirat) EnterCollectible(MouseState.Pirat);
    }

    public void ClearPirat()
    {
        OnActivity();
        if (_state == MouseState.Pirat) ReturnToBase();
    }

    public void TriggerKitchenEntry()
    {
        OnActivity();
        if (IsBadState(_state) || IsNeedState(_state) || _state == MouseState.Obrazona) return;
        if (HandleRozochwana()) return;
        SetState(MouseState.Myszkujaca);
    }

    public void TriggerKitchenExit()
    {
        OnActivity();
        if (_state == MouseState.Myszkujaca) ReturnToBase();
    }

    public void TriggerAlarmWakeup(AlarmType alarm)
    {
        MouseState s = alarm == AlarmType.Early ? MouseState.Niewyspana : MouseState.WesolaPoPobudce;
        EnterCollectible(s);
    }
}
