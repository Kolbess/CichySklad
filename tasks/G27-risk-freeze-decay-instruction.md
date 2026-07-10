# G27 — Instrukcja (Zamrożenie ryzyka przed spadkiem)

Po wzroście ryzyka jest ono **utrzymywane przez chwilę** (pauza), zanim zacznie automatycznie
opadać — żeby skok był odczuwalny. Zmiana jest w całości w kodzie; do podpięcia jest tylko jedno pole.

## Co dodaje kod

| Plik | Rola |
|---|---|
| `Assets/Scripts/Core/RiskCalculator.cs` | +`CanDecay(timeSinceLastIncrease, freezeSeconds)` — czysta decyzja „czy już opadać” (test EditMode). |
| `Assets/Scripts/RiskSystem/RiskManager.cs` | Licznik pauzy resetowany przy każdym wzroście; `Update` pomija automatyczny spadek dopóki trwa pauza. |

## Pole w Inspectorze (`RiskManager` → sekcja **Decay Freeze**)

- `Decay Freeze Seconds` (domyślnie `15`) — ile sekund ryzyko **stoi** po wzroście, zanim ruszy
  automatyczny spadek. Konspekt zakłada **10–20 s**. `OnValidate` pilnuje `>= 0`; `0` = spadek od razu.

## Zachowanie

- **Każdy wzrost** ryzyka (pickup, event, `AddRisk`/`SetRisk` w górę) startuje pauzę od nowa.
- **Wzrost w trakcie pauzy** — odświeża jej odliczanie.
- **Po pauzie** ryzyko opada jak dotąd (mnożniki pasm z `RiskCalculator`, bez zmian).
- **Jawne `ReduceRisk`** (redukcje z wyborów gracza / eventów) działają **natychmiast**, niezależnie
  od pauzy — pauza dotyczy wyłącznie automatycznego spadku w `Update`.

## Definicja Ukończenia — status po tej zmianie

- [x] Kod zgodny z regułami Unity 6 (`[SerializeField] private`, `[Tooltip]`, `OnValidate`).
- [x] Logika pauzy (`CanDecay`) pokryta testem EditMode (`RiskCalculatorTests`).
- [x] Pauza → spadek, odświeżanie i niezależność jawnych redukcji w PlayMode (`RiskManagerPlayTests`).
- [ ] Ustawienie `Decay Freeze Seconds` w scenie (opcjonalnie — domyślne 15 jest w zakresie).
- [ ] Commit w konwencji Conventional Commits.
