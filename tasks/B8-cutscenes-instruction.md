# B8 — Instrukcja (System krótkich cutscenek tekstowych)

Odtwarzacz sekwencji dialogów: linia po linii, portret per mówca, przejście klikiem / „Dalej” /
gamepadem (po C10) lub auto-advance, opcjonalny wybór na końcu. W trakcie cutscenki okno dialogowe
jest zablokowane, więc eventy/pętla dnia go nie nadpisują.

## Co dodaje kod

| Plik | Rola |
|---|---|
| `Assets/Scripts/Cutscene.cs` | `CutsceneLine` (mówca + tekst + auto-advance) i `Cutscene` (ScriptableObject z listą linii). |
| `Assets/Scripts/CutscenePlayer.cs` | Odtwarza sekwencję przez `DialogueSystem`. |
| `Assets/Scripts/DialogueSystem.cs` | +`ShowLine`/`Hide`/`SetLocked`/`IsLocked`; `ShowDialogue*` ignorowane, gdy zablokowane. |

## 1. Zasób cutscenki

Utwórz zasób: **Create → CichySklad → Cutscene**. Wypełnij `Lines`:
- `Speaker Key` — klucz portretu dla `DialogueSystem.GetPortraitSprite` (`maria`, `ochrana`, `kowal`,
  `neighbour`; puste = domyślny).
- `Text` — treść linii.
- `Auto Advance Seconds` — `0` = czekaj na gracza; `>0` = przejdź samo po tylu sekundach.

## 2. Obiekt `CutscenePlayer`

- `Dialogue System` → obiekt z `DialogueSystem`. *(wymagane)*
- `Demo Cutscene` → (opcjonalnie) zasób odtworzony raz na `Start` — szybkie demo w Play Mode.

## 3. Odtwarzanie z kodu

- `Play(cutscene, onComplete)` — zwykła sekwencja; po ostatniej linii okno się chowa i leci `onComplete`.
- `PlayWithEndChoices(cutscene, choices)` — ostatnia linia pokazuje przyciski wyboru
  (`DialogueSystem.Choice[]`, akcje w kodzie); wybór kończy cutscenkę.
- `Advance()` — publiczne; podepnij pod przycisk „Dalej” lub gamepad (C10). Klik myszy działa od razu.
- `OnCutsceneFinished` — zdarzenie po zakończeniu (lub po wyborze końcowym).
- `IsPlaying` — czy trwa odtwarzanie (np. dla pętli dnia / eventów, jeśli chcą dodatkowo pauzować).

> **B7:** co najmniej jeden wątek fabularny powinien wywołać `Play(...)` z zasobem cutscenki.

## 4. Blokada nakładania (AC)

Podczas cutscenki `DialogueSystem` jest zablokowany: `ShowDialogue`/`ShowDialogueWithChoices`
wołane przez eventy są **ignorowane**, więc nie nadpisują ani nie chowają okna. Blokada zwalnia się
po ostatniej linii (przed pokazaniem wyboru) i na końcu.

## 5. Szybki test w Play Mode

1. Podepnij `Demo Cutscene` (≥3 linie z różnymi `Speaker Key`) i uruchom scenę.
2. Klikaj (lub czekaj na auto-advance) — linie lecą po kolei z właściwymi portretami.
3. Dla wersji z wyborem: ostatnia linia pokazuje przyciski; wybór wywołuje konsekwencję.

## Definicja Ukończenia — status

- [x] Sekwencja ≥3 linii, portret per linia; przejście klik/auto; wybór na końcu; blokada eventów.
- [x] PlayMode test (`CutscenePlayerPlayTests`).
- [ ] Demo-cutscenka odtworzona w Play Mode (podpięcie `Demo Cutscene`).
- [ ] Użyta przez co najmniej jeden wątek z B7.
- [ ] Commit w konwencji Conventional Commits.
