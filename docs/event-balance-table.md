# Tabela balansu eventów — Pakiet 1 (B6)

Wspólna tabela konsekwencji dla 10 eventów rozgrywkowych pakietu 1. Wartości pochodzą wprost
z kodu: dobór (waga / dzień / pasmo ryzyka) z `Core/GameEventPool.cs`, a skutki wyborów
z `EventHandler.cs`. Eventy są losowane przez `EventScheduler` (A4) — `DayCycle` nie zawiera puli.

Legenda skutków: `ryzyko` = `RiskManager` (ReduceRisk/AddRisk), `zaufanie` = `ResourceManager.AddTrust`,
zasoby (`papier`, `tusz`, `ulotki`, `pieniądze`) = `ResourceManager`. „(brak środków)” = gałąź gdy
`TrySpend` się nie powiedzie. Zasoby liczone przycinają się do 0; zaufanie jest bez ograniczeń.

## Dobór z puli

| # | Event | `GameEventId` | Waga | Min. dzień | Pasmo ryzyka | Jednorazowy |
|---|-------|---------------|:----:|:----------:|--------------|:-----------:|
| 1 | Rewizja Ochrany (pełna kontrola) | `OchranaRaid` | 3 | 2 | Medium+ | nie |
| 2 | Pukanie do drzwi | `KnockAtDoor` | 8 | 1 | dowolne | nie |
| 3 | Podejrzany sąsiad podgląda | `NeighborPeeking` | 8 | 1 | dowolne | nie |
| 4 | Ranny kurier | `CourierInjured` | 7 | 1 | dowolne | nie |
| 5a | Zniszczona partia papieru | `LostPaperBatch` | 5 | 1 | dowolne | nie |
| 5b | Braki w tuszu | `OutOfInk` | 6 | 1 | dowolne | nie |
| 6 | Wilgoć niszczy papier | `MoistureDamage` | 5 | 1 | dowolne | nie |
| 7 | Donosiciel wypytuje | `InformerAsks` | 5 | 2 | dowolne | nie |
| 8 | Głośny hałas — szybkie chowanie | `LoudNoise` | 5 | 1 | dowolne | nie |
| 9 | Tajna darowizna | `SecretDonation` | 4 | 1 | dowolne | nie |
| 10 | Oferta tańszego papieru | `BuyPaperOffer` | 4 | 1 | dowolne | nie |

## Wybory i konsekwencje

| # | Event | Wybór | Konsekwencja |
|---|-------|-------|--------------|
| 1 | Rewizja Ochrany | „Wręcz łapówkę (5 rubli)” | −5 pieniędzy, ryzyko −20 · (brak środków) ryzyko +15 |
| 1 | Rewizja Ochrany | „Poddaj się rewizji” | ryzyko +10, zaufanie −3 |
| 2 | Pukanie do drzwi | „Otwórz” | ryzyko −10, rozpoczyna inspekcję |
| 2 | Pukanie do drzwi | „Nie otwieraj” | ryzyko +10, rozpoczyna inspekcję |
| 3 | Podejrzany sąsiad | „Zignoruj” | ryzyko +15 |
| 3 | Podejrzany sąsiad | „Grzecznie wyproś” | ryzyko −5 |
| 4 | Ranny kurier | „Pomóż” | ryzyko +10, zaufanie +10 |
| 4 | Ranny kurier | „Zignoruj” | zaufanie −5, ryzyko −5 |
| 5a | Zniszczona partia | „Zapłać donosicielowi (3 ruble)” | −3 pieniędzy, +3 papieru · (brak środków) ryzyko +10 |
| 5a | Zniszczona partia | „Odpuść” | ryzyko +5 |
| 5b | Braki w tuszu | „Zużyj resztki tuszu” | +2 ulotki |
| 5b | Braki w tuszu | „Oszczędzaj tusz” | +1 tusz |
| 6 | Wilgoć niszczy papier | „Wyrzuć zniszczony papier” | −1 papier |
| 6 | Wilgoć niszczy papier | „Użyj wilgotnego papieru” | +2 ulotki, ryzyko +8 |
| 7 | Donosiciel wypytuje | „Skłam” | ryzyko −5 |
| 7 | Donosiciel wypytuje | „Odpraw go” | ryzyko +5 |
| 7 | Donosiciel wypytuje | „Zignoruj” | ryzyko +8 |
| 8 | Głośny hałas | „Błyskawicznie chowaj sprzęt” | ryzyko −8 |
| 8 | Głośny hałas | „Udawaj, że nic się nie stało” | ryzyko +10 |
| 9 | Tajna darowizna | „Weź datek” | +5 pieniędzy, ryzyko +8 |
| 9 | Tajna darowizna | „Zostaw” | zaufanie +5 |
| 10 | Oferta tańszego papieru | „Zainwestuj w tańszy papier (4 ruble)” | −4 pieniędzy, +6 papieru · (brak środków) ryzyko +3 |
| 10 | Oferta tańszego papieru | „Odrzuć ofertę” | zaufanie +2 |

## Zasady spójności

- Każdy event ma tekst PL, ≥2 sensowne wybory i jednoznaczne konsekwencje — brak „martwych”
  wyborów (spójność z A3).
- Dodanie eventu do puli: nowa wartość w `GameEventId`, wiersz w `GameEventPool.Default()` i wpis
  w mapie triggerów `EventScheduler`. `DayCycle` pozostaje nietknięty (A4).
- Testy: `EventSelectorTests` (EditMode) pilnują osiągalności każdego eventu z puli;
  `EventHandlerChoicePlayTests` (PlayMode) sprawdzają skutek każdego wyboru.
