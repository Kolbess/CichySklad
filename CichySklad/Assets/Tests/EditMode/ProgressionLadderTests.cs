using NUnit.Framework;

/// <summary>EditMode coverage for the pure trust-threshold progression maths.</summary>
public class ProgressionLadderTests
{
    [Test]
    public void IsReached_IsInclusiveAtTheThreshold()
    {
        Assert.IsFalse(ProgressionLadder.IsReached(24, 25));
        Assert.IsTrue(ProgressionLadder.IsReached(25, 25));
        Assert.IsTrue(ProgressionLadder.IsReached(40, 25));
    }

    [Test]
    public void WasJustReached_OnlyTrueOnTheUpwardCrossing()
    {
        Assert.IsTrue(ProgressionLadder.WasJustReached(24, 25, 25), "Crossing up fires once.");
        Assert.IsTrue(ProgressionLadder.WasJustReached(0, 100, 25), "A big jump still counts.");
        Assert.IsFalse(
            ProgressionLadder.WasJustReached(25, 40, 25),
            "Already above the threshold must not re-fire."
        );
        Assert.IsFalse(ProgressionLadder.WasJustReached(10, 24, 25), "Not reached yet.");
        Assert.IsFalse(ProgressionLadder.WasJustReached(40, 10, 25), "A decrease never fires.");
    }
}
