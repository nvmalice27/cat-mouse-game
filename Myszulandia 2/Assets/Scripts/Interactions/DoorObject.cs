using UnityEngine;

public class DoorObject : ClickableObject
{
    [SerializeField] GameObject roomDoorMenu;
    [SerializeField] bool       isInRoom = true;

    protected override void OnInteract()
    {
        if (isInRoom) roomDoorMenu.SetActive(true);
        else
        {
            MouseStateManager.Instance.TriggerKitchenExit();
            GameManager.Instance.NavigateTo("Room");
        }
    }

    public void GoToKitchen()
    {
        roomDoorMenu.SetActive(false);
        MouseStateManager.Instance.TriggerKitchenEntry();
        GameManager.Instance.NavigateTo("Kitchen");
    }

    public void GoToBathroom()
    {
        roomDoorMenu.SetActive(false);
        GameManager.Instance.NavigateTo("Bathroom");
    }

    public void CloseDoorMenu() => roomDoorMenu?.SetActive(false);
}
