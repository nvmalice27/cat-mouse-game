# Przepisanie Systemu Statów Myszy — Plan Implementacji

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Przepisać MouseStateManager.cs od zera — dodać stat uwagi, uprościć maszyn stanów i ujednolicić logikę zgodnie z nową specyfikacją.

**Architecture:** Singleton MonoBehaviour zarządza 3 statami (float 0–100), maszyną stanów (enum MouseState), i 4 timerami (aktywność, zły stan, Obrażona, kolekcjonerski). Akcje gracza = publiczne metody. Każda zmiana stanu → `GameEvents.RaiseMouseStateChanged`. Galeria korzysta z `CollectibleIndex()` do mapowania stanów na indeksy slotów.

**Tech Stack:** Unity 2021+, C#, NUnit (Unity Test Runner — Edit Mode)

## Global Constraints
- `StatGrowRate = 1f / 60f` (~1 pkt/sek) — do regulacji przez balancing
- `NeedThreshold = 60f`, `NeedMax = 100f`
- `InactivityTimeout = 60f`, `BadStateTimeout = 60f`
- `ObrazedBlockDur = 60f`, `ObrazedGraceDur = 60f`
- `CollectibleDuration = 60f`
- Rozmiar `_unlocked` = 27 (indeksy galerii 0–26)
- Zachować istniejące publiczne sygnatury: `TriggerBike`, `TriggerPirat`, `ClearPirat`, `IsDirty`, `TriggerWash`, `PauseTimers`, `ResumeTimers`, `CurrentState`, `ResetForNewDay`, `TriggerAlarmWakeup`, `ApplySaveData`, `WriteSaveData`, `Feed`, `TriggerRose`, `TriggerVacation`, `TriggerMakapaka`, `TriggerCzonstkowa`, `TriggerKitchenEntry`, `TriggerKitchenExit`
- `ApplyDirectAction(int)` i `ApplyNegativeAction()` — USUNĄĆ (nie są już potrzebne)

---

## Pliki do zmiany

| Plik | Operacja |
|------|----------|
| `Assets/Scripts/Core/MouseState.cs` | Modyfikacja — nowe stany |
| `Assets/Scripts/Core/SaveData.cs` | Modyfikacja — pole `attention` |
| `Assets/Scripts/Managers/MouseStateManager.cs` | Przepisanie od zera |
| `Assets/Scripts/Mouse/MouseActionMenu.cs` | Modyfikacja — semantyczne metody |
| `Assets/Scripts/UI/RadioUI.cs` | Modyfikacja — `TriggerMusic(bool)` |
| `Assets/Scripts/Managers/DayNightManager.cs` | Modyfikacja — wywołanie `TriggerSleep()` |
| (Inspector w Unity) `GalleryUI.mouseTypes[]` | Re-mapowanie ScriptableObject w inspektorze |

---

### Task 1: Zaktualizuj MouseState.cs — dodaj nowe stany

**Files:**
- Modify: `Assets/Scripts/Core/MouseState.cs`

**Interfaces:**
- Produces: nowe wartości enum używane przez MouseStateManager, GalleryUI

- [ ] **Step 1: Zastąp całą zawartość pliku nową wersją**

```csharp
public enum MouseState
{
    Normal           = 0,
    Hungry           = 1,   // potrzeba: głód >= 60
    Rozochocona      = 2,
    Chcaca           = 3,   // potrzeba: uwaga >= 60
    Zrozpaczona      = 4,   // zły stan (łańcuch)
    Heppi            = 5,   // tulanie/całowanie w stanie normalnym

    Zakochana        = 10,  // róża (cutscene)
    Czonstkujaca     = 11,  // nakarmienie głodnej
    Grobol           = 12,  // nakarmienie nie-głodnej
    Obrazona         = 13,  // zła akcja (2-fazowa blokada)
    Paczurowa        = 14,  // bilet lotniczy (cutscene)
    // 15 = zarezerwowane (nieużywane)
    Smrodliwa        = 16,  // potrzeba: brud >= 60
    Makapaka         = 17,  // 3 skarpetki
    Pirat            = 18,  // łóżko (trzyma do akcji)
    Niewyspana       = 19,  // za wczesny budzik
    WesolaPoPobudce  = 20,  // normalny budzik
    Myszkujaca       = 21,  // kuchnia (trzyma do wyjścia)
    Tanczaca         = 22,  // dobra muzyka
    Pachnaca         = 23,  // mycie brudnej
    Czonstkowa       = 24,  // 100 czonstek
    Pumpuzka         = 25,  // tulanie chcącej
    Roztopiona       = 26,  // rozochocona + bumpcorzenie
    Spankowa         = 27,  // poduszka/sen
    Czosnkowa        = 28,  // czosnek

    Smutna           = 30,  // zły stan — łańcuch poziom 1
    Zlowroga         = 31,  // zły stan — łańcuch poziom 3
    Sciekla          = 32,  // zły stan — łańcuch poziom 4
    ScieklaII        = 33   // game over
}

public enum AlarmType { None = 0, Early = 1, Normal = 2 }
```

