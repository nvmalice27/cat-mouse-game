using UnityEngine;

public class MouseController : MonoBehaviour
{
    [SerializeField] SpriteRenderer  spriteRenderer;
    [SerializeField] Animator        animator;
    [SerializeField] MouseTypeSO[]   mouseTypes;
    [SerializeField] Sprite          normalSprite;
    [SerializeField] Sprite          hungrySprite;
    [SerializeField] SpriteRenderer  dirtyOverlay;
    [SerializeField] MouseActionMenu actionMenu;

    void OnEnable()  => GameEvents.OnMouseStateChanged += UpdateVisuals;
    void OnDisable() => GameEvents.OnMouseStateChanged -= UpdateVisuals;

    void Start() => UpdateVisuals(MouseStateManager.Instance.CurrentState);

    void UpdateVisuals(MouseState state)
    {
        int idx = MouseStateManager.CollectibleIndex(state);
        if (idx >= 0 && idx < mouseTypes.Length && mouseTypes[idx] != null)
        {
            var so = mouseTypes[idx];
            spriteRenderer.sprite = so.sprite;
            if (so.animationClip != null && animator != null)
                animator.Play(so.animationClip.name);
        }
        else
        {
            spriteRenderer.sprite = MouseStateManager.Instance.IsHungry()
                ? hungrySprite : normalSprite;
        }

        if (dirtyOverlay != null)
            dirtyOverlay.gameObject.SetActive(MouseStateManager.Instance.IsDirty());
    }

    // Called by CatController on left-click hit
    public void HandleClick() => actionMenu?.Toggle();
}
