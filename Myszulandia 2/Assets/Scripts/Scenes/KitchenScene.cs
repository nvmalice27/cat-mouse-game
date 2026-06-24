using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KitchenScene : MonoBehaviour
{
    [SerializeField] int          ingredientsNeeded = 3;
    [SerializeField] GameObject[] ingredientSources;
    [SerializeField] GameObject   cookButton;
    [SerializeField] Animator     potAnimator;
    [SerializeField] float        cookDuration = 2f;

    readonly List<int> _collected = new();
    bool _cooking;

    void Start() => cookButton.SetActive(false);

    public void CollectIngredient(int sourceIndex)
    {
        if (_cooking || _collected.Contains(sourceIndex)) return;
        _collected.Add(sourceIndex);
        ingredientSources[sourceIndex].SetActive(false);
        if (_collected.Count >= ingredientsNeeded)
            cookButton.SetActive(true);
    }

    public void Cook()
    {
        if (_cooking || _collected.Count < ingredientsNeeded) return;
        _cooking = true;
        cookButton.SetActive(false);
        if (potAnimator != null) potAnimator.SetTrigger("Cook");
        StartCoroutine(FinishCooking());
    }

    IEnumerator FinishCooking()
    {
        yield return new WaitForSeconds(cookDuration);
        InventoryManager.Instance.AddMeal(true);
        _collected.Clear();
        foreach (var src in ingredientSources) src.SetActive(true);
        _cooking = false;
    }
}
