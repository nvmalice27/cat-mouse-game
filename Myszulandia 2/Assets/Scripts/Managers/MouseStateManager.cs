using UnityEngine;
using System.Collections;

public class MouseStateManager : MonoBehaviour
{
    public static MouseStateManager Instance { get; private set; }

    const float HungerRate          = 1f / 3f;
    const float DirtRate            = 1f / 4.2f;
    const float HungerNormalMax     = 40f;
    const float HungerHungryMax     = 80f;
    const float DirtDirtyThreshold  = 60f;
    const float RandomStateInterval = 180f;
    const float TempStateTimeout    = 120f;
    const float CollectibleDuration = 120f;
    const float BadStateAutoAdvance = 300f;
    const int   SmutnaRecovery      = 3;
    const int   ZlowrogaRecovery    = 5;
    const int   ScieklaRecovery     = 10;

    float _hunger;
    float _dirt;
    MouseState _state = MouseState.Normal;
    bool[] _unlocked = new bool[15];
    int   _recoveryCounter;
    float _randomStateTimer;
    float _badStateTimer;
    float _collectibleTimer;
    float _tempStateTimer;
    bool  _timersRunning = true;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (!_timersRunning) return;
        float dt = Time.deltaTime;
        _hunger = Mathf.Min(100f, _hunger + HungerRate * dt);
        _dirt   = Mathf.Min(100f, _dirt   + DirtRate   * dt);

