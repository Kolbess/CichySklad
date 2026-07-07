using NUnit.Framework;

/// <summary>EditMode coverage for the pure risk banding and decay maths.</summary>
public class RiskCalculatorTests
{
    private const float Medium = 30f;
    private const float High = 70f;
    private const float Critical = 90f;

    [Test]
    public void DetermineLevel_BelowMedium_IsLow()
    {
        Assert.AreEqual(RiskLevel.Low, RiskCalculator.DetermineLevel(0f, Medium, High, Critical));
        Assert.AreEqual(
            RiskLevel.Low,
            RiskCalculator.DetermineLevel(29.9f, Medium, High, Critical)
        );
    }

    [Test]
    public void DetermineLevel_ThresholdsAreInclusiveLowerBounds()
    {
        Assert.AreEqual(
            RiskLevel.Medium,
            RiskCalculator.DetermineLevel(Medium, Medium, High, Critical)
        );
        Assert.AreEqual(
            RiskLevel.High,
            RiskCalculator.DetermineLevel(High, Medium, High, Critical)
        );
        Assert.AreEqual(
            RiskLevel.Critical,
            RiskCalculator.DetermineLevel(Critical, Medium, High, Critical)
        );
    }

    [Test]
    public void DetermineLevel_AtOrAboveCritical_IsCritical()
    {
        Assert.AreEqual(
            RiskLevel.Critical,
            RiskCalculator.DetermineLevel(100f, Medium, High, Critical)
        );
    }

    [Test]
    public void DecayMultiplier_ReturnsBandSpecificMultiplier()
    {
        Assert.AreEqual(
            1.0f,
            RiskCalculator.DecayMultiplier(RiskLevel.Low, 1.0f, 0.75f, 0.5f, 0.25f)
        );
        Assert.AreEqual(
            0.75f,
            RiskCalculator.DecayMultiplier(RiskLevel.Medium, 1.0f, 0.75f, 0.5f, 0.25f)
        );
        Assert.AreEqual(
            0.5f,
            RiskCalculator.DecayMultiplier(RiskLevel.High, 1.0f, 0.75f, 0.5f, 0.25f)
        );
        Assert.AreEqual(
            0.25f,
            RiskCalculator.DecayMultiplier(RiskLevel.Critical, 1.0f, 0.75f, 0.5f, 0.25f)
        );
    }
}
