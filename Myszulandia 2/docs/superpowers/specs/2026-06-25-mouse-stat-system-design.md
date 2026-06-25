# Projekt logiki systemu statów myszy — Myszulandia 2
**Data:** 2026-06-25

---

## 1. Staty potrzeb

Trzy liczniki float (0–100), rosnące pasywnie co sekundę:

| Stat | Nazwa stanu potrzeby | Akcja rozwiązująca | Stan po rozwiązaniu |
|------|---------------------|--------------------|---------------------|
| Głód | Głodna | Karmienie | Czonstkująca |
| Uwaga | Chcąca | Tulanie | Pumpużka |
| Brud | Smrodliwa | Mycie | Pachnąca |

**Kiedy staty rosną:**
- Zawsze, gdy mysz jest w stanie pozytywnym (neutralna lub kolekcjonerska)
- Zamrożone gdy: zły stan (Smutna–Ściekła II) LUB Obrażona

**Kiedy staty są resetowane do 0:**
- Poprawna akcja w stanie potrzeby (karmienie/tulanie/mycie)
- Wejście w zły stan (wszystkie 3 staty → 0)
- Złowroga wywołana przez eskalację potrzeb (wszystkie 3 staty → 0)

---

## 2. Kategorie stanów i priorytety

```
PRIORYTET 1 — ZŁE STANY (najwyższy)
Smutna → Zrozpaczona → Złowroga → Ściekła → Ściekła II
Staty zamrożone na 0. Działają tylko tulanie i całowanie.

PRIORYTET 2 — OBRAŻONA
Osobna mechanika blokady. Staty zamrożone (bez resetowania).

PRIORYTET 3 — STANY POTRZEB
Głodna / Chcąca / Smrodliwa
Przerywają każdy stan pozytywny gdy stat osiągnie 60.

PRIORYTET 4 — STANY POZYTYWNE (najniższy)
Neutralna + wszystkie stany kolekcjonerskie.
Staty rosną normalnie.
```

Wyższy priorytet zawsze wygrywa — np. mysz Tańcząca gdy głód osiągnie 60 natychmiast staje się Głodna.

---

## 3. Stany potrzeb — szczegółowa logika

### Wejście
- Stat osiąga 60 → mysz wchodzi w odpowiedni stan potrzeby (przerywa aktualny stan pozytywny)
- Rower (specjalny trigger): jeśli brud < 60, ustawia brud = 60 → natychmiast Smrodliwa; jeśli brud ≥ 60, brak efektu

### Eskalacja (gracz nie reaguje)
- Aktywuje się 2. stan potrzeby gdy 1. jest już aktywny → **Złowroga**, wszystkie 3 staty = 0
- Aktywny stat potrzeby dochodzi do 100 → **Złowroga**, wszystkie 3 staty = 0

### Wyjście przez dobrą akcję
- Karmienie głodnej → głód = 0, wejście w **Czonstkująca**
- Tulanie chcącej → uwaga = 0, wejście w **Pumpużka**
- Mycie smrodliwej → brud = 0, wejście w **Pachnąca**

### Wyjście przez złą akcję
- Zła akcja w stanie potrzeby → **Złowroga**, wszystkie 3 staty = 0
- Wyjątek: tulanie głodnej lub smrodliwej = brak efektu (nie liczy się jako zła akcja)

---

## 4. Stany pozytywne

### Ze stanu normalnego
| Akcja | Efekt |
|-------|-------|
| Brak aktywności > 1 min | → Smutna |
| Złe jedzenie / okruszki / zła muzyka | → Obrażona |
| Tulanie / całowanie | → Heppi |
| Karmienie (gdy nie głodna) | → Grubol |
| Mycie (gdy nie smrodliwa) | brak efektu |
| Specjalne triggery | → odpowiedni stan kolekcjonerski |

### Timer aktywności
- Resetuje się na KAŻDĄ akcję gracza (dobrą lub złą)
- Działa we WSZYSTKICH stanach normalnych (neutralna bazowa + wszystkie stany kolekcjonerskie)
- Nie działa podczas stanów potrzeb, złych stanów ani Obrażonej

### Stany kolekcjonerskie
- Staty nadal rosną — stan potrzeby (60+) przerywa je natychmiast
- Większość trwa ~1 minutę, potem powrót do neutralnej
- Wyjątki trzymające stan do akcji: Pirat (do kliknięcia łóżka), Myszkująca (do wyjścia z kuchni)
- Tulanie/całowanie w stanie kolekcjonerskim → Heppi

---

## 5. Złe stany

