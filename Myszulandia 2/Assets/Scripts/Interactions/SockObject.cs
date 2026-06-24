using UnityEngine;

public class SockObject : MonoBehaviour
{
    void OnEnable()  => GameEvents.OnNewDayStarted += Respawn;
    void OnDisable() => GameEvents.OnNewDayStarted -= Respawn;

    void OnMouseDown()
    {
        InventoryManager.Instance.CollectSock();
        gameObject.SetActive(false);
    }

    void Respawn() => gameObject.SetActive(true);
}
