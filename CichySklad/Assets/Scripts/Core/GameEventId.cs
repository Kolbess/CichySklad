/// <summary>
/// Stable identifiers for every narrative event the day loop can schedule. Lives in
/// <c>Core</c> (engine-agnostic) so selection logic and its EditMode tests can reason about the
/// pool without touching <c>GameEvents</c> or any <c>MonoBehaviour</c>. The runtime
/// <c>EventScheduler</c> owns the id -> trigger mapping.
///
/// System-driven beats (OchranaBribe, OfficerInspection*, Arrest) are deliberately absent: they are
/// fired by the inspection flow, not rolled from the daily pool.
/// </summary>
public enum GameEventId
{
    // 1. Kontrole
    KnockAtDoor,
    NeighborPeeking,
    OchranaStepsHeard,
    OchranaRaid,

    // 2. Zasoby
    OutOfInk,
    LostPaperBatch,
    MoistureDamage,
    SecretDonation,

    // 3. Donosiciele / sąsiedzi
    NeighborSawCourier,
    InformerAsks,
    RumorsSpread,

    // 4. Kurier / przesyłki
    CourierInjured,
    UrgentDelivery,
    PackageUncertain,

    // 5. Sabotage / niepewne kontakty
    StuckHidingSpot,
    StrangerNeedsHelp,
    LampExplosion,

    // 6. Fabularne / cutscenki (multi-stage threads — see StoryFlag)
    MariaWarns, // Maria thread, stage 1
    MariaRequest, // Maria thread, stage 2 (branches on MariaHeeded)
    LetterFromPanKowal, // Kowal thread, stage 1
    KowalTask, // Kowal thread, stage 2 (branches on KowalAcceptedTask)
    InformerSuspicion, // Informer thread, stage 1
    InformerDisappears, // Informer thread, stage 2 (branches on InformerAppeased)

    // 7. Stresujące / natychmiastowe
    LoudNoise,
    FireCandle,
    BrokenLock,

    // 8. Ekonomiczne / łapówki
    BuyPaperOffer,
}
