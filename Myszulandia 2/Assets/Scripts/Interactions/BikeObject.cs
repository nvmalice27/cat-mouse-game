using UnityEngine;

public class BikeObject : ClickableObject
{
    protected override void OnInteract() => MouseStateManager.Instance.TriggerBike();
}
