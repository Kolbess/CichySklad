# G25 — Instrukcja podpięcia w Edytorze (Package Shelf)

Półka porządkuje **gotowe paczki z G24** (`PackedParcel`) w wyznaczonych slotach do czasu odbioru
przez kuriera (G26). Wzorowana na `HidingSpot`. Poniżej co podpiąć w Unity 6.

## Co dodaje kod

| Plik | Rola |
|---|---|
| `Assets/Scripts/Core/ShelfSlotMap.cs` | Czysta logika zajętości slotów (test EditMode). |
| `Assets/Scripts/PackageShelf.cs` | Półka: przyjmowanie/zwalnianie paczek, limit, zdarzenie. |
| `Assets/Scripts/PackedParcel.cs` | Rozszerzony o powiązanie z półką (zwalnia slot przy zniszczeniu). |

---

## 1. Obiekt półki (`PackageShelf`)

Dodaj komponent **`Package Shelf`** na obiekt półki.

### 1a. Collider do wykrywania paczek
Jak w `HidingSpot`:
- Dodaj **Collider2D z `Is Trigger`** obejmujący obszar półki.
- **Reguła triggerów 2D:** paczka (`PackedParcel`) musi mieć `Rigidbody2D` (kinematic wystarcza) —
  ma to już z G24 (jest przeciągana).

### 1b. Pola w Inspectorze

**Slots**
- `Slots` → lista `Transform`ów-slotów w kolejności. **Liczba slotów = pojemność półki.** Rozmieść
  je tam, gdzie paczki mają stać. Wymagany co najmniej jeden (asercja w `Awake`).

> Pojemność jest zdefiniowana liczbą slotów — to jedyne źródło prawdy, więc nie da się ustawić
> pojemności większej niż liczba miejsc. Chcesz większą półkę → dodaj sloty.

---

## 2. Paczka (`PackedParcel`, z G24)

Nic nowego do dodania — paczka z G24 już ma `PackedParcel` + `InteractableObject` (przeciąganie) +
`Collider2D` + `Rigidbody2D`. Kod paczki dostał tylko powiązanie z półką, żeby przy zniszczeniu
zwolnić slot.

---

## 3. Przebieg (co robi gracz)

1. Przeciąga gotową paczkę na półkę i puszcza → paczka **przeskakuje na wolny slot** (pozycja slotu).
2. Pełna półka **nie przyjmuje** kolejnych paczek (paczka zostaje w ręce/na podłodze).
3. Przeciąga paczkę z półki → opuszcza obszar półki → **slot się zwalnia**.
4. Zniszczenie/odebranie paczki (np. kurier w G26) też **zwalnia slot** — brak martwych referencji.

> Dla kuriera (G26): półka wystawia `StoredParcels`, `StoredCount`, `IsFull`, `Capacity`, publiczne
> `TryStore(...)` / `RemoveParcel(...)` oraz zdarzenie **`OnContentsChanged`** (bez twardych referencji).

---

## 4. Szybki test w Play Mode

1. Ustaw 2–3 sloty. Start.
2. Odłóż paczki po kolei → lądują na kolejnych slotach, uporządkowane.
3. Odłóż o jedną za dużo → odrzucona (limit).
4. Zabierz jedną paczkę → slot wolny, można odłożyć nową.
5. Zniszcz paczkę na półce → slot wolny (licznik spada).

---

## Definicja Ukończenia — status po tej zmianie

- [x] Kod zgodny z regułami Unity 6 (`[SerializeField] private`, `[Tooltip]`, `Awake` asercje).
- [x] Logika zajętości slotów pokryta testem EditMode (`ShelfSlotMapTests`).
- [x] Odkładanie/pobieranie i limit pojemności w PlayMode (`PackageShelfPlayTests`).
- [ ] Podpięcie sceny wg tej instrukcji (ręcznie w Edytorze).
- [ ] Commit w konwencji Conventional Commits.
