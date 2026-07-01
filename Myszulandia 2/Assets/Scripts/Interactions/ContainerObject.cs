using UnityEngine;

public class ContainerObject : ClickableObject
{
    [SerializeField] SpriteRenderer sr;
    [SerializeField] Sprite         closedSprite;
    [SerializeField] Sprite         openSprite;
    [SerializeField] GameObject[]   hiddenObjects;

    bool _isOpen;

    void Awake()     => GameEvents.OnNewDayStarted += Close;
    void OnDestroy() => GameEvents.OnNewDayStarted -= Close;
    void Start()     => Close();

    protected override void OnInteract()
    {
        if (_isOpen) Close();
        else         Open();
    }

    void Open()
    {
        _isOpen   = true;
        sr.sprite = openSprite;
        foreach (var obj in hiddenObjects)
            obj.SetActive(true);
    }

    void Close()
    {
        _isOpen   = false;
        sr.sprite = closedSprite;
        foreach (var obj in hiddenObjects)
            obj.SetActive(false);
    }
}
