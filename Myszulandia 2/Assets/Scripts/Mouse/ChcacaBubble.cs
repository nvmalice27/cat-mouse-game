using UnityEngine;

public class ChcacaBubble : MonoBehaviour
{
    [SerializeField] SpriteRenderer iconRenderer;
    [SerializeField] Sprite         drinkIcon;
    [SerializeField] Sprite         mouseBallIcon;
    [SerializeField] Sprite         strongHugIcon;

    void OnEnable()
    {
        GameEvents.OnMouseStateChanged += OnStateChanged;
        if (MouseStateManager.Instance != null)
            OnStateChanged(MouseStateManager.Instance.CurrentState);
    }

    void OnDisable() => GameEvents.OnMouseStateChanged -= OnStateChanged;

    void OnStateChanged(MouseState state)
    {
        if (state != MouseState.Chcaca)
        {
            gameObject.SetActive(false);
            return;
        }

        Sprite icon = MouseStateManager.Instance.CurrentChcacaRequest switch
        {
            ChcacaRequest.Drink     => drinkIcon,
            ChcacaRequest.MouseBall => mouseBallIcon,
            ChcacaRequest.StrongHug => strongHugIcon,
            _                       => null
        };

        gameObject.SetActive(icon != null);
        if (iconRenderer != null) iconRenderer.sprite = icon;
    }
}
