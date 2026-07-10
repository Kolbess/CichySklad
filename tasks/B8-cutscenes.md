# B8 — System krótkich cutscenek tekstowych

| | |
|---|---|
| **ID** | B8 |
| **Blok** | B — Treść |
| **Priorytet** | P2 |
| **Szacunek** | 1–2 dni |
| **Zależności** | — (używany przez B7) |
| **Pliki** | `Assets/Scripts/DialogueSystem.cs`, nowy np. `Assets/Scripts/CutscenePlayer.cs` |

## Kontekst / problem
Konspekt (Dzień 7) wymaga „systemu krótkich cutscenek tekstowych”. Obecny `DialogueSystem`
pokazuje pojedyncze linie (`ShowDialogue`) lub jeden ekran z wyborami — brak sekwencji wielu
linii z różnymi mówcami/portretami odtwarzanych po kolei.

## Zakres
**W zakresie:** odtwarzacz sekwencji dialogów (linia po linii, „dalej”/auto-advance, portret
per linia, opcjonalny wybór na końcu).
**Poza zakresem:** animowane cutscenki graficzne (poza konspektem).

## Wskazówki implementacyjne
- Model danych: `CutsceneLine { string speakerKey; string text; }` + lista linii.
- Reużyj `GetPortraitSprite` i `_dialogueBox`.
- Wejście: klik / przycisk „Dalej” lub gamepad (spójność z C10); ostatnia linia może wywołać
  `ShowDialogueWithChoices`.
- Blokada nakładania się z eventami/pętlą dnia w trakcie odtwarzania.

## Acceptance Criteria
- [ ] Można zdefiniować i odtworzyć sekwencję ≥3 linii, każda z własnym portretem/mówcą.
- [ ] Gracz przechodzi między liniami (klik/przycisk lub auto-advance z czasem).
- [ ] Sekwencja może zakończyć się oknem wyboru z konsekwencjami.
- [ ] W trakcie cutscenki nie odpalają się równolegle inne eventy / nie znika okno.
- [ ] Sterowanie działa myszą i (po C10) gamepadem.

## Definicja Ukończenia (DoD)
- [ ] Demo-cutscenka odtworzona w Play Mode.
- [ ] Wykorzystana przez co najmniej jeden wątek z B7.
- [ ] Commit w konwencji Conventional Commits.
