using NUnit.Framework;

/// <summary>EditMode coverage for the pure shelf slot-occupancy bookkeeping.</summary>
public class ShelfSlotMapTests
{
    [Test]
    public void NewMap_HasCapacity_AndIsEmpty()
    {
        var map = new ShelfSlotMap(3);
        Assert.AreEqual(3, map.Capacity);
        Assert.AreEqual(0, map.OccupiedCount);
        Assert.IsTrue(map.IsEmpty);
        Assert.IsFalse(map.IsFull);
    }

    [Test]
    public void Occupy_FillsSlotsInOrder_UntilFull()
    {
        var map = new ShelfSlotMap(2);

        Assert.AreEqual(0, map.Occupy());
        Assert.AreEqual(1, map.Occupy());
        Assert.IsTrue(map.IsFull);
        Assert.AreEqual(2, map.OccupiedCount);
    }

    [Test]
    public void Occupy_WhenFull_ReturnsMinusOne()
    {
        var map = new ShelfSlotMap(1);
        map.Occupy();

        Assert.AreEqual(-1, map.Occupy(), "A full shelf must reject further parcels.");
    }

    [Test]
    public void Free_ReopensSlot_AndNextOccupyReusesTheLowestFree()
    {
        var map = new ShelfSlotMap(2);
        map.Occupy(); // 0
        map.Occupy(); // 1

        Assert.IsTrue(map.Free(0));
        Assert.IsFalse(map.IsFull);
        Assert.AreEqual(0, map.Occupy(), "The freed low slot is reused first.");
    }

    [Test]
    public void Free_OutOfRangeOrAlreadyFree_ReturnsFalse()
    {
        var map = new ShelfSlotMap(2);

        Assert.IsFalse(map.Free(0), "Slot 0 was never occupied.");
        Assert.IsFalse(map.Free(-1));
        Assert.IsFalse(map.Free(5));

        map.Occupy();
        Assert.IsTrue(map.Free(0));
        Assert.IsFalse(map.Free(0), "Double-free is rejected.");
    }

    [Test]
    public void IsOccupied_TracksSlotState()
    {
        var map = new ShelfSlotMap(2);
        int index = map.Occupy();

        Assert.IsTrue(map.IsOccupied(index));
        map.Free(index);
        Assert.IsFalse(map.IsOccupied(index));
    }
}
