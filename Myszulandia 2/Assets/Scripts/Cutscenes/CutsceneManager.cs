using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CutsceneManager : MonoBehaviour
{
    [System.Serializable]
    public class Entry { public string key; public AnimationClip clip; }

    [SerializeField] List<Entry> cutscenes;
    [SerializeField] Animator    animator;
    [SerializeField] GameObject  panel;

    CanvasGroup _cg;
    string _currentKey;

    void Awake()
    {
        _cg = panel != null ? panel.GetComponent<CanvasGroup>() : null;
        GameEvents.OnCutsceneRequested += Play;
        Hide();
    }

    void OnDestroy() => GameEvents.OnCutsceneRequested -= Play;

    void Play(string key)
    {
        _currentKey = key;
        var entry = cutscenes?.Find(e => e.key == key);
        if (entry == null || entry.clip == null) { OnComplete(); return; }

        Show();
        animator.Play(entry.clip.name);
        StartCoroutine(WaitAndComplete(entry.clip.length));
    }

    IEnumerator WaitAndComplete(float duration)
    {
        yield return new WaitForSeconds(duration);
        Hide();
        OnComplete();
    }

    void OnComplete()
    {
        if (_currentKey == "Night" || _currentKey == "NightMakapaka")
            DayNightManager.Instance.OnNightCutsceneComplete();
        else if (_currentKey == "GameOver")
            DayNightManager.Instance.ResetDay();
        _currentKey = null;
    }

    void Show()
    {
        if (_cg != null) { _cg.alpha = 1f; _cg.blocksRaycasts = true; }
        else if (panel != null) panel.SetActive(true);
    }

    void Hide()
    {
        if (_cg != null) { _cg.alpha = 0f; _cg.blocksRaycasts = false; }
        else if (panel != null) panel.SetActive(false);
    }
}
