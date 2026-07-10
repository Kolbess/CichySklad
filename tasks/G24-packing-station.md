# G24 — Stacja pakowania (ulotki → paczka)

| | |
|---|---|
| **ID** | G24 |
| **Status** | ✅ Zrobione |
| **Blok** | G — Pętla produkcji i ryzyka |
| **Priorytet** | P2 — Ważne |
| **Szacunek** | 1–2 dni |
| **Zależności** | G23 (produkcja ulotek), istniejący `Package`, `ResourceManager` |
| **Pliki** | nowy `Assets/Scripts/PackingStation.cs`, `Assets/Scripts/Package.cs`, `Assets/Scripts/ResourceManager.cs` |

## Kontekst / problem
W scenie istnieje stacja pakowania, ale nie jest wykorzystana w rozgrywce. Potrzebny jest krok
pętli, w którym gotowe ulotki są pakowane w paczkę gotową do odbioru przez kuriera (G26).

## Zakres
**W zakresie:**
- Stacja przyjmuje ulotki do zapakowania, **pojemność 2**.
- Po włożeniu **co najmniej 1** ulotki uruchomienie pakowania kosztuje **1 monetę**.
- Pakowanie trwa **1–3 s** (konfigurowalne) z widocznym postępem.
- Po zakończeniu gotowa **paczka jest wydawana graczowi po kliknięciu** na stację.

**Poza zakresem:** przechowywanie paczek na półce (G25), odbiór przez kuriera (G26).

## Wskazówki implementacyjne
- Nowy komponent `PackingStation` w stylu `PrintLeaflet`/`HidingSpot` (`[SerializeField] [Tooltip]`,
  `Awake` asercje, `OnValidate` walidacja pojemności i czasu).
- Pola: `_capacity = 2`, `_packingCost = 1` (monety), `_minPackTime`, `_maxPackTime`.
- Wejście: przeciągnięcie ulotki (obiekt) do stacji lub pobranie z licznika `ResourceManager.Leaflets`
  — wybrać spójnie z modelem materiałów z G23.
- Koszt pobierać przez `ResourceManager.TrySpend(costMoney: _packingCost)` w momencie startu pakowania;
  brak monety → brak startu + komunikat (istniejące ostrzeżenia zasobów).
- Wynik: instancja prefabu `Package` (`Package.AddItem`) wydawana po kliknięciu (stan „Ready”).
- Logikę stanu/czasu/pojemności wydzielić do klasy w `Core` (test EditMode).

## Acceptance Criteria
- [ ] Stacja przyjmuje maksymalnie 2 ulotki (dalsze wejście zablokowane).
- [ ] Pakowanie da się uruchomić dopiero z ≥1 ulotką w środku.
- [ ] Start pakowania pobiera 1 monetę; brak monety blokuje z czytelnym komunikatem.
- [ ] Pakowanie trwa 1–3 s z widocznym postępem.
- [ ] Po zakończeniu kliknięcie na stację wydaje graczowi gotową paczkę.
- [ ] Parametry (pojemność, koszt, czas) konfigurowalne w Inspectorze z `[Tooltip]`.

## Definicja Ukończenia (DoD)
- [ ] Kod zgodny z regułami Unity 6 (`codingRules.txt`).
- [ ] Logika pokryta testem EditMode; pełny cykl (wkładanie→koszt→pakowanie→wydanie) zweryfikowany w Play Mode (PlayMode test).
- [ ] Commit w konwencji Conventional Commits.
