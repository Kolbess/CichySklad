# Wątki fabularne — Pakiet 2 (B7)

Trzy wieloetapowe wątki z ciągłością: decyzja w etapie 1 ustawia flagę (`StoryFlag`), którą etap 2
odczytuje i rozgałęzia przebieg. Wątki są losowane przez `EventScheduler` (A4) — każdy etap jest
`once`, w oknie dni, a etap 2 wymaga flagi „stage 1 done”, więc beaty przychodzą w kolejności.

- Stan: `StoryState` (komponent sceny) — `EventHandler` ustawia flagi z wyborów, `EventScheduler`
  czyta `ActiveFlags` do gatowania puli.
- Portrety: Maria → `maria`, Pan Kowal → `kowal`, Donosiciel → `neighbour`.
- Testy: rozgałęzienia w `StoryThreadPlayTests` (PlayMode); gatowanie flag w `EventSelectorTests`
  (EditMode).

## Maria (łączniczka)

| Etap | Event | Okno dni | Wymaga | Tekst / dylemat | Wybory → konsekwencje |
|------|-------|:--------:|--------|-----------------|-----------------------|
| 1 | `MariaWarns` | 1–3 | — | Ostrzega przed kontrolą | „Zaufaj i przygotuj się” → ryzyko −10, flaga `MariaHeeded` · „Zlekceważ ostrzeżenie” → ryzyko +10 |
| 2 (zaufana) | `MariaRequest` | 3–7 | `MariaStage1Done` + `MariaHeeded` | Ufa ci, przynosi zapas | „Pomóż jej w dostawie” → zaufanie +10, ryzyko +5 · „Odmów” → zaufanie −3 |
| 2 (chłodna) | `MariaRequest` | 3–7 | `MariaStage1Done`, brak `MariaHeeded` | Patrzy z rezerwą | „Spróbuj naprawić relację (2 ruble)” → zaufanie +5 (brak środków: ryzyko +3) · „Zignoruj ją” → zaufanie −5, ryzyko +5 |

## Pan Kowal (szef konspiracji)

| Etap | Event | Okno dni | Wymaga | Tekst / dylemat | Wybory → konsekwencje |
|------|-------|:--------:|--------|-----------------|-----------------------|
| 1 | `LetterFromPanKowal` | 2–4 | — | Dylemat: ruch vs siebie | „Podejmij zadanie” → ryzyko +10, zaufanie +10, flaga `KowalAcceptedTask` · „Odmów” → ryzyko −5, zaufanie −5 |
| 2 (przyjął) | `KowalTask` | 4–8 | `KowalStage1Done` + `KowalAcceptedTask` | Czas dostarczyć odezwy | „Rozprowadź ulotki” → ryzyko +10, zaufanie +10 · „Wycofaj się” → zaufanie −8, ryzyko −5 |
| 2 (odmówił) | `KowalTask` | 4–8 | `KowalStage1Done`, brak `KowalAcceptedTask` | Tylko drobna rola | „Przyjmij drobną robotę” → zaufanie +4, ryzyko +3 · „Trzymaj się z boku” → zaufanie −3 |

## Donosiciel (narastające podejrzenie → kulminacja)

| Etap | Event | Okno dni | Wymaga | Tekst / dylemat | Wybory → konsekwencje |
|------|-------|:--------:|--------|-----------------|-----------------------|
| 1 | `InformerSuspicion` | 2–4 | — | Węszy koło składu | „Przekup go (4 ruble)” → ryzyko −8, flaga `InformerAppeased` (brak środków: ryzyko +8) · „Udawaj obojętność” → ryzyko +6 |
| 2 (spłacony) | `InformerDisappears` | 5–11 | `InformerStage1Done` + `InformerAppeased` | Kulminacja: cicho znika | (bez wyboru) ryzyko −10 — ulga |
| 2 (niespłacony) | `InformerDisappears` | 5–11 | `InformerStage1Done`, brak `InformerAppeased` | Kulminacja: donosi | (bez wyboru) ryzyko +30 — Ochrana węszy |

## Zasady

- Wcześniejsza decyzja wpływa na dalszy przebieg wątku (flagi rozgałęziają etap 2).
- Wszystkie etapy `once` — nie powtarzają się w obrębie jednego przejścia.
- Dodanie etapu/wątku: `GameEventId` + `StoryFlag` + wiersze w `GameEventPool` (z `requiredFlags`)
  + trigger w `EventScheduler` + handler w `EventHandler`. `DayCycle` pozostaje nietknięty.
