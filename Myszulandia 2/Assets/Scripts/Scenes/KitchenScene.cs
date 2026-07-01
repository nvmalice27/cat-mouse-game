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

    void OnEnable()  { GameEvents.OnNewDayStarted += OnNewDay; GameEvents.OnMouseStateChanged += OnStateChanged; }
    void OnDisable() { GameEvents.OnNewDayStarted -= OnNewDay; GameEvents.OnMouseStateChanged -= OnStateChanged; }

    void Start()
    {
        if (cookButton == null) { Debug.LogError("KitchenScene: cookButton nie jest przypisany!"); return; }
        cookButton.SetActive(false);

        var mgr = InventoryManager.Instance;
        if (mgr == null) return;

        if (mgr.CookedToday)
        {
            DisableAllSources();
            return;
        }

        // Odtwórz listę zebranych składników z InventoryManager
        for (int i = 0; i < ingredientSources.Length; i++)
        {
            if (mgr.IsIngredientHarvested(i))
                _collected.Add(i);
        }
        if (_collected.Count >= ingredientsNeeded)
            cookButton.SetActive(true);
        // IngredientSource.Start() samodzielnie chowa już zebrane składniki
    }

    void OnNewDay()
    {
        _cooking = false;
        _collected.Clear();
        if (cookButton != null) cookButton.SetActive(false);
        // IngredientSource.Respawn() samodzielnie odkrywa składniki w nowym dniu
    }

    void OnStateChanged(MouseState newState)
    {
        if (newState == MouseState.Smutna)
            GameManager.Instance.NavigateTo("Room");
    }

    void DisableAllSources()
    {
        foreach (var src in ingredientSources) src.SetActive(false);
    }

    public void CollectIngredient(int sourceIndex)
    {
        if (_cooking || _collected.Contains(sourceIndex)) return;
        _collected.Add(sourceIndex);
        InventoryManager.Instance.MarkIngredientHarvested(sourceIndex);
        if (_collected.Count >= ingredientsNeeded && cookButton != null)
            cookButton.SetActive(true);
    }

    public void Cook()
    {
        if (_cooking || _collected.Count < ingredientsNeeded) return;
        _cooking = true;
        if (cookButton != null) cookButton.SetActive(false);
        if (potAnimator != null) potAnimator.SetTrigger("Cook");
        StartCoroutine(FinishCooking());
    }

    IEnumerator FinishCooking()
    {
        yield return new WaitForSeconds(cookDuration);
        InventoryManager.Instance.AddMeal(true);
        InventoryManager.Instance.MarkCooked();
        _collected.Clear();
        _cooking = false;
        DisableAllSources();
    }
}
