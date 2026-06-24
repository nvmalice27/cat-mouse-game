using UnityEngine;

public class RadioObject : ClickableObject
{
    [SerializeField] RadioUI radioUI;
    protected override void OnInteract() => radioUI?.Open();
}
