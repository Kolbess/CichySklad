# Magazyn Konspiracji — Backlog zadań

Zadania potrzebne do pełnego ukończenia gry, rozpisane na podstawie konspektu i przeglądu
aktualnego stanu projektu. Każdy plik to osobny ticket z kryteriami akceptacji (Acceptance
Criteria) i Definicją Ukończenia (DoD).

## Legenda priorytetów
- **P1 — Blokujące:** bez tego gra nie ma grywalnej/wygrywalnej pętli.
- **P2 — Ważne:** wymagane przez konspekt do „kompletnej” gry.
- **P3 — Wykończenie / release:** polish, buildy, dystrybucja.

## Bloki

### A. Rdzeń rozgrywki — naprawy blokujące (P1)
- ✅ [A1](A1-fix-win-condition.md) — Naprawa warunku zwycięstwa i pętli końca gry
- ✅ [A2](A2-fix-random-rolls.md) — Naprawa błędnych zakresów losowania w cyklu dnia
- ✅ [A3](A3-close-dialogue-choices.md) — Domknięcie wyborów dialogowych (usunięcie zaślepek)
- ✅ [A4](A4-event-activator.md) — System aktywacji/kolejkowania eventów z pełnej puli
- ✅ [A5](A5-remove-debug-code.md) — Usunięcie kodu debugowego z buildu produkcyjnego

### B. Treść — eventy i fabuła (P1–P2)
- ✅ [B6](B6-event-pack-1.md) — Pakiet eventów 1 (10 sytuacji rozgrywkowych)
- ✅ [B7](B7-story-pack-2.md) — Pakiet fabularny 2 (Maria, Pan Kowal, dylematy moralne)
- ✅ [B8](B8-cutscenes.md) — System krótkich cutscenek tekstowych
- [B9](B9-progression-unlocks.md) — Progresja i odblokowania narzędzi konspiracji

### C. Sterowanie i UX (P2)
- [C10](C10-gamepad.md) — Obsługa gamepada (Input System)
- [C11](C11-hud.md) — Spójny HUD (ryzyko / zaufanie / zasoby / czas)
- [C12](C12-save-continue.md) — Zapis gry i „Kontynuuj” / model permadeath

### D. Świat i poziomy (P2–P3)
- [D13](D13-locations.md) — Dodatkowe lokacje (piwnica, punkt kontaktowy, kontrola)
- [D14](D14-interactive-environment.md) — Interaktywne elementy środowiska
- [D15](D15-balance-pacing.md) — Balans i pacing (ramp trudności)

### E. Oprawa audio-wizualna (P2–P3)
- [E16](E16-era-fixes.md) — Poprawa realiów historycznych (XIX w.)
- [E17](E17-character-sprites.md) — Sprite'y i animacje postaci (Maria, Kowal, Sąsiad)
- [E18](E18-audio.md) — Muzyka i efekty dźwiękowe
- [E19](E19-visual-style.md) — Spójny styl wizualny (pixel art, paleta)

### F. Wydanie (P3)
- [F20](F20-builds.md) — Buildy docelowe (Windows / Linux / WebGL)
- [F21](F21-testing.md) — Testy wewnętrzne i rozbudowa testów EditMode
- [F22](F22-release.md) — Finalizacja i publikacja na itch.io

### G. Pętla produkcji i ryzyka (P2)
- ✅ [G23](G23-printer-material-flow.md) — Przepływ materiałów w drukarce (ładowanie → druk → stygnięcie)
- ✅ [G24](G24-packing-station.md) — Stacja pakowania (ulotki → paczka)
- ✅ [G25](G25-package-shelf.md) — Półka na paczki (magazynowanie gotowych paczek)
- ✅ [G26](G26-courier-delivery.md) — Odbiór paczek przez kuriera (zapłata skalowana ryzykiem)
- ✅ [G27](G27-risk-freeze-decay.md) — Zamrożenie ryzyka przed spadkiem (pauza po wzroście)

## Sugerowana kolejność
A1 → A2 → A5 → A3 → A4 → (B6, B7, B8) → C11 → D15 → reszta P2 → blok E → blok F.
Blok A odblokowuje grywalną i testowalną pętlę i powinien być zrobiony w pierwszej kolejności.
