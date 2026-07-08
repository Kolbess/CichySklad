using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

/// <summary>
/// EditMode coverage for the pure event-selection layer (A4): eligibility filtering by day / risk /
/// one-shot, weighted picking, and the guarantee that every event in the production pool is
/// reachable. Deterministic — no engine, no <c>Random</c>.
/// </summary>
public class EventSelectorTests
{
    // Weights 1 / 3 / 2 -> total 6. Cumulative bands: [0,1) [1,4) [4,6).
    private static List<GameEventDefinition> SyntheticPool() =>
        new List<GameEventDefinition>
        {
            new GameEventDefinition(GameEventId.KnockAtDoor, weight: 1),
            new GameEventDefinition(GameEventId.OutOfInk, weight: 3),
            new GameEventDefinition(GameEventId.CourierInjured, weight: 2),
        };

    // ---- Eligibility -------------------------------------------------------

    [Test]
    public void IsEligible_BeforeMinDay_IsFalse()
    {
        var def = new GameEventDefinition(GameEventId.InformerAsks, weight: 1, minDay: 3);
        Assert.IsFalse(def.IsEligible(2, RiskLevel.Low, alreadyFired: false));
        Assert.IsTrue(def.IsEligible(3, RiskLevel.Low, alreadyFired: false));
    }

    [Test]
    public void IsEligible_BelowMinRiskBand_IsFalse()
    {
        var def = new GameEventDefinition(
            GameEventId.OchranaStepsHeard,
            weight: 1,
            minRiskLevel: RiskLevel.Medium
        );
        Assert.IsFalse(def.IsEligible(1, RiskLevel.Low, alreadyFired: false));
        Assert.IsTrue(def.IsEligible(1, RiskLevel.Medium, alreadyFired: false));
        Assert.IsTrue(def.IsEligible(1, RiskLevel.Critical, alreadyFired: false));
    }

    [Test]
    public void IsEligible_OneShotAlreadyFired_IsFalse()
    {
        var def = new GameEventDefinition(GameEventId.LetterFromPanKowal, weight: 1, once: true);
        Assert.IsTrue(def.IsEligible(1, RiskLevel.Low, alreadyFired: false));
        Assert.IsFalse(def.IsEligible(1, RiskLevel.Low, alreadyFired: true));
    }

    [Test]
    public void Eligible_ExcludesFiredOneShotEvents()
    {
        var pool = new List<GameEventDefinition>
        {
            new GameEventDefinition(GameEventId.KnockAtDoor, weight: 1),
            new GameEventDefinition(GameEventId.LetterFromPanKowal, weight: 1, once: true),
        };
        var fired = new HashSet<GameEventId> { GameEventId.LetterFromPanKowal };

        List<GameEventDefinition> eligible = EventSelector.Eligible(
            pool,
            day: 5,
            RiskLevel.Low,
            fired
        );

        CollectionAssert.AreEquivalent(
            new[] { GameEventId.KnockAtDoor },
            eligible.Select(d => d.Id).ToList()
        );
    }

    // ---- Weighted picking --------------------------------------------------

    [Test]
    public void TotalWeight_SumsEligibleWeights()
    {
        Assert.AreEqual(6, EventSelector.TotalWeight(SyntheticPool()));
    }

    [Test]
    public void PickWeighted_RollLandsInCorrectCumulativeBand()
    {
        List<GameEventDefinition> pool = SyntheticPool();

        Assert.AreEqual(GameEventId.KnockAtDoor, EventSelector.PickWeighted(pool, 0).Id);
        Assert.AreEqual(GameEventId.OutOfInk, EventSelector.PickWeighted(pool, 1).Id);
        Assert.AreEqual(GameEventId.OutOfInk, EventSelector.PickWeighted(pool, 3).Id);
        Assert.AreEqual(GameEventId.CourierInjured, EventSelector.PickWeighted(pool, 4).Id);
        Assert.AreEqual(GameEventId.CourierInjured, EventSelector.PickWeighted(pool, 5).Id);
    }

    [Test]
    public void PickWeighted_SweepingEveryRoll_ReachesEveryEvent()
    {
        List<GameEventDefinition> pool = SyntheticPool();
        int total = EventSelector.TotalWeight(pool);
        var reached = new HashSet<GameEventId>();

        for (int roll = 0; roll < total; roll++)
            reached.Add(EventSelector.PickWeighted(pool, roll).Id);

        CollectionAssert.AreEquivalent(pool.Select(d => d.Id).ToList(), reached.ToList());
    }

    [Test]
    public void PickWeighted_RollAtOrPastTotal_Throws()
    {
        List<GameEventDefinition> pool = SyntheticPool();
        Assert.Throws<ArgumentOutOfRangeException>(() => EventSelector.PickWeighted(pool, 6));
        Assert.Throws<ArgumentOutOfRangeException>(() => EventSelector.PickWeighted(pool, -1));
    }

