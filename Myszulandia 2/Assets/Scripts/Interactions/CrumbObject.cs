using UnityEngine;
using System.Collections;

public class CrumbObject : ClickableObject
{
    [SerializeField] GameObject visuals;

    protected override void OnInteract() => StartCoroutine(CollectAndRespawn());

    IEnumerator CollectAndRespawn()
    {
        visuals.SetActive(false);
        InventoryManager.Instance.CollectCrumb();
        yield return new WaitForSeconds(0.3f);
        visuals.SetActive(true);
    }
}
