# G26 — Odbiór paczek przez kuriera (zapłata skalowana ryzykiem)

| | |
|---|---|
| **ID** | G26 |
| **Status** | ✅ Zrobione |
| **Blok** | G — Pętla produkcji i ryzyka |
| **Priorytet** | P2 — Ważne |
| **Szacunek** | 1–2 dni |
| **Zależności** | G24/G25 (źródło paczek), `RiskManager`, `ResourceManager` |
| **Pliki** | nowy `Assets/Scripts/Courier.cs`, `Assets/Scripts/RiskSystem/RiskManager.cs`, `Assets/Scripts/ResourceManager.cs`, `Assets/Scripts/EventHandler.cs` |

## Kontekst / problem
Pętla produkcji nie ma domknięcia po stronie „sprzedaży”: gotowe paczki nie są nigdzie odbierane.
Kurier ma zamykać pętlę — odbierać paczkę, płacić (kwota zależna od bieżącego ryzyka) i podnosić
zaufanie, bo gracz wykonuje swoją konspiracyjną robotę.

## Zakres
**W zakresie:**
- Kurier odbiera **do 1 paczki** na wizytę (z półki G25 lub bezpośrednio od gracza).
- Zapłata w monetach **skalowana bieżącym ryzykiem: od 1 do 5** (wyższe ryzyko = wyższa stawka
  za odbiór ryzykownej dostawy).
- Udana dostawa **zwiększa zaufanie** (`ResourceManager.AddTrust`).

**Poza zakresem:** kontrola/aresztowanie w trakcie odbioru (istniejący `InspectionSystem`),
balans dokładnych wartości (D15).

## Wskazówki implementacyjne
- Nowy komponent `Courier` z `[SerializeField]` referencjami do `RiskManager`/`ResourceManager`
  (bez singletonów), triggerowany zdarzeniem lub interakcją (klik/przeciągnięcie paczki na kuriera).
- Mapowanie ryzyko → zapłata (1..5) jako **czysta funkcja w `Core`** (np. rozszerzyć `RiskCalculator`
  lub nowa klasa), testowalna w EditMode; wykorzystać istniejące pasma `RiskLevel`.
- Brak paczki → kurier nic nie odbiera i komunikuje to (dialog/`DialogueSystem`).
- Rozważyć spięcie z pulą eventów (`EventScheduler`/`GameEventPool`) jako okresowa wizyta kuriera.
- Konsekwencje realne: `TrySpend`/`AddMoney` + `AddTrust`, spójnie z resztą ekonomii.

## Acceptance Criteria
- [ ] Kurier odbiera maksymalnie 1 paczkę na wizytę.
- [ ] Wypłata za paczkę skaluje się z bieżącym ryzykiem w zakresie 1–5 monet (funkcja czysta).
- [ ] Udana dostawa zwiększa zaufanie o konfigurowalną wartość.
- [ ] Brak dostępnej paczki → brak odbioru i czytelny komunikat.
- [ ] Progi/parametry (mapowanie ryzyka, przyrost zaufania) konfigurowalne w Inspectorze z `[Tooltip]`.

## Definicja Ukończenia (DoD)
- [ ] Kod zgodny z regułami Unity 6 (`codingRules.txt`).
- [ ] Mapowanie ryzyko→zapłata pokryte testem EditMode; odbiór+wypłata+zaufanie zweryfikowane w Play Mode (PlayMode test).
- [ ] Commit w konwencji Conventional Commits.
