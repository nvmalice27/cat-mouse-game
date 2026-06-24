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
        // Normalne
        MouseState.Normal      => Color.white,
        MouseState.Happy       => new Color(1.00f, 0.95f, 0.50f),  // żółty

        // Głód
        MouseState.Hungry      => new Color(0.50f, 0.70f, 1.00f),  // niebieski

        // Stany losowe/tymczasowe
        MouseState.Rozochocona => new Color(1.00f, 0.60f, 0.85f),  // różowy
        MouseState.Chcaca      => new Color(0.75f, 0.40f, 1.00f),  // fioletowy
        MouseState.Zrozpaczona => new Color(0.20f, 0.20f, 0.20f),  // czarny

        // Negatywne akcje (okruszki, zła muzyka, złe jedzenie)
        MouseState.Zla         => new Color(1.00f, 0.55f, 0.10f),  // pomarańczowy
        MouseState.Obrazona    => new Color(0.85f, 0.35f, 0.65f),  // różowo-fioletowy

        // Złe stany — coraz bardziej czerwone
        MouseState.Smutna      => new Color(1.00f, 0.60f, 0.60f),  // jasny czerwony
        MouseState.Zlowroga    => new Color(1.00f, 0.25f, 0.25f),  // średni czerwony
        MouseState.Sciekla     => new Color(0.90f, 0.05f, 0.05f),  // mocny czerwony
        MouseState.ScieklaII   => new Color(0.60f, 0.00f, 0.00f),  // ciemny czerwony

        // Kolekcjonerskie
        MouseState.Kochana        => new Color(1.00f, 0.70f, 0.80f),  // jasny róż
        MouseState.Szczesliwa     => new Color(1.00f, 0.95f, 0.40f),  // złoty
        MouseState.Grobol         => new Color(0.55f, 0.90f, 0.45f),  // zielony
        MouseState.Wakacyjna      => new Color(0.30f, 0.85f, 0.90f),  // turkus
        MouseState.Tanczaca       => new Color(0.80f, 0.50f, 1.00f),  // jasny fiolet
        MouseState.Makapaka       => new Color(1.00f, 0.85f, 0.20f),  // złoto
        MouseState.Brudna         => new Color(0.60f, 0.45f, 0.30f),  // brąz
        MouseState.Pirat          => new Color(0.40f, 0.40f, 0.50f),  // stalowy
        MouseState.Niewyspana     => new Color(0.55f, 0.45f, 0.75f),  // ciemny fiolet
        MouseState.WesolaPoPobudce=> new Color(1.00f, 0.75f, 0.40f),  // pomarańcz
        MouseState.Myszkujaca     => new Color(0.35f, 0.80f, 0.70f),  // morski
        MouseState.Pachnaca       => new Color(0.70f, 0.90f, 1.00f),  // błękitny
        MouseState.Czonstkowa     => new Color(1.00f, 0.90f, 0.10f),  // jaskrawy żółty

        _                      => Color.white
    };

    // Called by CatController on left-click hit
    public void HandleClick() => actionMenu?.Toggle();
}
