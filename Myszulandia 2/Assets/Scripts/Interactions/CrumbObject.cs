using UnityEngine;
using System.Collections;

public class CrumbObject : ClickableObject
{
    SpriteRenderer _sr;
    Collider2D     _col;

    void Awake()
    {
        _sr  = GetComponent<SpriteRenderer>();
        _col = GetComponent<Collider2D>();
    }

    protected override void OnInteract() => StartCoroutine(CollectAndRespawn());

    IEnumerator CollectAndRespawn()
    {
        _sr.enabled  = false;
        _col.enabled = false;
        InventoryManager.Instance.CollectCrumb();
        yield return new WaitForSeconds(0.3f);
        _sr.enabled  = true;
        _col.enabled = true;
    }
}
