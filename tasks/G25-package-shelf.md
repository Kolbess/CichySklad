# G25 — Półka na paczki (magazynowanie gotowych paczek)

| | |
|---|---|
| **ID** | G25 |
| **Status** | ✅ Zrobione |
| **Blok** | G — Pętla produkcji i ryzyka |
| **Priorytet** | P2 — Ważne |
| **Szacunek** | 1 dzień |
| **Zależności** | G24 (źródło paczek), istniejący `Package`, wzorzec `HidingSpot` |
| **Pliki** | nowy `Assets/Scripts/PackageShelf.cs`, `Assets/Scripts/Package.cs` |

## Kontekst / problem
Gotowe paczki (z G24) nie mają miejsca składowania — leżą luzem w scenie, co jest nieczytelne i
bałaganiarskie. Potrzebna jest półka porządkująca paczki w wyznaczonych slotach do czasu odbioru
przez kuriera (G26).

## Zakres
**W zakresie:**
- Półka przechowuje paczki w slotach (pojemność konfigurowalna).
- Gracz odkłada paczkę na półkę i zabiera ją z powrotem (przeciąganie/klik).
- Paczki na półce są ułożone na slotach, nie leżą w losowych miejscach sceny.

**Poza zakresem:** odbiór/zapłata kuriera (G26), pakowanie (G24).

## Wskazówki implementacyjne
- Wzorować się na `HidingSpot` (lista przechowywanych obiektów, sloty na content, triggery 2D,
  `RemoveObject` przy zniszczeniu obiektu).
- Pola: `_capacity`, referencje do slotów/kontenera (`[SerializeField] [Tooltip]`, `Awake` asercje).
- Paczka odłożona na półkę „przyczepia się” do slotu (pozycja slotu), a pobranie zwalnia slot.
- Zabezpieczyć przed przepełnieniem: przy pełnej półce nie przyjmować kolejnych paczek.
- Powiadamiać ewentualnego konsumenta (kurier z G26) o dostępnych paczkach — najlepiej przez
  `System.Action`/zdarzenie, bez twardych referencji.

## Acceptance Criteria
- [ ] Półka przechowuje paczki do zdefiniowanej pojemności; przepełnienie jest zablokowane.
- [ ] Gracz może odłożyć paczkę na wolny slot i zabrać ją z powrotem.
- [ ] Paczki na półce zajmują sloty (uporządkowane), nie leżą losowo w scenie.
- [ ] Zniszczenie/odebranie paczki zwalnia slot (brak „martwych” referencji).
- [ ] Pojemność i sloty konfigurowalne w Inspectorze z `[Tooltip]`.

## Definicja Ukończenia (DoD)
- [ ] Kod zgodny z regułami Unity 6 (`codingRules.txt`).
- [ ] Odkładanie/pobieranie i limit pojemności zweryfikowane w Play Mode (PlayMode test).
- [ ] Commit w konwencji Conventional Commits.
