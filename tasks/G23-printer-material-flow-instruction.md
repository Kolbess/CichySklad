# G23 — Instrukcja podpięcia w Edytorze (Printer Material Flow)

Kod jest gotowy i przetestowany (EditMode + PlayMode zielone). To, czego **nie da się zrobić z CLI**,
to wizualne podpięcie sceny. Poniżej krok po kroku, co kliknąć w Unity 6, żeby nowa pętla
`Idle → Loaded → Printing → CoolingDown → Idle` działała w grze.

## Co się zmieniło w kodzie

| Plik | Rola |
|---|---|
| `Assets/Scripts/Core/PrinterState.cs` | Enum faz cyklu (czysta logika, `Core`). |
| `Assets/Scripts/Core/PrinterCycle.cs` | Maszyna stanów + czasy druku/stygnięcia (testowalna w EditMode). |
| `Assets/Scripts/PrinterMaterial.cs` | Znacznik na obiekcie: `Paper` albo `Ink`. |
| `Assets/Scripts/PrintLeaflet.cs` | Przerobiona stacja: ładowanie → Start → druk → stygnięcie. |

Nazwy pól przeniesione przez `FormerlySerializedAs`, więc **istniejące podpięcia na stacji
(slider, tekst kosztu, koszty, cooldown) nie odpadną** — zostaną automatycznie przemapowane.

---

## 1. Stacja drukarki (obiekt z `PrintLeaflet`)

Otwórz obiekt drukarki na scenie. Komponent `Print Leaflet` ma teraz nowe sekcje w Inspectorze.

### 1a. Collider do wykrywania materiałów
Ładowanie działa przez trigger 2D (tak jak w `HidingSpot`):
- Dodaj/ustaw na drukarce **Collider2D z zaznaczonym `Is Trigger`** (np. `BoxCollider2D`),
  obejmujący pole „wlotu” materiału.
- Ten sam collider obsłuży też kliknięcie stacji (`OnMouseDown` = Start), więc jeden trigger
  wystarcza.
- **Ważne (reguła triggerów 2D):** aby `OnTriggerStay2D` odpalało, przeciągany materiał musi mieć
  `Rigidbody2D` (kinematic wystarcza) — dokładnie tak jak inne przeciągane obiekty, które już
  wpadają do `HidingSpot`. Jeśli materiał robisz z istniejącego, przeciąganego obiektu, ma to już.

### 1b. Pola w Inspectorze `PrintLeaflet`

**Dependencies**
- `Resource Manager` → przeciągnij obiekt sceny z `ResourceManager`. *(wymagane — twardy assert w `Awake`)*

**UI Elements**
- `Progress Slider` → slider paska postępu (0..1). Pokazuje najpierw druk, potem stygnięcie.
  *(wymagane; jeśli miałeś podpięty stary „cooldown slider”, przemapuje się sam)*
- `Cost Text` → `TextMeshProUGUI` pokazujący koszt na hover oraz komunikaty
  („Załaduj papier i tusz!”, „Brak materiałów!”). *(wymagane)*

**Timing**
- `Print Duration` — sekundy druku (domyślnie `6`).
- `Cooldown Duration` — sekundy stygnięcia (domyślnie `2`). **Musi być krótsze niż druk** —
  `OnValidate` przytnie i ostrzeże, jeśli ustawisz >= druku.

**Costs**
- `Cost Ink` (domyślnie `1`), `Cost Paper` (domyślnie `2`) — pobierane z `ResourceManager` w
  momencie naciśnięcia Start.

**Load Points (optional)**
- `Paper Slot`, `Ink Slot` — puste `Transform`y, do których „przyklei się” załadowany obiekt.
  Opcjonalne; gdy puste, materiał ląduje na pozycji drukarki.

---

## 2. Obiekty materiałów (papier / tusz)

Każdy przeciągany obiekt papieru/tuszu, który ma wpadać do drukarki, potrzebuje znacznika:

1. Zaznacz prefab/obiekt papieru → **Add Component → `Printer Material`** → `Type = Paper`.
2. Zaznacz prefab/obiekt tuszu → **Add Component → `Printer Material`** → `Type = Ink`.
3. Upewnij się, że obiekt ma **Collider2D** (`PrinterMaterial` tego wymaga) oraz **Rigidbody2D**
   (dla triggerów, patrz 1a) i mechanizm przeciągania, którego już używasz (`InteractableObject`).

> Gdy materiał zostanie załadowany, jego `InteractableObject` jest wyłączany (nie da się go
> wyciągnąć z powrotem), a przy starcie druku obiekt jest **niszczony** — to widoczne zużycie.

---

## 3. Przycisk „Start” (opcjonalny, obok kliknięcia stacji)

Domyślnie **kliknięcie stacji = Start**. Jeśli chcesz osobny przycisk UI:
1. Dodaj `Button` (UI).
2. W `On Click ()` dodaj wpis → przeciągnij obiekt drukarki → wybierz
   **`PrintLeaflet → StartPrint ()`**.

`StartPrint()` sam sprawdzi, czy komplet materiałów jest załadowany i czy stać nas na koszt —
w przeciwnym razie pokaże komunikat i nic nie zużyje.

---

## 4. Pasek postępu

- Ustaw `Slider`: `Min Value = 0`, `Max Value = 1` (kod i tak to wymusza w `Start`).
- W trakcie druku wypełnia się 0→1, potem resetuje i wypełnia się w fazie stygnięcia,
  a po powrocie do `Idle` wraca do 0.

---

## 5. Szybki test w Play Mode

1. Start gry. Slider pusty, stan `Idle`.
2. Przeciągnij papier na drukarkę i puść → nic (za mało).
3. Przeciągnij tusz i puść → stacja „uzbrojona” (`Loaded`).
4. Kliknij stację (lub Start) → papier/tusz znikają, liczniki spadają, pasek rusza (`Printing`).
5. Po `Print Duration` → pojawia się ulotka (+1), pasek startuje fazę stygnięcia (`CoolingDown`).
6. Kolejny druk zablokowany do końca stygnięcia; potem wraca `Idle`.
7. Naciśnięcie Start bez materiałów → komunikat „Załaduj papier i tusz!”, zero zużycia.

---

## ⚠️ Uwaga o podwójnym zużyciu

Zużycie jest celowo dwutorowe: załadowane obiekty są **niszczone** (widoczny efekt „materiały
zniknęły”) **oraz** `ResourceManager.TrySpend` zdejmuje z liczników. Jeśli w Twojej scenie
załadowane obiekty to **te same jednostki**, które śledzi licznik, ustaw `Cost Ink`/`Cost Paper`
tak, aby nie liczyć ich dwa razy (np. koszt `0`, jeśli fizyczny obiekt jest jedyną „walutą”).

---

## Definicja Ukończenia — status po tej zmianie

- [x] Kod zgodny z regułami Unity 6 (`[SerializeField] private`, `[Tooltip]`, `OnValidate`).
- [x] Logika stanów/czasów w `PrinterCycle` pokryta testem EditMode (`PrinterCycleTests`).
- [x] Ścieżka ładowanie→druk→stygnięcie w PlayMode (`PrintLeafletPlayTests`).
- [ ] Podpięcie sceny wg tej instrukcji (ręcznie w Edytorze).
- [ ] Commit w konwencji Conventional Commits.
