using System.Collections.Generic;

/// <summary>
/// The production catalogue of schedulable narrative events with their selection tuning (weight,
/// day window, required risk band, one-shot flag). Pure data in <c>Core</c> so the pool — and its
/// reachability — is covered by EditMode tests without booting the engine.
///
/// To add an event to the daily pool: add its <see cref="GameEventId"/>, add one line here, and map
/// the id to its trigger in <c>EventScheduler</c>. <c>DayCycle</c> never changes.
/// </summary>
public static class GameEventPool
{
    /// <summary>A fresh copy of the default event pool. Callers may keep and reuse the returned list.</summary>
    public static List<GameEventDefinition> Default()
    {
        return new List<GameEventDefinition>
        {
            // 1. Kontrole — the "heat" beats. OchranaStepsHeard only once suspicion is rising.
            new GameEventDefinition(GameEventId.KnockAtDoor, weight: 8),
            new GameEventDefinition(GameEventId.NeighborPeeking, weight: 8),
            new GameEventDefinition(
                GameEventId.OchranaStepsHeard,
                weight: 5,
                minRiskLevel: RiskLevel.Medium
            ),
            // A full search — a serious, high-heat beat gated to later days and elevated risk.
            new GameEventDefinition(
                GameEventId.OchranaRaid,
                weight: 3,
                minDay: 2,
                minRiskLevel: RiskLevel.Medium
            ),
            // 2. Zasoby
            new GameEventDefinition(GameEventId.OutOfInk, weight: 6),
            new GameEventDefinition(GameEventId.LostPaperBatch, weight: 5),
            new GameEventDefinition(GameEventId.MoistureDamage, weight: 5),
            new GameEventDefinition(GameEventId.SecretDonation, weight: 4),
            // 3. Donosiciele / sąsiedzi — the network heats up after the opening day.
            new GameEventDefinition(GameEventId.NeighborSawCourier, weight: 5, minDay: 2),
            new GameEventDefinition(GameEventId.InformerAsks, weight: 5, minDay: 2),
            new GameEventDefinition(GameEventId.RumorsSpread, weight: 6),
            // 4. Kurier / przesyłki
            new GameEventDefinition(GameEventId.CourierInjured, weight: 7),
            new GameEventDefinition(GameEventId.UrgentDelivery, weight: 5),
            new GameEventDefinition(GameEventId.PackageUncertain, weight: 5),
            // 5. Sabotage / niepewne kontakty
            new GameEventDefinition(GameEventId.StuckHidingSpot, weight: 4),
            new GameEventDefinition(GameEventId.StrangerNeedsHelp, weight: 4, minDay: 2),
            new GameEventDefinition(GameEventId.LampExplosion, weight: 3),
            // 6. Fabularne — three multi-stage threads. Each stage is one-shot; stage 2 requires its
            // thread's "stage 1 done" flag, so the beats always arrive in order, in sensible day
            // windows. Weights are high so story reliably surfaces inside its window. Branch flags
            // (MariaHeeded / KowalAcceptedTask / InformerAppeased) are read inside the handlers.
            // -- Maria (łączniczka) --
            new GameEventDefinition(
                GameEventId.MariaWarns,
                weight: 20,
                minDay: 1,
                maxDay: 3,
                once: true
            ),
            new GameEventDefinition(
                GameEventId.MariaRequest,
                weight: 20,
                minDay: 3,
                maxDay: 7,
                once: true,
                requiredFlags: new[] { StoryFlag.MariaStage1Done }
            ),
            // -- Pan Kowal (szef konspiracji) --
            new GameEventDefinition(
                GameEventId.LetterFromPanKowal,
                weight: 20,
                minDay: 2,
                maxDay: 4,
                once: true
            ),
            new GameEventDefinition(
                GameEventId.KowalTask,
                weight: 20,
                minDay: 4,
                maxDay: 8,
                once: true,
                requiredFlags: new[] { StoryFlag.KowalStage1Done }
            ),
            // -- Donosiciel (informer): rising suspicion -> climax --
            new GameEventDefinition(
                GameEventId.InformerSuspicion,
                weight: 20,
                minDay: 2,
                maxDay: 4,
                once: true
            ),
            new GameEventDefinition(
                GameEventId.InformerDisappears,
                weight: 20,
                minDay: 5,
                maxDay: 11,
                once: true,
                requiredFlags: new[] { StoryFlag.InformerStage1Done }
            ),
            // 7. Stresujące / natychmiastowe
            new GameEventDefinition(GameEventId.LoudNoise, weight: 5),
            new GameEventDefinition(GameEventId.FireCandle, weight: 4),
            new GameEventDefinition(GameEventId.BrokenLock, weight: 4),
            // 8. Ekonomiczne / łapówki
            new GameEventDefinition(GameEventId.BuyPaperOffer, weight: 4),
        };
    }
}
