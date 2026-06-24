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

        spriteRenderer.color = StateColor(state);

        if (dirtyOverlay != null)
            dirtyOverlay.gameObject.SetActive(MouseStateManager.Instance.IsDirty());
    }

    static Color StateColor(MouseState s) => s switch
    {
        MouseState.Hungry      => new Color(0.50f, 0.70f, 1.00f),  // niebieski
        MouseState.Rozochocona => new Color(1.00f, 0.60f, 0.85f),  // różowy
        MouseState.Chcaca      => new Color(0.75f, 0.40f, 1.00f),  // fioletowy
        MouseState.Zrozpaczona => new Color(0.20f, 0.20f, 0.20f),  // czarny
        MouseState.Smutna      => new Color(1.00f, 0.60f, 0.60f),  // jasny czerwony
        MouseState.Zlowroga    => new Color(1.00f, 0.25f, 0.25f),  // średni czerwony
        MouseState.Sciekla     => new Color(0.90f, 0.05f, 0.05f),  // mocny czerwony
        MouseState.ScieklaII   => new Color(0.60f, 0.00f, 0.00f),  // ciemny czerwony
        _                      => Color.white
    };

    // Called by CatController on left-click hit
    public void HandleClick() => actionMenu?.Toggle();
}