- [ ] **Step 2: Zweryfikuj kompilację w Unity**

Otwórz Unity, poczekaj na recompile. Sprawdź Console — nie powinno być błędów związanych z MouseState. Mogą pojawić się błędy w MouseStateManager.cs (stary plik odwołuje się do usuniętych stanów) — to normalne, zostanie naprawione w Task 3.

- [ ] **Step 3: Commit**

```
git add "Assets/Scripts/Core/MouseState.cs"
git commit -m "feat: expand MouseState enum — add Heppi, Czonstkujaca, Smrodliwa, Pumpuzka, Roztopiona, Spankowa, Czosnkowa"
```

---

### Task 2: Zaktualizuj SaveData.cs — dodaj pole attention

**Files:**
- Modify: `Assets/Scripts/Core/SaveData.cs`

**Interfaces:**
- Produces: `SaveData.attention` (float) używane przez MouseStateManager

- [ ] **Step 1: Dodaj pole attention i zaktualizuj domyślny rozmiar unlockedMouseTypes**

```csharp
using System;

[Serializable]
public class SaveData
{
    public int    coins               = 10;
    public int    dayNumber           = 1;
    public float  hunger              = 0f;
    public float  attention           = 0f;   // NOWE
    public float  dirt                = 0f;
    public int    mouseStateValue     = 0;
    public bool[] unlockedMouseTypes  = new bool[27];  // rozszerzone z 21 do 27
    public int    crumbsTotal         = 0;
    public int    crumbsInInventory   = 0;
    public int    socksCollected      = 0;
    public int    alarmTypeValue      = 0;
    public bool   bedMade             = false;
}
```

- [ ] **Step 2: Zweryfikuj kompilację — brak błędów**

- [ ] **Step 3: Commit**

```
git add "Assets/Scripts/Core/SaveData.cs"
git commit -m "feat: add attention field to SaveData, expand unlockedMouseTypes to 27"
```

---

### Task 3: Przepisz MouseStateManager.cs od zera

**Files:**
- Rewrite: `Assets/Scripts/Managers/MouseStateManager.cs`

**Interfaces:**
- Consumes: `MouseState` (Task 1), `SaveData.attention` (Task 2), `GameEvents`, `AlarmType`
- Produces: wszystkie publiczne metody i właściwości wymienione w Global Constraints

- [ ] **Step 1: Usuń całą zawartość starego pliku i wklej szkielet + stałe + pola**

```csharp
using UnityEngine;

public class MouseStateManager : MonoBehaviour
{
    public static MouseStateManager Instance { get; private set; }

    // Stałe — zmień StatGrowRate dla balansu
    const float StatGrowRate        = 1f / 60f;
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
}
```

- [ ] **Step 2: Zweryfikuj kompilację (błędy z brakującymi metodami to OK na razie)**

- [ ] **Step 3: Dodaj Update() z logiką statów**

Dopisz pod `WriteSaveData`:

```csharp
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

        _hunger    = Mathf.Min(NeedMax, _hunger    + StatGrowRate * dt);
        _attention = Mathf.Min(NeedMax, _attention + StatGrowRate * dt);
        _dirt      = Mathf.Min(NeedMax, _dirt      + StatGrowRate * dt);

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
```

- [ ] **Step 4: Dodaj helpery klasyfikacji stanów i przejścia**

Dopisz pod `UpdateTimers`:

```csharp
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
```

- [ ] **Step 5: Dodaj wszystkie publiczne akcje gracza**

Dopisz pod `CollectibleIndex`:

```csharp
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
        if (IsNeedState(_state)) { ResetAllStats(); EnterBadState(MouseState.Zlowroga); return; }
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
```

Zamknij klasę: dodaj `}` na końcu pliku.

