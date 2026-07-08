/// <summary>
/// Persistent per-run story progress markers. Each multi-stage thread sets a "stage done" flag so
/// its next stage becomes eligible, plus a branch flag recording the player's earlier decision so
/// the later stage can diverge. Pure enum in <c>Core</c> so pool eligibility (and its EditMode
/// tests) can reason about story gating without the engine. The runtime <c>StoryState</c> holds the
/// active set; <c>EventHandler</c> sets flags from choices, <c>EventScheduler</c> reads them to gate.
/// </summary>
public enum StoryFlag
{
    // Maria (łączniczka)
    MariaStage1Done,
    MariaHeeded, // branch: player trusted Maria's warning

    // Pan Kowal (szef konspiracji)
    KowalStage1Done,
    KowalAcceptedTask, // branch: player took the dangerous job

    // Donosiciel (informer)
    InformerStage1Done,
    InformerAppeased, // branch: player bought the informer's silence
}
