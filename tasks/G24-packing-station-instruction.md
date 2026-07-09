# G24 — Instrukcja podpięcia w Edytorze (Packing Station)

Kod jest gotowy i sformatowany. To, czego **nie da się zrobić z CLI**, to wizualne podpięcie sceny.
Poniżej krok po kroku, jak uruchomić pętlę pakowania `Idle → Filling → Packing → Ready → Idle`
w Unity 6. Model jest spójny z G23 (drukarka): fizyczne obiekty przeciągane triggerem.

## Co dodaje kod

| Plik | Rola |
|---|---|
| `Assets/Scripts/Core/PackingState.cs` | Enum faz cyklu (czysta logika, `Core`). |
| `Assets/Scripts/Core/PackingCycle.cs` | Maszyna stanów + czas + pojemność (test EditMode). |
| `Assets/Scripts/PackableLeaflet.cs` | Znacznik na obiekcie ulotki, który stacja przyjmuje. |
| `Assets/Scripts/PackedParcel.cs` | Zaplombowana paczka-wynik: przenośny item (nie otwiera się). |
| `Assets/Scripts/PackingStation.cs` | Stacja: ładowanie → koszt → pakowanie → wydanie paczki. |

---

## 1. Stacja pakowania (obiekt z `PackingStation`)

Utwórz/wybierz obiekt stacji pakowania i dodaj **`Packing Station`**.

### 1a. Collider do wykrywania ulotek
Jak w drukarce (G23) i `HidingSpot`:
- Dodaj **Collider2D z `Is Trigger`** obejmujący pole wkładania ulotek.
- Ten sam collider obsłuży kliknięcie stacji (Start / odbiór paczki).
- **Reguła triggerów 2D:** ulotka musi mieć `Rigidbody2D` (kinematic wystarcza), tak jak inne
  przeciągane obiekty.

### 1b. Pola w Inspectorze `PackingStation`

**Dependencies**
- `Resource Manager` → obiekt sceny z `ResourceManager`. *(wymagane)*

**UI Elements**
- `Progress Slider` → slider paska postępu pakowania (0..1). *(wymagane)*
- `Status Text` → `TextMeshProUGUI` na licznik „Ulotki: x/2” oraz komunikaty
  („Włóż ulotkę do zapakowania!”, „Brak monety!”, „Gotowe! Kliknij, aby odebrać paczkę.”). *(wymagane)*

**Packing**
- `Capacity` — maks. ulotek naraz (domyślnie `2`).
- `Packing Cost` — koszt startu w monetach (domyślnie `1`).
- `Min Pack Time` / `Max Pack Time` — zakres losowego czasu pakowania (domyślnie `1`–`3` s).
  `OnValidate` pilnuje `min > 0` i `max >= min`.

**Output**
- `Parcel Prefab` → prefab **zaplombowanej paczki** z komponentem `PackedParcel`. Ma być
  **przenośny, ale nie otwieralny**: dodaj `InteractableObject` (przeciąganie) + `SpriteRenderer` +
  `Collider2D`, ale **nie** komponent `Package` (ten otwiera się po kliknięciu). Stacja stempluje
  liczbę ulotek przez `PackedParcel.Initialize(...)`. *(wymagane)*
- `Dispense Point` → `Transform`, gdzie pojawia się gotowa paczka. Opcjonalne (fallback: pozycja stacji).

**Load Points (optional)**
- `Leaflet Slots` — lista `Transform`ów, pod które kolejno podpinane są załadowane ulotki
  (dziecko, `localPosition = 0`, `localScale = 1`, więc przejmują pozycję i skalę slotu).
  Opcjonalne; gdy pusto, ulotki lądują na pozycji stacji.

---

## 2. Obiekt ulotki (wejście)

Ulotka, którą gracz wrzuca do stacji, potrzebuje znacznika:
1. Zaznacz prefab ulotki (ten sam, który produkuje drukarka z G23) → **Add Component →
   `Packable Leaflet`**.
2. Upewnij się, że ma **Collider2D**, **Rigidbody2D** (dla triggerów) i mechanizm przeciągania
   (`InteractableObject`), tak jak reszta obiektów.

> Po załadowaniu ulotki jej `InteractableObject` i **wszystkie Collider2D** są wyłączane (nie da się
> jej wyciągnąć), a przy starcie pakowania obiekt jest **niszczony** — to widoczne zużycie.

---

## 3. Przebieg (co robi gracz)

1. Przeciąga ulotkę na stację i puszcza → licznik „Ulotki: 1/2” (`Filling`). Druga ulotka → 2/2.
   Trzecia jest **odrzucana** (pojemność 2).
2. Klika stację → **pobiera 1 monetę** i rusza pakowanie (`Packing`) na losowe 1–3 s; pasek rośnie.
   - Brak ulotek → „Włóż ulotkę do zapakowania!”, brak startu.
   - Brak monety → „Brak monety!”, brak startu, ulotki zostają.
3. Po zakończeniu (`Ready`) → komunikat „Gotowe! Kliknij, aby odebrać paczkę.”.
4. Klika stację → dostaje **zaplombowaną paczkę** (`PackedParcel`) — przenośny item, który **nie
   otwiera się** po kliknięciu; można go przeciągać. Stacja wraca do `Idle`.

> Kliknięcie stacji jest kontekstowe: w `Ready` = **odbierz paczkę**, w innym stanie = **Start**.
> Możesz też podpiąć osobny przycisk UI do `PackingStation → StartPacking ()` /
> `PackingStation → CollectPackage ()`.

---

## 4. Szybki test w Play Mode

1. Start. `Status Text` = „Ulotki: 0/2”, pasek pusty.
2. Wrzuć 2 ulotki → „Ulotki: 2/2”, trzecia odrzucona.
3. Kliknij → monety −1, pasek rusza, ulotki znikają.
4. Po 1–3 s → „Gotowe! Kliknij…”.
5. Kliknij → pojawia się paczka; „Ulotki: 0/2”.
6. Bez monety (wydaj wszystkie) → przy starcie „Brak monety!”, zero zużycia.

---

## ⚠️ Uwaga o liczniku ulotek

Jedynym kosztem w `ResourceManager` jest **1 moneta**. Fizyczne ulotki są niszczone przy starcie
pakowania, a ich liczba jest zapisywana w `PackedParcel.LeafletCount`, ale **licznik
`ResourceManager.Leaflets` nie jest automatycznie zmniejszany** (jak w G23 — model fizyczny vs
licznik). Jeśli w Twojej scenie wrzucane ulotki mają odpowiadać licznikowi, zdejmij je z licznika
samodzielnie, żeby liczby się zgadzały.

---

## Definicja Ukończenia — status po tej zmianie

- [x] Kod zgodny z regułami Unity 6 (`[SerializeField] private`, `[Tooltip]`, `Awake`, `OnValidate`).
- [x] Logika pojemności/czasu/stanów w `PackingCycle` pokryta testem EditMode (`PackingCycleTests`).
- [x] Pełny cykl wkładanie→koszt→pakowanie→wydanie w PlayMode (`PackingStationPlayTests`).
- [ ] Podpięcie sceny wg tej instrukcji (ręcznie w Edytorze).
- [ ] Commit w konwencji Conventional Commits.