- [ ] **Step 6: Zweryfikuj kompilację w Unity — zero błędów**

Jeśli są błędy związane ze starymi metodami (`ApplyDirectAction`, `ApplyNegativeAction`, `CollectibleIndex` ze starą sygnaturą) — zostają naprawione w kolejnych taskach.

- [ ] **Step 7: Commit**

```
git add "Assets/Scripts/Managers/MouseStateManager.cs"
git commit -m "feat: rewrite MouseStateManager — 3-stat need system, new state machine"
```

---

### Task 4: Zaktualizuj MouseActionMenu.cs

**Files:**
- Modify: `Assets/Scripts/Mouse/MouseActionMenu.cs`

**Interfaces:**
- Consumes: `MouseStateManager.TriggerHug()`, `MouseStateManager.TriggerKiss()`

- [ ] **Step 1: Zastąp całą zawartość pliku**

```csharp
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
```

> **Uwaga:** Przyciski OnPet i OnPlay w prefabie menu akcji należy usunąć lub odpiąć w Inspektorze — te akcje nie istnieją w nowym projekcie.

- [ ] **Step 2: Zweryfikuj kompilację — zero błędów**

- [ ] **Step 3: Commit**

```
git add "Assets/Scripts/Mouse/MouseActionMenu.cs"
git commit -m "refactor: MouseActionMenu — replace ApplyDirectAction with TriggerHug/TriggerKiss"
```

---

### Task 5: Zaktualizuj RadioUI.cs

**Files:**
- Modify: `Assets/Scripts/UI/RadioUI.cs`

**Interfaces:**
- Consumes: `MouseStateManager.TriggerMusic(bool isGood)`

- [ ] **Step 1: Zastąp całą zawartość pliku**

```csharp
using UnityEngine;

public class RadioUI : MonoBehaviour
{
    [SerializeField] GameObject panel;

    public void Open()  => panel.SetActive(true);
    public void Close() { if (panel != null) panel.SetActive(false); }

    // Dobra muzyka → Tańcząca
    public void PlayPop()       { MouseStateManager.Instance.TriggerMusic(true);  Close(); }
    public void PlaySpokojna()  { MouseStateManager.Instance.TriggerMusic(true);  Close(); }
    public void PlayKlasyczna() { MouseStateManager.Instance.TriggerMusic(true);  Close(); }

    // Zła muzyka → Obrażona
    public void PlayMetal()     { MouseStateManager.Instance.TriggerMusic(false); Close(); }
}
```

- [ ] **Step 2: Zweryfikuj kompilację — zero błędów**

- [ ] **Step 3: Commit**

```
git add "Assets/Scripts/UI/RadioUI.cs"
git commit -m "refactor: RadioUI — TriggerMusic(bool) replaces TriggerMusic()/ApplyNegativeAction()"
```

---

### Task 6: Zaktualizuj DayNightManager.cs — wywołaj TriggerSleep przed nocną cutscenką

**Files:**
- Modify: `Assets/Scripts/Managers/DayNightManager.cs`

**Interfaces:**
- Consumes: `MouseStateManager.TriggerSleep()`

- [ ] **Step 1: W metodzie Sleep() dodaj wywołanie TriggerSleep() przed PauseTimers**

Znajdź metodę `Sleep()` i zmień ją na:

```csharp
    public void Sleep()
    {
        if (!_dayActive) return;
        _dayActive = false;
        MouseStateManager.Instance.TriggerSleep();   // DODANE — mysz wchodzi w stan Spankowa
        MouseStateManager.Instance.PauseTimers();

        bool isMakapaka = MouseStateManager.Instance.CurrentState == MouseState.Makapaka;
        string key = isMakapaka ? "NightMakapaka" : "Night";

        SaveManager.Instance.Save();
        GameEvents.RaiseDayEnded();
        GameEvents.RaiseCutsceneRequested(key);
    }
```

- [ ] **Step 2: Zweryfikuj kompilację — zero błędów**

- [ ] **Step 3: Commit**

```
git add "Assets/Scripts/Managers/DayNightManager.cs"
git commit -m "feat: call TriggerSleep before night cutscene — adds Spankowa state"
```

---

### Task 7: Zaktualizuj galerię w Inspektorze Unity

**Files:**
- (Inspector) GameObject z komponentem `GalleryUI` → pole `mouseTypes[]`

