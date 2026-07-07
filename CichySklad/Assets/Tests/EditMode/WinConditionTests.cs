using NUnit.Framework;

/// <summary>EditMode coverage for the pure win-condition check (inclusive day and trust thresholds).</summary>
public class WinConditionTests
{
    private const int WinDay = 7;
    private const int TrustThreshold = 50;

    [Test]
    public void IsMet_BeforeWinDay_IsFalseEvenWithEnoughTrust()
    {
        Assert.IsFalse(WinCondition.IsMet(6, WinDay, 100, TrustThreshold));
    }

    [Test]
    public void IsMet_OnWinDayWithTooLittleTrust_IsFalse()
    {
        Assert.IsFalse(WinCondition.IsMet(WinDay, WinDay, TrustThreshold - 1, TrustThreshold));
    }

    [Test]
    public void IsMet_OnWinDayAtExactThreshold_IsTrue()
    {
        // Threshold is an inclusive lower bound, not an equality — reaching it exactly wins.
        Assert.IsTrue(WinCondition.IsMet(WinDay, WinDay, TrustThreshold, TrustThreshold));
    }

    [Test]
    public void IsMet_PastWinDayWithExcessTrust_IsTrue()
    {
        // Neither comparison is an equality check: overshooting the day or the trust still wins.
        Assert.IsTrue(WinCondition.IsMet(WinDay + 2, WinDay, 100, TrustThreshold));
    }
}
