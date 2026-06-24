using UnityEngine;

public class PhoneObject : ClickableObject
{
    [SerializeField] PhoneUI phoneUI;
    protected override void OnInteract() => phoneUI.Open();
}
