using UnityEngine;

public class RadioObject : ClickableObject
{
    [SerializeField] AudioSource audioSource;
    bool _playing;

    protected override void OnInteract()
    {
        _playing = !_playing;
        if (audioSource != null)
        {
            if (_playing) audioSource.Play();
            else          audioSource.Stop();
        }
        if (_playing) MouseStateManager.Instance.TriggerMusic();
    }
}