### Wejście w zły stan
- Wszystkie 3 staty → 0 i zamrożone
- Działają WYŁĄCZNIE tulanie i całowanie; wszystkie inne akcje ignorowane

### Łańcuch eskalacji
```
Smutna → Zrozpaczona → Złowroga → Ściekła → Ściekła II (game over)
```
Każdy poziom: brak aktywności > 1 min LUB zła akcja → następny poziom

### Wyjście (tulanie / całowanie = 1 poziom w tył)
```
Smutna      → Neutralna
Zrozpaczona → Smutna
Złowroga    → Zrozpaczona
Ściekła     → Złowroga
Ściekła II  → brak wyjścia (game over)
```

### Po wyjściu ze złego stanu
Wszystkie 3 staty startują od 0 i zaczynają rosnąć.

---

## 6. Obrażona

Specjalny stan z dwufazową mechaniką. Staty zamrożone przez cały czas trwania (nie rosną, nie resetują się do zera — zachowują wartość sprzed wejścia).

### Co wywołuje Obrażoną
- Przeciągnięcie **złego jedzenia** na mysz
- Przeciągnięcie **okruszków** na mysz
- Będąc w stanie **Rozochocona** i wykonanie jakiejkolwiek akcji innej niż bumpcorzenie
- Puszczenie **złej muzyki** w radiu

| Faza | Czas | Efekt akcji |
|------|------|-------------|
| Blokada | 0–60 sek | Wszystkie akcje zablokowane |
| Łaska | 60–120 sek | Tulanie/całowanie → Neutralna; brak aktywności lub zła akcja → Złowroga (z pominięciem Smutnej i Zrozpaczonej) |

**Po wyjściu z Obrażonej:**
- Staty odmrażają się z wartością sprzed zamrożenia
- Jeśli któryś stat był już ≥ 60 — natychmiast aktywuje się odpowiedni stan potrzeby

---

## 7. Tabela akcji vs stany

| Akcja | Stan normalny | Głodna | Chcąca | Smrodliwa | Zły stan | Obrażona (blok) | Obrażona (łaska) |
|-------|--------------|--------|--------|-----------|----------|-----------------|-----------------|
| Tulanie | → Heppi | brak efektu | → Pumpużka | brak efektu | +1 poziom | zablokowane | → Neutralna |
| Całowanie | → Heppi | brak efektu | brak efektu | brak efektu | +1 poziom | zablokowane | → Neutralna |
| Karmienie dobre | → Grubol | → Czonstkująca | → Złowroga | → Złowroga | ignorowane | zablokowane | ignorowane |
| Złe jedzenie / okruszki / zła muzyka | → Obrażona | → Złowroga | → Złowroga | → Złowroga | ignorowane | zablokowane | → Złowroga |
| Mycie | brak efektu | brak efektu | brak efektu | → Pachnąca | ignorowane | zablokowane | ignorowane |
| Dobra muzyka | → Tańcząca | → Złowroga | → Złowroga | → Złowroga | ignorowane | zablokowane | ignorowane |
| Rower (brud < 60) | → Smrodliwa (brud=60) | → Złowroga* | → Złowroga* | brak efektu | ignorowane | zablokowane | ignorowane |

*Rower gdy już aktywny stan potrzeby → 2. stan potrzeby → Złowroga, wszystkie staty = 0

---

## 8. Specjalne triggery stanów kolekcjonerskich

| Trigger | Stan |
|---------|------|
| Tulanie/całowanie w stanie neutralnym/pozytywnym | Heppi |
| Karmienie gdy nie głodna | Grubol |
| Karmienie głodnej | Czonstkująca |
| Tulanie chcącej | Pumpużka |
| Mycie smrodliwej | Pachnąca |
| 3 skarpetki | Maka Paka |
| Róża | Zakochana |
| Kliknięcie łóżka | Pirat |
| Sen/poduszka | Spankowa |
| Bilet lotniczy | Pączurowa |
| Budzik ustawiony na za wczesną godzinę (poprzedniego dnia) | Żółtko nie weszło (Niewyspana) — 1 minuta |
| Budzik wyłączony lub na normalną godzinę (poprzedniego dnia) | Żółtko weszło (Wesoła po pobudce) — 1 minuta |
| Wejście do kuchni | Myszkująca |
| Dobra muzyka w radiu | Tańcząca |
| 100 czonstek | Czonstkowa |
| Radio + świeczki / Zakochana + świeczki | Rozochocona |
| Rozochocona + bumpcorzenie | Roztopiona |
| Rower (brud < 60) | Smrodliwa (brud=60) |
| Przeciągnięcie czosnku na mysz | Czosnkowa |
