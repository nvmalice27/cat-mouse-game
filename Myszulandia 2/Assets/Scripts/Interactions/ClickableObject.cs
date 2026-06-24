using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class ClickableObject : MonoBehaviour
{
    public void Interact() => OnInteract();
    protected abstract void OnInteract();
}
