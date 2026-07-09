using System.Collections.Generic;
using NUnit.Framework;

/// <summary>
/// EditMode coverage for the pure packing state machine + timing:
/// <c>Idle → Filling → Packing → Ready → Idle</c>, the capacity cap, single package emission, and
/// the packed-count hand-off on dispense.
/// </summary>
public class PackingCycleTests
{
    private static PackingCycle Filled(int leaflets, int capacity = 2)
    {
        var cycle = new PackingCycle(capacity);
        for (int i = 0; i < leaflets; i++)
            cycle.TryAddLeaflet();
        return cycle;
    }

    [Test]
    public void NewCycle_StartsIdle_Empty_CannotStart()
    {
        var cycle = new PackingCycle(2);
        Assert.AreEqual(PackingState.Idle, cycle.State);
        Assert.AreEqual(0, cycle.Count);
        Assert.IsFalse(cycle.CanStart);
    }

    [Test]
    public void TryAddLeaflet_FillsUpToCapacity_ThenBlocks()
    {
        var cycle = new PackingCycle(2);

        Assert.IsTrue(cycle.TryAddLeaflet());
        Assert.AreEqual(PackingState.Filling, cycle.State);
        Assert.IsTrue(cycle.TryAddLeaflet());
        Assert.IsTrue(cycle.IsFull);

        Assert.IsFalse(cycle.TryAddLeaflet(), "A third leaflet must be rejected at capacity 2.");
        Assert.AreEqual(2, cycle.Count);
    }

    [Test]
    public void CanStart_OnlyWithAtLeastOneLeaflet()
    {
        var empty = new PackingCycle(2);
        Assert.IsFalse(empty.CanStart);

        var one = Filled(1);
        Assert.IsTrue(one.CanStart);
    }

    [Test]
    public void TryStartPacking_WhenEmpty_ReturnsFalse()
    {
        var cycle = new PackingCycle(2);
        Assert.IsFalse(cycle.TryStartPacking(2f));
        Assert.AreEqual(PackingState.Idle, cycle.State);
    }

    [Test]
    public void TryStartPacking_CapturesLoadedLeaflets_AndClearsTheLoad()
    {
        var cycle = Filled(2);

        Assert.IsTrue(cycle.TryStartPacking(2f));
        Assert.AreEqual(PackingState.Packing, cycle.State);
        Assert.AreEqual(0, cycle.Count, "Loaded leaflets move into the package.");
        Assert.AreEqual(2, cycle.PackedCount);
    }

    [Test]
    public void Packing_CompletesAfterDuration_FiresFinishedOnce_ThenReady()
    {
        var cycle = Filled(1);
        cycle.TryStartPacking(2f);
        int finished = 0;
        cycle.OnPackFinished += () => finished++;

        cycle.Tick(1f);
        Assert.AreEqual(PackingState.Packing, cycle.State);
        Assert.AreEqual(0, finished);
        Assert.AreEqual(0.5f, cycle.PackProgress, 1e-4f);

        cycle.Tick(1f); // reaches the 2s duration
        Assert.AreEqual(1, finished, "The package is finished exactly once.");
        Assert.AreEqual(PackingState.Ready, cycle.State);
    }

    [Test]
    public void Dispense_WhenReady_ReturnsPackedCount_AndResetsToIdle()
    {
        var cycle = Filled(2);
        cycle.TryStartPacking(1f);
        cycle.Tick(1f);
        Assert.AreEqual(PackingState.Ready, cycle.State);

        int packed = cycle.Dispense();

        Assert.AreEqual(2, packed);
        Assert.AreEqual(PackingState.Idle, cycle.State);
        Assert.AreEqual(0, cycle.PackedCount);
        Assert.AreEqual(0, cycle.Count);
    }

    [Test]
    public void Dispense_WhenNotReady_ReturnsZero()
    {
        var cycle = Filled(1);
        Assert.AreEqual(0, cycle.Dispense(), "Nothing to dispense while still filling.");
        Assert.AreEqual(PackingState.Filling, cycle.State);
    }

    [Test]
    public void AddingLeaflet_DuringPacking_IsRejected()
    {
        var cycle = Filled(1);
        cycle.TryStartPacking(5f);

        Assert.IsFalse(cycle.TryAddLeaflet(), "A live job must not accept more leaflets.");
        Assert.AreEqual(PackingState.Packing, cycle.State);
    }

    [Test]
    public void StateChanges_AreReportedInOrder()
    {
        var cycle = Filled(1);
        var states = new List<PackingState>();
        cycle.OnStateChanged += s => states.Add(s);

        cycle.TryStartPacking(1f); // → Packing
        cycle.Tick(1f); // → Ready
        cycle.Dispense(); // → Idle

        Assert.AreEqual(
            new[] { PackingState.Packing, PackingState.Ready, PackingState.Idle },
            states
        );
    }
}
