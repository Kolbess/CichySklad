using NUnit.Framework;

/// <summary>PlayMode coverage for <see cref="InspectionSystem.DetectSensitiveItem"/>: contraband
/// spotted during an inspection accrues risk, and crossing the catch threshold raises the bribe
/// standoff exactly once until <see cref="InspectionSystem.EndCatching"/> resolves it.</summary>
public class InspectionSystemPlayTests : PlayModeTestBase
{
    [Test]
    public void DetectSensitiveItem_AddsItemRiskToRiskManager()
    {
        RiskManager risk = BuildRiskManager();
        InspectionSystem inspection = BuildInspection(risk, out _);

        inspection.DetectSensitiveItem(30);

        Assert.AreEqual(30f, risk.CurrentRisk, 0.001f);
        Assert.IsFalse(inspection.IsCatching);
    }

    [Test]
    public void DetectSensitiveItem_AtCatchThreshold_StartsCatchingAndRaisesBribe()
    {
        RiskManager risk = BuildRiskManager();
        InspectionSystem inspection = BuildInspection(risk, out _);
        int bribeRaised = 0;
        System.Action handler = () => bribeRaised++;
        GameEvents.OnOchranaBribe += handler;

        try
        {
            inspection.DetectSensitiveItem(50); // risk 50, below the 90 threshold
            Assert.IsFalse(inspection.IsCatching);

            inspection.DetectSensitiveItem(50); // risk clamps to 100, >= 90

            Assert.IsTrue(inspection.IsCatching);
            Assert.AreEqual(1, bribeRaised, "The bribe standoff should be raised once.");

            inspection.DetectSensitiveItem(50); // already catching: no second bribe
            Assert.AreEqual(1, bribeRaised);
        }
        finally
        {
            GameEvents.OnOchranaBribe -= handler;
        }
    }

    [Test]
    public void EndCatching_ClearsTheStandoff()
    {
        RiskManager risk = BuildRiskManager();
        InspectionSystem inspection = BuildInspection(risk, out _);
        inspection.DetectSensitiveItem(95);
        Assert.IsTrue(inspection.IsCatching);

        inspection.EndCatching();

        Assert.IsFalse(inspection.IsCatching);
    }
}
