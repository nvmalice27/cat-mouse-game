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
}
