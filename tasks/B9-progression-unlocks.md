# B9 — Progresja i odblokowania narzędzi konspiracji

| | |
|---|---|
| **ID** | B9 |
| **Blok** | B — Treść |
| **Priorytet** | P2 |
| **Szacunek** | 2 dni |
| **Zależności** | A1 (zaufanie), A4, B7 |
| **Pliki** | `Assets/Scripts/ResourceManager.cs`, nowy np. `Assets/Scripts/ProgressionSystem.cs` |

## Kontekst / problem
Konspekt („Nagrody i motywacja gracza”): odblokowywanie nowych narzędzi konspiracji i rosnące
zaufanie organizacji. Obecnie zaufanie jest tylko liczbą na suwaku bez konsekwencji progresji.

## Zakres
**W zakresie:** system progresji spinający zaufanie z odblokowaniami (nowe skrytki/narzędzia/
opcje eventów) i rosnącymi możliwościami.
**Poza zakresem:** konkretne assety nowych narzędzi (mogą być zależne od E17/D14).

## Wskazówki implementacyjne
- Progi zaufania → odblokowania (np. lepsza skrytka, tańszy druk, nowa opcja dialogowa).
- Prosty model: `List<Unlock>` z progiem i efektem; UI komunikujące zdobycie.
- Spójność z warunkiem zwycięstwa (A1) — progresja powinna prowadzić w stronę celu.

## Acceptance Criteria
- [ ] Istnieją co najmniej 3 odblokowania powiązane z progami zaufania/postępem.
- [ ] Odblokowanie ma odczuwalny wpływ na rozgrywkę (nie tylko kosmetyka).
- [ ] Gracz otrzymuje czytelny komunikat o zdobyciu odblokowania.
- [ ] Progresja jest spójna z warunkiem zwycięstwa (A1).

## Definicja Ukończenia (DoD)
- [ ] Zweryfikowane w Play Mode: zdobycie i użycie co najmniej jednego odblokowania.
- [ ] Commit w konwencji Conventional Commits.
