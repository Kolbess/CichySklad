# G27 — Zamrożenie ryzyka przed spadkiem (pauza po wzroście)

| | |
|---|---|
| **ID** | G27 |
| **Status** | ✅ Zrobione |
| **Blok** | G — Pętla produkcji i ryzyka |
| **Priorytet** | P2 — Ważne |
| **Szacunek** | 0.5–1 dzień |
| **Zależności** | Istniejący `RiskManager` + `RiskCalculator` |
| **Pliki** | `Assets/Scripts/RiskSystem/RiskManager.cs`, `Assets/Scripts/Core/RiskCalculator.cs` |

## Kontekst / problem
Obecnie ryzyko zaczyna opadać natychmiast po każdym wzroście (`RiskManager.Update` co klatkę woła
`ReduceRisk` z mnożnikiem pasma). Przez to skoki ryzyka są mało odczuwalne — kara „rozpływa się”
od razu. Chcemy, aby po wzroście ryzyko przez chwilę **utrzymywało się** (pauza), zanim zacznie
spadać, co zwiększa napięcie i wagę decyzji.

## Zakres
**W zakresie:**
- Po wzroście ryzyka rozpoczyna się **pauza 10–20 s** (konfigurowalna), w trakcie której ryzyko
  nie opada.
- Kolejny wzrost w trakcie pauzy **odświeża** jej odliczanie.
- Po upływie pauzy ryzyko opada jak dotąd (z mnożnikami pasm z `RiskCalculator`).

**Poza zakresem:** zmiana samych mnożników/pasm (D15 — balans), UI licznika pauzy (opcjonalny wskaźnik).

## Wskazówki implementacyjne
- W `RiskManager` dodać `[SerializeField] [Tooltip] float _decayFreezeSeconds` (waliduj ≥ 0 w `OnValidate`)
  oraz licznik pauzy resetowany w `AddRisk`/`SetRisk` przy wzroście wartości.
- W `Update`: dekrementować licznik pauzy `Time.deltaTime`; dopóki > 0 — pomijać `ReduceRisk`.
- Decyzję „czy już można opadać” trzymać jako **czystą funkcję** (np. `RiskCalculator.CanDecay(
  timeSinceLastIncrease, freezeSeconds)` lub prosty helper), aby pokryć ją testem EditMode.
- Uwaga na spadki wynikające z decyzji gracza (`ReduceRisk` wołane jawnie z eventów) — pauza dotyczy
  wyłącznie **automatycznego** opadania w `Update`, nie jawnych redukcji z wyborów.

## Acceptance Criteria
- [ ] Po wzroście ryzyka przez konfigurowalny czas (10–20 s) automatyczne opadanie jest wstrzymane.
- [ ] Wzrost w trakcie pauzy odświeża jej odliczanie od nowa.
- [ ] Po pauzie ryzyko opada zgodnie z dotychczasowymi mnożnikami pasm.
- [ ] Jawne redukcje ryzyka z wyborów gracza działają niezależnie od pauzy.
- [ ] Czas pauzy konfigurowalny w Inspectorze z `[Tooltip]` i walidacją w `OnValidate`.

## Definicja Ukończenia (DoD)
- [ ] Kod zgodny z regułami Unity 6 (`codingRules.txt`).
- [ ] Logika pauzy pokryta testem EditMode; zachowanie (pauza → spadek, odświeżanie) zweryfikowane w Play Mode (PlayMode test na `RiskManager`).
- [ ] Commit w konwencji Conventional Commits.
