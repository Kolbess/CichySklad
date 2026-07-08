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
        risk.SetRisk(20f);
        float before = risk.CurrentRisk;

        // Let several frames of Update-driven decay run.
        for (int i = 0; i < 10; i++)
            yield return null;

        Assert.Less(risk.CurrentRisk, before, "Risk should bleed off over time.");
        Assert.GreaterOrEqual(risk.CurrentRisk, 0f);
    }

    [Test]
    public void OnRiskDelta_BusEvent_FeedsThroughToRisk()
    {
        RiskManager risk = BuildRiskManager();

        GameEvents.RaiseRisk(12f);

        Assert.AreEqual(12f, risk.CurrentRisk, 0.001f);
    }
}
