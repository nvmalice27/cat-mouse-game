using UnityEngine;
using System.Collections.Generic;

public class BathroomScene : MonoBehaviour
{
    [SerializeField] List<GameObject> dirtySpots;
    [SerializeField] float            brushRadius = 0.4f;
    [SerializeField] GameObject       spongeCursor;

    bool _washing;

    void Start()
    {
        bool isDirty = MouseStateManager.Instance.IsDirty();
        foreach (var s in dirtySpots) s.SetActive(isDirty);
        _washing = isDirty;
    }

    void Update()
    {
        if (!_washing) return;

        Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        world.z = 0;

        if (spongeCursor != null)
            spongeCursor.transform.position = world;

        if (!Input.GetMouseButton(0)) return;

        bool anyActive = false;
        foreach (var spot in dirtySpots)
        {
            if (!spot.activeSelf) continue;
            anyActive = true;
            if (Vector2.Distance(world, spot.transform.position) < brushRadius)
                spot.SetActive(false);
        }

        if (!anyActive) FinishWashing();
    }

    void FinishWashing()
    {
        _washing = false;
        MouseStateManager.Instance.TriggerWash();
    }
}
