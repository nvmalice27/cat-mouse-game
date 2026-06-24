using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class ClickableObject : MonoBehaviour
{
    [SerializeField] float interactRadius = 2f;

    void OnMouseDown()
    {
        var cat = FindFirstObjectByType<CatController>();
        if (cat == null || Vector2.Distance(cat.transform.position, transform.position) > interactRadius)
            return;
        OnInteract();
    }

    protected abstract void OnInteract();
}