        if (IsNormalOrHungry())
        {
            SyncHungerState();
            _randomStateTimer += dt;
            if (_randomStateTimer >= RandomStateInterval)
            {
                _randomStateTimer = 0f;
                DrawRandomTempState();
            }
        }
        else if (IsTemporary())
        {
            _tempStateTimer += dt;
            if (_tempStateTimer >= TempStateTimeout)
            {
                _tempStateTimer = 0f;
                OnTempStateExpired();
            }
        }
        else if (IsBadState())
        {
            _badStateTimer += dt;
            if (_badStateTimer >= BadStateAutoAdvance)
            {
                _badStateTimer = 0f;
                AdvanceBadState();
            }
        }
        else if (IsCollectible())
        {
            _collectibleTimer += dt;
            if (_collectibleTimer >= CollectibleDuration)
            {
                _collectibleTimer = 0f;
                ReturnToBase();
            }
        }
    }

    public bool IsNormalOrHungry() =>
        _state == MouseState.Normal || _state == MouseState.Hungry || _state == MouseState.Happy;
    public bool IsTemporary() =>
        _state == MouseState.Rozochocona || _state == MouseState.Chcaca || _state == MouseState.Zrozpaczona;
    public bool IsBadState() =>
        _state is MouseState.Smutna or MouseState.Zlowroga or MouseState.Sciekla or MouseState.ScieklaII;
    public bool IsCollectible() =>
        (int)_state >= 10 && (int)_state <= 24;
    public bool IsHungry() => _hunger >= HungerNormalMax;
    public bool IsDirty()  => _dirt   >= DirtDirtyThreshold;

    void SyncHungerState()
    {
        if (_state == MouseState.Happy) return;
        if (_hunger >= HungerHungryMax)
        {
            EnterBadState(MouseState.Smutna);
        }
        else
        {
            MouseState target = _hunger < HungerNormalMax ? MouseState.Normal : MouseState.Hungry;
            if (_state != target) SetState(target);
        }
    }

    void DrawRandomTempState()
    {
        MouseState s = Random.Range(0, 3) switch
        {
            0 => MouseState.Rozochocona,
            1 => MouseState.Chcaca,
            _ => MouseState.Zrozpaczona
        };
        SetState(s);
        _tempStateTimer = 0f;
    }

    void OnTempStateExpired()
    {
        if (_state == MouseState.Zrozpaczona)
            EnterBadState(MouseState.Zlowroga);
        else
            EnterBadState(MouseState.Smutna);
    }

    void EnterBadState(MouseState bad)
    {
        SetState(bad);
        _badStateTimer   = 0f;
        _recoveryCounter = 0;
        if (bad == MouseState.ScieklaII) TriggerGameOver();
    }

    void AdvanceBadState()
    {
        MouseState next = _state switch
        {
            MouseState.Smutna   => MouseState.Zlowroga,
            MouseState.Zlowroga => MouseState.Sciekla,
            MouseState.Sciekla  => MouseState.ScieklaII,
            _                   => MouseState.ScieklaII
        };
        EnterBadState(next);
    }

    public void ApplyNegativeAction(bool isCrumbs)
    {
        if (IsTemporary())  { EnterBadState(MouseState.Zlowroga); return; }
        if (IsBadState())   { AdvanceBadState(); return; }
        EnterCollectibleState(isCrumbs ? MouseState.Zla : MouseState.Obrazona);
    }

    public void ApplyDirectAction(int action)
    {
        if (IsBadState())
        {
            _recoveryCounter++;
            int needed = _state switch
            {
                MouseState.Smutna   => SmutnaRecovery,
                MouseState.Zlowroga => ZlowrogaRecovery,
                MouseState.Sciekla  => ScieklaRecovery,
                _                   => 999
            };
            if (_recoveryCounter >= needed)
            {
                _recoveryCounter = 0;
                MouseState prev = _state switch
                {
                    MouseState.Smutna   => MouseState.Normal,
                    MouseState.Zlowroga => MouseState.Smutna,
                    MouseState.Sciekla  => MouseState.Zlowroga,
                    _                   => MouseState.Zlowroga
                };
                SetState(prev);
                _badStateTimer = 0f;
            }
            return;
        }

        if (!IsTemporary()) return;

        bool correct = (_state == MouseState.Zrozpaczona && action == 0) ||
                       (_state == MouseState.Chcaca       && action == 1) ||
                       (_state == MouseState.Rozochocona  && action == 3);
        if (!correct) return;

        SetState(MouseState.Happy);
        _tempStateTimer = 0f;
        StartCoroutine(ReturnFromHappyAfter(3f));
    }

    IEnumerator ReturnFromHappyAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToBase();
    }

    public void Feed(bool isGood)
    {
        if (!isGood) { ApplyNegativeAction(false); return; }
        bool wasHungry = _hunger >= HungerNormalMax;
        _hunger = 0f;
        EnterCollectibleState(wasHungry ? MouseState.Szczesliwa : MouseState.Grobol);
    }

    public void TriggerRose()     => EnterCollectibleState(MouseState.Kochana);
    public void TriggerVacation() => EnterCollectibleState(MouseState.Wakacyjna);

    public void TriggerBike()
    {
        _dirt = 100f;
        EnterCollectibleState(MouseState.Brudna);
    }

    public void TriggerMakapaka() => EnterCollectibleState(MouseState.Makapaka);

    public void TriggerPirat()
    {
        if (_state != MouseState.Pirat)
            EnterCollectibleState(MouseState.Pirat);
    }

    public void ClearPirat()
    {
        if (_state == MouseState.Pirat) ReturnToBase();
    }

    public void TriggerAlarmWakeup(AlarmType alarm)
    {
        EnterCollectibleState(alarm == AlarmType.Early
            ? MouseState.Niewyspana
            : MouseState.WesolaPoPobudce);
    }

    public void TriggerKitchenEntry()
    {
        if (!IsBadState() && !IsCollectible())
            SetState(MouseState.Myszkujaca);
    }

    public void TriggerKitchenExit() => ReturnToBase();

    public void TriggerMusic()
    {
        if (!IsBadState())
            EnterCollectibleState(MouseState.Tanczaca);
    }

    public void TriggerWash()
    {
        bool wasDirty = _dirt >= DirtDirtyThreshold;
        _dirt = 0f;
        if (wasDirty) EnterCollectibleState(MouseState.Pachnaca);
    }

    public void TriggerCzonstkowa() => EnterCollectibleState(MouseState.Czonstkowa);

    void EnterCollectibleState(MouseState s)
    {
        SetState(s);
        _collectibleTimer = 0f;
        TryUnlock(s);
    }

    void TryUnlock(MouseState s)
    {
        int idx = CollectibleIndex(s);
        if (idx < 0 || idx >= 15) return;
        bool isFirst = !_unlocked[idx];
        _unlocked[idx] = true;
        GameEvents.RaiseMouseTypeUnlocked(idx, isFirst);
    }

    void ReturnToBase()
    {
        SetState(_hunger < HungerNormalMax ? MouseState.Normal : MouseState.Hungry);
    }

    public static int CollectibleIndex(MouseState s)
    {
        int v = (int)s;
        return (v >= 10 && v <= 24) ? v - 10 : -1;
    }

    void SetState(MouseState s)
    {
        if (_state == s) return;
        _state = s;
        GameEvents.RaiseMouseStateChanged(s);
    }

    void TriggerGameOver()
    {
        _timersRunning = false;
        GameEvents.RaiseGameOver();
        GameEvents.RaiseCutsceneRequested("GameOver");
    }

    public void PauseTimers()  => _timersRunning = false;
    public void ResumeTimers() => _timersRunning = true;

    public void ResetForNewDay()
    {
        _hunger = _dirt = 0f;
        _recoveryCounter = _randomStateTimer = _badStateTimer =
            _collectibleTimer = _tempStateTimer = 0f;
        SetState(MouseState.Normal);
        _timersRunning = true;
    }

    public void ApplySaveData(SaveData d)
    {
        _hunger   = d.hunger;
        _dirt     = d.dirt;
        _state    = (MouseState)d.mouseStateValue;
        _unlocked = (bool[])d.unlockedMouseTypes.Clone();
        GameEvents.RaiseMouseStateChanged(_state);
    }

    public void WriteSaveData(SaveData d)
    {
        d.hunger             = _hunger;
        d.dirt               = _dirt;
        d.mouseStateValue    = (int)_state;
        d.unlockedMouseTypes = (bool[])_unlocked.Clone();
    }

    public MouseState CurrentState => _state;
    public bool[]     UnlockedTypes => _unlocked;
    public float      Hunger        => _hunger;
    public float      Dirt          => _dirt;
}
