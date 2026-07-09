using System.Collections.Generic;
using NUnit.Framework;

/// <summary>
/// EditMode coverage for the pure printer state machine + timing:
/// <c>Idle → Loaded → Printing → CoolingDown → Idle</c>, material gating, single leaflet emission,
/// and the phase progress fractions.
/// </summary>
public class PrinterCycleTests
{
    private static PrinterCycle Loaded(float print = 4f, float cooldown = 1f)
    {
        var cycle = new PrinterCycle(print, cooldown);
        cycle.SetPaperLoaded(true);
        cycle.SetInkLoaded(true);
        return cycle;
    }

    [Test]
    public void NewCycle_StartsIdle_AndCannotStart()
    {
        var cycle = new PrinterCycle(4f, 1f);
        Assert.AreEqual(PrinterState.Idle, cycle.State);
        Assert.IsFalse(cycle.CanStart);
    }

    [Test]
    public void OneMaterial_IsNotEnough_BothTransitionsToLoaded()
    {
        var cycle = new PrinterCycle(4f, 1f);

        cycle.SetPaperLoaded(true);
        Assert.AreEqual(PrinterState.Idle, cycle.State, "Paper alone must not arm the station.");

        cycle.SetInkLoaded(true);
        Assert.AreEqual(PrinterState.Loaded, cycle.State);
        Assert.IsTrue(cycle.CanStart);
    }

    [Test]
    public void RemovingMaterial_WhileLoaded_FallsBackToIdle()
    {
        var cycle = Loaded();

        cycle.SetPaperLoaded(false);

        Assert.AreEqual(PrinterState.Idle, cycle.State);
        Assert.IsFalse(cycle.CanStart);
    }

    [Test]
    public void TryStartPrinting_WhenNotLoaded_ReturnsFalse()
    {
        var cycle = new PrinterCycle(4f, 1f);

        Assert.IsFalse(cycle.TryStartPrinting());
        Assert.AreEqual(PrinterState.Idle, cycle.State);
    }

    [Test]
    public void TryStartPrinting_WhenLoaded_ConsumesMaterialsAndPrints()
    {
        var cycle = Loaded();

        Assert.IsTrue(cycle.TryStartPrinting());
        Assert.AreEqual(PrinterState.Printing, cycle.State);
        Assert.IsFalse(cycle.HasPaper, "Materials are consumed the moment printing starts.");
        Assert.IsFalse(cycle.HasInk);
    }

    [Test]
    public void Printing_CompletesAfterPrintDuration_FiresFinishedOnce_ThenCoolsDown()
    {
        var cycle = Loaded(print: 2f, cooldown: 1f);
        cycle.TryStartPrinting();
        int finished = 0;
        cycle.OnPrintFinished += () => finished++;

        cycle.Tick(1f);
        Assert.AreEqual(PrinterState.Printing, cycle.State);
        Assert.AreEqual(0, finished);
        Assert.AreEqual(0.5f, cycle.PrintProgress, 1e-4f);

        cycle.Tick(1f); // reaches the 2s print duration
        Assert.AreEqual(1, finished, "The leaflet must be produced exactly once.");
        Assert.AreEqual(PrinterState.CoolingDown, cycle.State);
        Assert.AreEqual(0f, cycle.PrintProgress, "Print progress is 0 outside the print phase.");
    }

    [Test]
    public void Cooldown_CompletesAfterCooldownDuration_ReturnsToIdle()
    {
        var cycle = Loaded(print: 2f, cooldown: 1f);
        cycle.TryStartPrinting();
        cycle.Tick(2f); // finish printing → CoolingDown
        Assert.AreEqual(PrinterState.CoolingDown, cycle.State);

        cycle.Tick(0.5f);
        Assert.AreEqual(PrinterState.CoolingDown, cycle.State);
        Assert.AreEqual(0.5f, cycle.CooldownProgress, 1e-4f);

        cycle.Tick(0.5f); // reaches the 1s cooldown duration
        Assert.AreEqual(PrinterState.Idle, cycle.State);
    }

    [Test]
    public void LoadingMaterial_DuringPrinting_IsIgnored()
    {
        var cycle = Loaded(print: 5f, cooldown: 1f);
        cycle.TryStartPrinting();

        cycle.SetPaperLoaded(true);
        cycle.SetInkLoaded(true);

        Assert.AreEqual(PrinterState.Printing, cycle.State, "A live job must not be re-gated.");
    }

    [Test]
    public void StateChanges_AreReportedInOrder()
    {
        var cycle = Loaded(print: 1f, cooldown: 1f);
        var states = new List<PrinterState>();
        cycle.OnStateChanged += s => states.Add(s);

        cycle.TryStartPrinting(); // → Printing
        cycle.Tick(1f); // → CoolingDown
        cycle.Tick(1f); // → Idle

        Assert.AreEqual(
            new[] { PrinterState.Printing, PrinterState.CoolingDown, PrinterState.Idle },
            states
        );
    }
}
