using UnityEngine;

public class RadioUI : MonoBehaviour
{
    [SerializeField] GameObject panel;

    public void Open()  => panel.SetActive(true);
    public void Close() { if (panel != null) panel.SetActive(false); }

    // Pop / Spokojna / Klasyczna → mysz tańczy (Tanczaca)
    public void PlayPop()       { MouseStateManager.Instance.TriggerMusic(); Close(); }
    public void PlaySpokojna()  { MouseStateManager.Instance.TriggerMusic(); Close(); }
    public void PlayKlasyczna() { MouseStateManager.Instance.TriggerMusic(); Close(); }

    // Heavy Metal → pogarsza nastrój (jak okruszki ale bez jedzenia)
    public void PlayMetal()     { MouseStateManager.Instance.ApplyNegativeAction(false); Close(); }
}
