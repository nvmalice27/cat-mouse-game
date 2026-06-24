using UnityEngine;

public class SockObject : ClickableObject
{
    [SerializeField] int sockIndex;

    void Awake()  => GameEvents.OnNewDayStarted += Respawn;
    void OnDestroy() => GameEvents.OnNewDayStarted -= Respawn;

    void Start()
    {
        if (InventoryManager.Instance != null && InventoryManager.Instance.IsSockHarvested(sockIndex))
            gameObject.SetActive(false);
    }

    protected override void OnInteract()
    {
        InventoryManager.Instance.CollectSock(sockIndex);
        gameObject.SetActive(false);
    }

    void Respawn() => gameObject.SetActive(true);
}