> **Kontekst:** `GalleryUI` wyświetla sloty według indeksów 0–26. Nowe mapowanie to `MouseStateManager.CollectibleIndex()`. Kolejność slotów zmieniła się względem starej wersji — array `mouseTypes[]` w Inspektorze musi być ułożony w nowej kolejności.

- [ ] **Step 1: Sprawdź obecną kolejność slotów**

W Inspektorze Unity otwórz prefab/scenę z GalleryUI. Sprawdź tablicę `mouseTypes[]` — ile ma elementów i w jakiej kolejności.

- [ ] **Step 2: Ułóż mouseTypes[] w nowej kolejności**

Nowe mapowanie galerii (indeks → stan):

| Indeks | Stan |
|--------|------|
| 0 | Heppi |
| 1 | Czonstkująca |
| 2 | Grobol |
| 3 | Obrażona |
| 4 | Pączurowa |
| 5 | Smrodliwa |
| 6 | Maka Paka |
| 7 | Pirat |
| 8 | Niewyspana |
| 9 | Wesoła po pobudce |
| 10 | Myszkująca |
| 11 | Tańcząca |
| 12 | Pachnąca |
| 13 | Czonstkowa |
| 14 | Pumpużka |
| 15 | Roztopiona |
| 16 | Spankowa |
| 17 | Czosnkowa |
| 18 | Zakochana |
| 19 | Rozochocona |
| 20 | Głodna |
| 21 | Chcąca |
| 22 | Smutna |
| 23 | Zrozpaczona |
| 24 | Złowroga |
| 25 | Ściekła |
| 26 | Ściekła II |

> **Uwaga dla artysty:** Stany 14–18 (Pumpużka, Roztopiona, Spankowa, Czosnkowa) są NOWE — będą potrzebować nowych ScriptableObject MouseTypeSO z odpowiednimi sprite'ami i opisami.

- [ ] **Step 3: Utwórz brakujące MouseTypeSO dla nowych stanów**

Dla każdego nowego stanu (Pumpużka, Roztopiona, Spankowa, Czosnkowa, Czonstkująca): prawym przyciskiem w Project → Create → (ścieżka do MouseTypeSO) → wypełnij `displayName`, `galleryHint`, `sprite`.

- [ ] **Step 4: Przetestuj galerię**

Uruchom grę w Play Mode. Wywołaj ręcznie `MouseStateManager.Instance.TryUnlock(MouseState.Heppi)` z konsoli lub testowego przycisku. Sprawdź czy slot 0 w galerii się odblokował i wyświetla poprawny sprite.

- [ ] **Step 5: Commit sceny/prefabów**

```
git add Assets/
git commit -m "feat: remap GalleryUI mouseTypes[] to new 27-slot CollectibleIndex order"
```

---

## Weryfikacja końcowa (Play Mode)

Po ukończeniu wszystkich tasków przetestuj następujące ścieżki w grze:

| Scenariusz | Oczekiwany wynik |
|-----------|-----------------|
| Czekaj 60 sek bez akcji | Mysz → Smutna |
| Smutna + tulanie | Mysz → Neutralna |
| Głód rośnie do 60 | Mysz → Głodna (przerywa stan kolekcjonerski) |
| Głodna + karmienie (dobre) | Mysz → Czonstkująca, głód = 0 |
| Głodna + czekaj do 100 | Mysz → Złowroga, wszystkie staty = 0 |
| Głodna + zła muzyka | Mysz → Złowroga, wszystkie staty = 0 |
| Głodna + aktywuje się też brudzenie (60) | Mysz → Złowroga, wszystkie staty = 0 |
| Smrodliwa + mycie | Mysz → Pachnąca, brud = 0 |
| Chcąca + tulanie | Mysz → Pumpużka, uwaga = 0 |
| Rower (brud < 60) | Mysz → Smrodliwa, brud = 60 |
| Rower (brud >= 60) | Brak efektu |
| Rozochocona + cokolwiek oprócz bumpu | Mysz → Obrażona |
| Rozochocona + bump | Mysz → Roztopiona |
| Obrażona — pierwsze 60 sek | Wszystkie akcje blokowane |
| Obrażona — po 60 sek + tulanie | Mysz → Neutralna |
| Obrażona — po 120 sek bez akcji | Mysz → Złowroga |
| Zła muzyka | Mysz → Obrażona |
| Złe jedzenie | Mysz → Obrażona |
| Sen (poduszka) | Mysz → Spankowa przed cutscenką |
| Czosnek przeciągnięty | Mysz → Czosnkowa |
