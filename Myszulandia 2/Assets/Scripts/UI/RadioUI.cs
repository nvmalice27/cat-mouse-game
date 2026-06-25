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
