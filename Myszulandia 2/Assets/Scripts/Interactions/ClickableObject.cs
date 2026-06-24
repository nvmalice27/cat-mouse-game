using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class ClickableObject : MonoBehaviour
{
    void OnMouseDown()
    {
        var cat = FindFirstObjectByType<CatController>();
        if (cat != null) cat.WalkThenInteract(this);
    }

    // Called by CatController once it arrives
    public void Interact() => OnInteract();

    protected abstract void OnInteract();
}
