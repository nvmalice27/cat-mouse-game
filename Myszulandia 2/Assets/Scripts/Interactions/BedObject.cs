using UnityEngine;

public class BedObject : ClickableObject
{
    [SerializeField] SpriteRenderer sr;
    [SerializeField] Sprite         madeSprite;
    [SerializeField] Sprite         unmadeSprite;

    bool _made;

    protected override void OnInteract()
    {
        _made     = !_made;
        sr.sprite = _made ? madeSprite : unmadeSprite;
        if (_made) MouseStateManager.Instance.TriggerPirat();
        else       MouseStateManager.Instance.ClearPirat();
    }

    void OnEnable()  => GameEvents.OnMouseStateChanged += OnStateChanged;
    void OnDisable() => GameEvents.OnMouseStateChanged -= OnStateChanged;

    void OnStateChanged(MouseState newState)
    {
        if (newState != MouseState.Pirat && _made)
        {
            _made     = false;
            sr.sprite = unmadeSprite;
        }
    }
}
