# B9 — Instrukcja (Progresja i odblokowania)

Rosnące zaufanie odblokowuje kolejne „narzędzia konspiracji”. Każdy próg zaufania → jedno
odblokowanie: komunikat + efekt w rozgrywce. Spójne z warunkiem zwycięstwa (A1) — progi trzymaj
**≤ progu zaufania do wygranej**, żeby progresja prowadziła do celu.

## Co dodaje kod

| Plik | Rola |
|---|---|
| `Assets/Scripts/Core/ProgressionLadder.cs` | Czysta logika progów (`IsReached`, `WasJustReached`) — test EditMode. |
| `Assets/Scripts/Unlock.cs` | ScriptableObject: klucz, próg zaufania, tytuł/opis, efekt + siła. |
| `Assets/Scripts/ProgressionSystem.cs` | Nasłuchuje zaufania, przyznaje odblokowania, pokazuje komunikat, stosuje efekt. |
| `Assets/Scripts/ResourceManager.cs` | +`OnTrustChanged` — zdarzenie ze zmianą zaufania. |
| `Assets/Scripts/HidingSpot.cs` | +`IncreaseCapacity` / `MaxCapacity` — efekt „lepsza skrytka”. |

## 1. Zasoby odblokowań (≥3)

Create → **CichySklad → Unlock**. Dla każdego:
- `Key` — unikalny klucz (np. `stash`, `wsparcie`, `zaufany-kurier`). Inne systemy sprawdzają go
  przez `ProgressionSystem.IsUnlocked(key)`.
- `Trust Threshold` — próg zaufania 0..100 (np. `20`, `35`, `50`).
- `Title` / `Description` — treść komunikatu „Odblokowano: …”.
- `Effect` + `Magnitude`:
  - `MoneyReward` — jednorazowo `Magnitude` monet.
  - `ExtraHidingCapacity` — powiększa skrytkę o `Magnitude` (wymaga podpiętej skrytki, patrz niżej).
  - `None` — tylko komunikat/flaga (efekt realizuje inny system czytający `IsUnlocked`).

## 2. Obiekt `ProgressionSystem`

- `Resource Manager` → obiekt z `ResourceManager`. *(wymagane)*
- `Dialogue System` → do komunikatu o odblokowaniu. *(opcjonalne — bez niego log)*
- `Hiding Spot` → skrytka powiększana przez `ExtraHidingCapacity`. *(opcjonalne)*
- `Unlocks` → lista zasobów z pkt 1 (co najmniej 3).

## 3. Jak to działa

- Kurier (G26) i eventy podnoszą zaufanie → `ResourceManager.OnTrustChanged` → `ProgressionSystem`
  sprawdza progi (`ProgressionLadder`) i przyznaje **każde** świeżo przekroczone odblokowanie
  (nawet kilka naraz przy dużym skoku).
- Każde odblokowanie: komunikat + efekt + zdarzenie `OnUnlocked(unlock)`. Zdobyte raz, nie wraca
  przy spadku i ponownym wzroście zaufania.
- Inne systemy mogą bramkować treść przez `ProgressionSystem.IsUnlocked("klucz")` (np. nowa opcja
  dialogowa w evencie).

## 4. Szybki test w Play Mode

1. Podepnij ≥3 `Unlock` (np. `stash` @20 ExtraHidingCapacity, `wsparcie` @35 MoneyReward, `x` @50).
2. Zdobywaj zaufanie (dostawy kuriera / eventy) — po przekroczeniu progu leci komunikat i efekt
   (monety rosną / skrytka ma więcej miejsca).

## Definicja Ukończenia — status

- [x] ≥3 odblokowania powiązane z progami zaufania.
- [x] Odczuwalny efekt (monety / większa skrytka), nie kosmetyka.
- [x] Czytelny komunikat o zdobyciu (`DialogueSystem`).
- [x] Spójność z A1 (progi ≤ próg wygranej — do ustawienia w zasobach).
- [x] PlayMode test (`ProgressionSystemPlayTests`).
- [ ] Podpięcie zasobów/obiektu w scenie.
- [ ] Commit w konwencji Conventional Commits.
