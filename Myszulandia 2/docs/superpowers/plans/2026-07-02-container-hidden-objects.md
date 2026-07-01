# System ukrytych obiektów w pojemnikach — Plan implementacji

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Dodać skrypt `ContainerObject` umożliwiający ukrycie obiektów w pojemniku (szuflada, lodówka, kołdra) i odkrycie ich przez kliknięcie.

**Architecture:** Nowy skrypt `ContainerObject : ClickableObject` — toggle otwiera/zamyka pojemnik przez zamianę sprite'a i aktywację/dezaktywację dzieci. Nowy dzień zamyka pojemnik. Żadnych zmian w istniejącym kodzie.

**Tech Stack:** Unity 2D, C#, istniejący system `ClickableObject` / `GameEvents`

## Global Constraints

- Dziedziczy po `ClickableObject` (plik: `Assets/Scripts/Interactions/ClickableObject.cs`)
- Używa `GameEvents.OnNewDayStarted` (plik: `Assets/Scripts/Core/GameEvents.cs`)
- Nazwy pól zgodne z konwencją projektu: camelCase z prefixem `_` dla prywatnych, `SerializeField` dla pól edytora
- Nie modyfikować żadnego istniejącego pliku

---

### Task 1: Skrypt `ContainerObject`

**Files:**
- Create: `Assets/Scripts/Interactions/ContainerObject.cs`

**Interfaces:**
- Consumes: `ClickableObject.OnInteract()` (override), `GameEvents.OnNewDayStarted` (event Action)
- Produces: publiczny komponent `ContainerObject` do dodania na obiekty pojemników w scenie

- [ ] **Krok 1: Utwórz plik skryptu**

Utwórz `Assets/Scripts/Interactions/ContainerObject.cs` z pełną zawartością:

```csharp
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
```

- [ ] **Krok 2: Sprawdź kompilację w Unity**

Otwórz Unity Editor. W konsoli nie powinny pojawić się żadne błędy kompilacji. Komponent `ContainerObject` powinien być dostępny do dodania na GameObject.

- [ ] **Krok 3: Ustaw testowy pojemnik w scenie**

W dowolnej scenie (np. Room):
1. Wybierz GameObject szuflady (lub utwórz tymczasowy)
2. Dodaj komponent `ContainerObject`
3. Przypisz `SpriteRenderer` (sr)
4. Przypisz dwa sprite'y: `closedSprite`, `openSprite`
5. Utwórz dziecko-obiekt (np. `SockObject` lub pusty GameObject z `ClickableObject`)
6. Domyślnie odznacz je w Inspectorze (inactive)
7. Przeciągnij je do pola `Hidden Objects` w `ContainerObject`
8. Upewnij się że pojemnik ma `Collider2D` (wymagane przez `ClickableObject`)

- [ ] **Krok 4: Przetestuj w Play Mode**

Uruchom Play Mode i sprawdź:

| Akcja | Oczekiwany wynik |
|---|---|
| Kliknij pojemnik (zamknięty) | Sprite zmienia się na `openSprite`, ukryty obiekt staje się aktywny i klikalny |
| Kliknij pojemnik ponownie (otwarty) | Sprite wraca do `closedSprite`, ukryty obiekt znika |
| Otwórz, nie zamykaj, wywołaj nowy dzień (lub ręcznie `GameEvents.RaiseNewDayStarted()` z konsoli) | Pojemnik zamyka się automatycznie, ukryty obiekt nieaktywny |
| Otwórz, zbierz ukryty obiekt (SockObject), zamknij i otwórz ponownie | Po nowym dniu skarpetka się resetuje zgodnie z logiką `SockObject` |

- [ ] **Krok 5: Zastosuj w scenach docelowych**

Dla każdego pojemnika w grze (szuflada / lodówka / kołdra):
1. Przygotuj dwa sprite'y (zamknięty/otwarty) i przypisz w Inspectorze
2. Umieść ukryte obiekty jako dzieci w hierarchii (domyślnie inactive)
3. Dodaj `ContainerObject` i połącz pola

Uwaga dot. łóżka: jeśli ta sama scena ma już `BedObject` (pirat-mechanic), `ContainerObject` z kołdrą powinien być **osobnym** obiektem nałożonym na łóżko, nie zastępować `BedObject`.

- [ ] **Krok 6: Commit**

```bash
git add "Assets/Scripts/Interactions/ContainerObject.cs" "Assets/Scripts/Interactions/ContainerObject.cs.meta"
git commit -m "feat: add ContainerObject for hidden items in drawers, fridge, blanket"
```
