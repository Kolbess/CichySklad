# G26 — Instrukcja podpięcia w Edytorze (Courier Delivery)

Kurier domyka pętlę produkcji: odbiera **1 paczkę** na wizytę (z półki G25 lub przeciągniętą przez
gracza), płaci monety **skalowane ryzykiem (1–5)** i podnosi zaufanie. Poniżej co podpiąć w Unity 6.

## Co dodaje kod

| Plik | Rola |
|---|---|
| `Assets/Scripts/Core/RiskCalculator.cs` | +`PaymentForRisk` — czysta funkcja ryzyko→zapłata (test EditMode). |
| `Assets/Scripts/Courier.cs` | Kurier: odbiór paczki, wypłata, zaufanie, komunikat. |

---

## 1. Obiekt kuriera (`Courier`)

Dodaj komponent **`Courier`**.

### 1a. Collider do odbioru przeciągnięciem
- Dodaj **Collider2D z `Is Trigger`** obejmujący kuriera (żeby dało się upuścić na niego paczkę).
- Paczka (`PackedParcel`) ma już `Rigidbody2D` z G24 — wymagane dla triggerów 2D.

### 1b. Pola w Inspectorze

**Dependencies**
- `Risk Manager` → obiekt sceny z `RiskManager`. *(wymagane)*
- `Resource Manager` → obiekt sceny z `ResourceManager`. *(wymagane)*
- `Package Shelf` → półka z G25, z której kurier bierze paczkę po kliknięciu. *(opcjonalne — sam
  drag-drop działa bez tego)*
- `Dialogue System` → do komunikatów o odbiorze / braku paczki. *(opcjonalne — bez niego trafia do logu)*

**Payment (coins, scaled by risk band)**
- `Min Payment` (domyślnie `1`) — zapłata przy najniższym paśmie ryzyka.
- `Max Payment` (domyślnie `5`) — zapłata przy najwyższym paśmie ryzyka.
  Cztery pasma `RiskLevel` (Low→Critical) są rozłożone równo na `[Min, Max]` → 1, 2, 4, 5 dla 1–5.

**Reward**
- `Trust Reward` (domyślnie `5`) — przyrost zaufania za udaną dostawę.

**Appearance Schedule**
- `Appears On Schedule` (domyślnie **on**) — kurier nie stoi cały czas; pojawia się tylko w dni
  wizyty. Wyłącz, jeśli ma być zawsze obecny.
- `Day Cycle` → obiekt z `DayCycle` (źródło dni). **Wymagane, gdy `Appears On Schedule` jest on** —
  bez tego kurier nigdy się nie pojawi.
- `Min Gap Days` (domyślnie `2`) / `Max Gap Days` (domyślnie `4`) — losowy, ale **gwarantowany**
  odstęp między wizytami: kurier zawsze przyjdzie w ciągu `Max Gap Days` dni.

> Gdy kuriera nie ma, jego `SpriteRenderer` i `Collider2D` są wyłączone (niewidoczny, nieklikalny),
> a komponent nadal słucha `DayCycle.OnDayStarted`. Po odebraniu paczki kurier odchodzi do następnej
> zaplanowanej wizyty. Dodaj mu `SpriteRenderer` (sylwetka) i trigger `Collider2D`.

---

## 2. Przebieg (co robi gracz)

0. Kurier **pojawia się co 2–4 dni** (patrz Appearance Schedule). Poza dniami wizyty jest nieobecny.
1. W dzień wizyty **przeciąga paczkę na kuriera** i puszcza → kurier ją odbiera, płaci wg ryzyka,
   podnosi zaufanie, **zdejmuje z licznika `Leaflets` tyle ulotek, ile było w paczce**
   (`PackedParcel.LeafletCount`), paczka znika, kurier odchodzi.
2. **Albo klika kuriera** → bierze pierwszą paczkę z podpiętej półki (G25) i płaci tak samo; slot na
   półce zwalnia się automatycznie.
3. **Brak paczki** (pusta półka / brak półki przy kliknięciu) → komunikat „Kurier: brak paczki do
   odebrania.”, zero wypłaty.

> Odbiór to zawsze **1 paczka na akcję**. Wyższe bieżące ryzyko = wyższa stawka (od `Min` do `Max`).
> `RiskManager` czyta bieżące pasmo w momencie odbioru.

> **Wizyta z eventu (opcjonalnie):** `Courier.CollectFromShelf()` jest publiczne — możesz je wywołać
> z `EventScheduler`/puli eventów, żeby zrobić okresową wizytę kuriera. (Nie podpięte w kodzie —
> zostawiam do decyzji projektanta.)

---

## 3. Szybki test w Play Mode

1. Ustaw ryzyko nisko → odbierz paczkę → +1 moneta, +zaufanie.
2. Podbij ryzyko wysoko → odbierz paczkę → +5 monet.
3. Kliknij kuriera z pełną półką → bierze 1 paczkę, slot wolny.
4. Kliknij kuriera z pustą półką → komunikat, zero monet.

---

## Definicja Ukończenia — status po tej zmianie

- [x] Kod zgodny z regułami Unity 6 (`[SerializeField] private`, `[Tooltip]`, `Awake`, `OnValidate`).
- [x] Mapowanie ryzyko→zapłata pokryte testem EditMode (`RiskCalculatorTests`).
- [x] Odbiór + wypłata + zaufanie + brak-paczki w PlayMode (`CourierPlayTests`).
- [ ] Podpięcie sceny wg tej instrukcji (ręcznie w Edytorze).
- [ ] Commit w konwencji Conventional Commits.
