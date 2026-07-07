using NUnit.Framework;
using UnityEngine;

/// <summary>EditMode coverage for the NPC cardinal-facing maths.</summary>
public class InspectionPathTests
{
    [Test]
    public void CardinalFacing_ZeroDelta_ReturnsZero()
    {
        Assert.AreEqual(Vector2.zero, InspectionPath.CardinalFacing(Vector2.zero));
    }

    [Test]
    public void CardinalFacing_DominantHorizontal_SnapsToXAxis()
    {
        Vector2 facing = InspectionPath.CardinalFacing(new Vector2(3f, 1f));
        Assert.AreEqual(new Vector2(1f, 0f), facing);

        facing = InspectionPath.CardinalFacing(new Vector2(-3f, 1f));
        Assert.AreEqual(new Vector2(-1f, 0f), facing);
    }

    [Test]
    public void CardinalFacing_DominantVertical_SnapsToYAxis()
    {
        Vector2 facing = InspectionPath.CardinalFacing(new Vector2(1f, 3f));
        Assert.AreEqual(new Vector2(0f, 1f), facing);

        facing = InspectionPath.CardinalFacing(new Vector2(1f, -3f));
        Assert.AreEqual(new Vector2(0f, -1f), facing);
    }
}