    [Test]
    public void PickWeighted_EmptyPool_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            EventSelector.PickWeighted(new List<GameEventDefinition>(), 0)
        );
    }

    // ---- Definition validation --------------------------------------------

    [Test]
    public void Definition_NonPositiveWeight_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GameEventDefinition(GameEventId.KnockAtDoor, weight: 0)
        );
    }

    [Test]
    public void Definition_MaxDayBeforeMinDay_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GameEventDefinition(GameEventId.KnockAtDoor, weight: 1, minDay: 5, maxDay: 4)
        );
    }

    [Test]
    public void Definition_MaxRiskBelowMinRisk_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new GameEventDefinition(
                GameEventId.KnockAtDoor,
                weight: 1,
                minRiskLevel: RiskLevel.High,
                maxRiskLevel: RiskLevel.Low
            )
        );
    }

    // ---- Production pool ---------------------------------------------------

    [Test]
    public void DefaultPool_ContainsEveryEventIdExactlyOnce()
    {
        List<GameEventId> ids = GameEventPool.Default().Select(d => d.Id).ToList();

        foreach (GameEventId id in Enum.GetValues(typeof(GameEventId)))
            Assert.AreEqual(1, ids.Count(x => x == id), $"{id} should appear once in the pool.");
        Assert.AreEqual(Enum.GetValues(typeof(GameEventId)).Length, ids.Count);
    }

    [Test]
    public void DefaultPool_EveryEventIsReachableGivenItsStoryPrerequisites()
    {
        // An event is reachable if some (day, risk) makes it eligible once its required story flags
        // are set (later story stages unlock only after earlier ones fire).
        foreach (GameEventDefinition def in GameEventPool.Default())
        {
            var active = new HashSet<StoryFlag>(def.RequiredFlags);
            bool reachable = false;

            for (int day = 1; day <= 14 && !reachable; day++)
                foreach (RiskLevel risk in Enum.GetValues(typeof(RiskLevel)))
                    if (def.IsEligible(day, risk, alreadyFired: false, active))
                    {
                        reachable = true;
                        break;
                    }

            Assert.IsTrue(reachable, $"{def.Id} is never eligible — unreachable event.");
        }
    }

    // ---- Story-flag gating -------------------------------------------------

    [Test]
    public void IsEligible_MissingRequiredFlag_IsFalseUntilFlagSet()
    {
        var def = new GameEventDefinition(
            GameEventId.MariaRequest,
            weight: 1,
            requiredFlags: new[] { StoryFlag.MariaStage1Done }
        );

        Assert.IsFalse(def.IsEligible(5, RiskLevel.Low, false, new HashSet<StoryFlag>()));
        Assert.IsTrue(
            def.IsEligible(
                5,
                RiskLevel.Low,
                false,
                new HashSet<StoryFlag> { StoryFlag.MariaStage1Done }
            )
        );
    }

    [Test]
    public void IsEligible_ForbiddenFlagPresent_IsFalse()
    {
        var def = new GameEventDefinition(
            GameEventId.KnockAtDoor,
            weight: 1,
            forbiddenFlags: new[] { StoryFlag.MariaHeeded }
        );

        Assert.IsTrue(def.IsEligible(1, RiskLevel.Low, false, new HashSet<StoryFlag>()));
        Assert.IsFalse(
            def.IsEligible(
                1,
                RiskLevel.Low,
                false,
                new HashSet<StoryFlag> { StoryFlag.MariaHeeded }
            )
        );
    }

    [Test]
    public void Eligible_UnlocksStoryStageTwoOnlyAfterStageOneFlag()
    {
        var pool = new List<GameEventDefinition>
        {
            new GameEventDefinition(GameEventId.MariaWarns, weight: 1),
            new GameEventDefinition(
                GameEventId.MariaRequest,
                weight: 1,
                requiredFlags: new[] { StoryFlag.MariaStage1Done }
            ),
        };
        var noEvents = new HashSet<GameEventId>();

        List<GameEventDefinition> beforeStage1 = EventSelector.Eligible(
            pool,
            5,
            RiskLevel.Low,
            noEvents,
            new HashSet<StoryFlag>()
        );
        CollectionAssert.AreEquivalent(
            new[] { GameEventId.MariaWarns },
            beforeStage1.Select(d => d.Id).ToList()
        );

        List<GameEventDefinition> afterStage1 = EventSelector.Eligible(
            pool,
            5,
            RiskLevel.Low,
            noEvents,
            new HashSet<StoryFlag> { StoryFlag.MariaStage1Done }
        );
        CollectionAssert.AreEquivalent(
            new[] { GameEventId.MariaWarns, GameEventId.MariaRequest },
            afterStage1.Select(d => d.Id).ToList()
        );
    }
}
