using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>PlayMode coverage for the runtime risk value: mutation, clamping, band/value events,
/// per-frame decay, and the static <see cref="GameEvents.OnRiskDelta"/> wiring.</summary>
public class RiskManagerPlayTests : PlayModeTestBase
{
    [Test]
    public void AddRisk_RaisesValue_AndFiresChangeEvent()
    {
        RiskManager risk = BuildRiskManager();
        float reported = -1f;
        risk.OnRiskChanged += value => reported = value;

        risk.AddRisk(25f);

        Assert.AreEqual(25f, risk.CurrentRisk, 0.001f);
        Assert.AreEqual(25f, reported, 0.001f);
    }

    [Test]
    public void SetRisk_ClampsToMaxRisk()
    {
        RiskManager risk = BuildRiskManager();

        risk.SetRisk(500f);

        Assert.AreEqual(risk.MaxRisk, risk.CurrentRisk, 0.001f);
    }

    [Test]
    public void ReduceRisk_ClampsAtZero()
    {
        RiskManager risk = BuildRiskManager();
        risk.SetRisk(10f);

        risk.ReduceRisk(50f);

        Assert.AreEqual(0f, risk.CurrentRisk, 0.001f);
    }

    [Test]
    public void CrossingThreshold_FiresLevelChangedOnce_WithNewBand()
    {
        RiskManager risk = BuildRiskManager();
        int levelChanges = 0;
        RiskLevel lastLevel = RiskLevel.Low;
        risk.OnRiskLevelChanged += level =>
        {
            levelChanges++;
            lastLevel = level;
        };

        risk.SetRisk(75f); // default High threshold is 70

        Assert.AreEqual(RiskLevel.High, risk.CurrentRiskLevel);
        Assert.AreEqual(RiskLevel.High, lastLevel);
        Assert.AreEqual(
            1,
            levelChanges,
            "Level-changed should fire exactly once per band crossing."
        );
    }

    [Test]
    public void StayingInSameBand_DoesNotRefireLevelChanged()
    {
        RiskManager risk = BuildRiskManager();
        risk.SetRisk(35f); // Medium
        int changesAfterFirst = 0;
        risk.OnRiskLevelChanged += _ => changesAfterFirst++;

        risk.SetRisk(40f); // still Medium

        Assert.AreEqual(RiskLevel.Medium, risk.CurrentRiskLevel);
        Assert.AreEqual(0, changesAfterFirst);
    }

    [UnityTest]
    public IEnumerator Risk_DecaysTowardZeroOverFrames()
    {
        RiskManager risk = BuildRiskManager();
        SetField(risk, "_decayFreezeSeconds", 0f); // isolate decay from the freeze hold
        risk.SetRisk(20f);
        float before = risk.CurrentRisk;

        // Let several frames of Update-driven decay run.
        for (int i = 0; i < 10; i++)
            yield return null;

        Assert.Less(risk.CurrentRisk, before, "Risk should bleed off over time.");
        Assert.GreaterOrEqual(risk.CurrentRisk, 0f);
    }

    [UnityTest]
    public IEnumerator Risk_HoldsDuringFreeze_ThenDecays()
    {
        RiskManager risk = BuildFrozenRiskManager(freeze: 0.5f);
        risk.SetRisk(20f); // starts the hold

        yield return Advance(0.2f); // still inside the freeze
        Assert.AreEqual(20f, risk.CurrentRisk, 0.001f, "Risk holds during the freeze.");

        yield return Advance(0.6f); // past the freeze — decay resumes
        Assert.Less(risk.CurrentRisk, 20f, "Risk bleeds off once the freeze elapses.");
    }

    [UnityTest]
    public IEnumerator Increase_DuringFreeze_RefreshesTheHold()
    {
        RiskManager risk = BuildFrozenRiskManager(freeze: 0.5f);
        risk.SetRisk(20f);

        yield return Advance(0.35f); // partway through the first hold
        risk.AddRisk(10f); // a fresh increase → the hold restarts from here

        yield return Advance(0.35f); // < 0.5s since the refresh, so still held
        Assert.AreEqual(30f, risk.CurrentRisk, 0.001f, "A new increase refreshes the freeze.");

        yield return Advance(0.4f); // now past the refreshed hold
        Assert.Less(risk.CurrentRisk, 30f, "Decay resumes after the refreshed freeze.");
    }

    [Test]
    public void ExplicitReduce_IgnoresTheFreeze()
    {
        RiskManager risk = BuildFrozenRiskManager(freeze: 10f);
        risk.SetRisk(20f); // frozen for 10s

        risk.ReduceRisk(5f); // an explicit player-choice reduction

        Assert.AreEqual(
            15f,
            risk.CurrentRisk,
            0.001f,
            "Explicit reductions apply immediately, regardless of the freeze."
        );
    }

    private RiskManager BuildFrozenRiskManager(float freeze)
    {
        RiskManager risk = AddInactive<RiskManager>(out _, "RiskManager");
        SetField(risk, "_decayFreezeSeconds", freeze);
        Activate(risk);
        return risk;
    }

    private static IEnumerator Advance(float seconds)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    [Test]
    public void OnRiskDelta_BusEvent_FeedsThroughToRisk()
    {
        RiskManager risk = BuildRiskManager();

        GameEvents.RaiseRisk(12f);

        Assert.AreEqual(12f, risk.CurrentRisk, 0.001f);
    }
}
