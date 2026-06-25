using UnityEngine;

public class CandleObject : ClickableObject
{
    [SerializeField] SpriteRenderer sr;
    [SerializeField] Sprite         litSprite;
    [SerializeField] Sprite         unlitSprite;

    bool _lit;

    protected override void OnInteract()
    {
        _lit      = !_lit;
        sr.sprite = _lit ? litSprite : unlitSprite;
        if (_lit) MouseStateManager.Instance.TriggerCandles();
    }

    void OnEnable()  => GameEvents.OnMouseStateChanged += OnStateChanged;
    void OnDisable() => GameEvents.OnMouseStateChanged -= OnStateChanged;

    void OnStateChanged(MouseState newState)
    {
        if (newState != MouseState.Rozochocona && _lit)
        {
            _lit      = false;
            sr.sprite = unlitSprite;
        }
    }
}
