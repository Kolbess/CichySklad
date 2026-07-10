# G23 — Przepływ materiałów w drukarce (ładowanie → druk → stygnięcie)

| | |
|---|---|
| **ID** | G23 |
| **Status** | ✅ Zrobione |
| **Blok** | G — Pętla produkcji i ryzyka |
| **Priorytet** | P2 — Ważne |
| **Szacunek** | 1–2 dni |
| **Zależności** | A5 (usunięty debug), istniejący `PrintLeaflet`, `Package`/`InteractableObject` |
| **Pliki** | `Assets/Scripts/PrintLeaflet.cs`, `Assets/Scripts/ResourceManager.cs`, `Assets/Scripts/InteractableObject.cs` |

## Kontekst / problem
Obecnie druk ulotki to pojedyncze kliknięcie na stację (`PrintLeaflet.OnMouseDown`), które od razu
woła `TrySpend(costPaper, costInk)` i dodaje ulotkę po cooldownie. Materiały znikają „w tle” —
gracz nie widzi, że papier/atrament są realnie zużywane, a produkcja nie ma czytelnej fazy pracy
maszyny. Konspekt zakłada odczuwalną pętlę produkcyjną: najpierw nakarm maszynę, potem ją uruchom.

## Zakres
**W zakresie:**
- Fizyczne ładowanie materiałów do drukarki: gracz przeciąga papier i atrament (widoczne obiekty)
  do stacji, a nie tylko klika.
- Osobny przycisk/akcja „Start” uruchamiająca druk dopiero po skompletowaniu materiałów.
- Faza druku trwająca określony czas (pasek postępu), a po niej faza stygnięcia (cooldown)
  **wyraźnie krótsza** niż sam druk.
- Widoczne zużycie materiałów (zniknięcie włożonych obiektów / spadek licznika).

**Poza zakresem:** pakowanie ulotek (G24), półka na paczki (G25).

## Wskazówki implementacyjne
- Rozbić `PrintLeaflet` na stan: `Idle → Loaded → Printing → CoolingDown → Idle`.
- Nowe pola z `[SerializeField] [Tooltip]`: `_printDuration`, `_cooldownDuration`
  (waliduj w `OnValidate`, że `_cooldownDuration < _printDuration`).
- Ładowanie: wykorzystać istniejący mechanizm przeciągania (`InteractableObject` / triggery jak
  w `HidingSpot`) do wykrycia włożonego papieru/atramentu; dopiero komplet odblokowuje „Start”.
- Czystą logikę stanów/czasów wydzielić do klasy w `Core` (testowalnej w EditMode).
- Zużycie zasobów nadal przez `ResourceManager.TrySpend`, ale wyzwalane w momencie startu druku.

## Acceptance Criteria
- [ ] Gracz ładuje materiały do drukarki jako widoczne obiekty (nie sam klik), a brak materiałów
      blokuje uruchomienie z czytelnym komunikatem.
- [ ] Druk uruchamia się osobną akcją „Start” po skompletowaniu materiałów.
- [ ] Faza druku trwa konfigurowalny czas z widocznym postępem.
- [ ] Po druku następuje stygnięcie krótsze niż druk; kolejny druk zablokowany do końca stygnięcia.
- [ ] Zużyte materiały są widocznie usuwane; wyprodukowana ulotka pojawia się (obiekt/licznik).
- [ ] Parametry (czas druku, czas stygnięcia, koszty) konfigurowalne w Inspectorze z `[Tooltip]`.

## Definicja Ukończenia (DoD)
- [ ] Kod zgodny z regułami Unity 6 (`codingRules.txt`): `[SerializeField] private`, `[Tooltip]`, `OnValidate`.
- [ ] Logika stanów/czasów pokryta testem EditMode; ścieżka ładowanie→druk→stygnięcie zweryfikowana w Play Mode (PlayMode test na `PrintLeaflet`).
- [ ] Commit w konwencji Conventional Commits.
