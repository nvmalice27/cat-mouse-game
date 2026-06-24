using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CutsceneManager : MonoBehaviour
{
    [System.Serializable]
    public class Entry { public string key; public AnimationClip clip; }

    [SerializeField] List<Entry> cutscenes;
    [SerializeField] Animator    animator;
    [SerializeField] GameObject  panel;

    string _currentKey;

    void OnEnable()  => GameEvents.OnCutsceneRequested += Play;
    void OnDisable() => GameEvents.OnCutsceneRequested -= Play;

    void Play(string key)
    {
        _currentKey = key;
        var entry = cutscenes.Find(e => e.key == key);
        if (entry == null || entry.clip == null) { OnComplete(); return; }

        panel.SetActive(true);
        animator.Play(entry.clip.name);
        StartCoroutine(WaitAndComplete(entry.clip.length));
    }

    IEnumerator WaitAndComplete(float duration)
    {
        yield return new WaitForSeconds(duration);
        panel.SetActive(false);
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
}
