# Design: System ukrytych obiektów w pojemnikach

**Data:** 2026-07-02  
**Projekt:** Myszulandia 2

---

## Cel

Niektóre obiekty na scenie mają być ukryte w pojemnikach (szuflada, lodówka, kołdra). Gracz musi najpierw otworzyć pojemnik, żeby zobaczyć i zebrać ukryty obiekt.

---

## Zachowanie

- Kliknięcie zamkniętego pojemnika → podmienia sprite na "otwarty", aktywuje ukryte obiekty wewnątrz
- Kliknięcie otwartego pojemnika → podmienia sprite na "zamknięty", chowa ukryte obiekty
- Ukryty obiekt wymaga osobnego kliknięcia żeby go zebrać (SockObject, IngredientSource itp.)
- Nowy dzień (`OnNewDayStarted`) → pojemnik się zamyka, ukryte obiekty resetują się zgodnie z własną logiką

---

## Nowy skrypt: `ContainerObject`

**Lokalizacja:** `Assets/Scripts/Interactions/ContainerObject.cs`

Dziedziczy po `ClickableObject` (istniejąca baza). Obsługuje toggle otwarcia/zamknięcia przez zmianę sprite'a i aktywację/dezaktywację dzieci.

### Pola (Inspector)

| Pole | Typ | Opis |
|---|---|---|
| `sr` | `SpriteRenderer` | Renderer sprite'a pojemnika |
| `closedSprite` | `Sprite` | Grafika stanu zamkniętego |
| `openSprite` | `Sprite` | Grafika stanu otwartego |
| `hiddenObjects` | `GameObject[]` | Obiekty ukryte wewnątrz pojemnika |

### Logika

```
OnInteract():
  toggle _isOpen
  sr.sprite = _isOpen ? openSprite : closedSprite
  foreach obj in hiddenObjects: obj.SetActive(_isOpen)

OnNewDayStarted():
  _isOpen = false
  sr.sprite = closedSprite
  foreach obj in hiddenObjects: obj.SetActive(false)
```

---

## Hierarchia w scenie (przykład)

```
[Szuflada]          ← ContainerObject
  ├── sprite:         drawer_closed / drawer_open
  └── [Skarpetka]   ← SockObject  (domyślnie inactive)
```

`hiddenObjects` zawiera referencję do Skarpetki (lub wielu obiektów).

---

## Zmiany w istniejącym kodzie

Żadnych. `SockObject`, `IngredientSource` i pozostałe `ClickableObject` działają bez modyfikacji. `ContainerObject` to wyłącznie nowy plik.

---

## Przykłady użycia

| Pojemnik | Ukryty obiekt | Skrypt ukrytego obiektu |
|---|---|---|
| Szuflada | Skarpetka | `SockObject` |
| Lodówka | Składniki | `IngredientSource` |
| Kołdra na łóżku | Cząstki / inne | `ClickableObject` (własny skrypt) |

---

## Co NIE wchodzi w zakres

- Animacje (kołdra, wysuwanie szuflady) — decyzja: tylko sprite swap
- Dźwięki przy otwieraniu
- Zapis stanu otwartego pojemnika między dniami (reset do zamkniętego przy nowym dniu)
