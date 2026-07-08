using System;
using NUnit.Framework;

/// <summary>
/// EditMode coverage for the pure roll selector behind <c>DayCycle</c>'s daily events. Proves the
/// property that matters for the A2 bug: sweeping the roll across <c>[0, RollBound)</c> reaches every
/// option exactly once, and stepping one past the bound throws instead of silently missing a case.
/// </summary>
public class RandomEventTableTests
{
    private static readonly string[] ThreeOptions = { "a", "b", "c" };

    [Test]
    public void RollBound_EqualsOptionCount()
    {
        Assert.AreEqual(ThreeOptions.Length, RandomEventTable.RollBound(ThreeOptions));
    }

    [Test]
    public void Select_SweepingRollAcrossBound_ReachesEveryOption()
    {
        int bound = RandomEventTable.RollBound(ThreeOptions);
        var reached = new bool[bound];

        for (int roll = 0; roll < bound; roll++)
        {
            string picked = RandomEventTable.Select(ThreeOptions, roll);
            int index = Array.IndexOf(ThreeOptions, picked);
            reached[index] = true;
        }

        foreach (bool wasReached in reached)
            Assert.IsTrue(wasReached, "Every option must be reachable by some in-range roll.");
    }

    [Test]
    public void Select_TopValidRoll_ReturnsLastOption()
    {
        // The exact off-by-one the old Random.Range(1, 3) literal stranded: the final case.
        string picked = RandomEventTable.Select(ThreeOptions, ThreeOptions.Length - 1);
        Assert.AreEqual("c", picked);
    }

    [Test]
    public void Select_RollAtBound_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RandomEventTable.Select(ThreeOptions, ThreeOptions.Length)
        );
    }

    [Test]
    public void Select_NegativeRoll_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RandomEventTable.Select(ThreeOptions, -1));
    }

    [Test]
    public void RollBound_EmptyTable_Throws()
    {
        Assert.Throws<ArgumentException>(() => RandomEventTable.RollBound(Array.Empty<string>()));
    }

    [Test]
    public void RollBound_NullTable_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => RandomEventTable.RollBound<string>(null));
    }
}
