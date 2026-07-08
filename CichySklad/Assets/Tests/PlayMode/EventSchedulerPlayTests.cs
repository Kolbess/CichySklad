using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// PlayMode coverage for the <see cref="EventScheduler"/> adapter: it fires a pool event each day
/// (reporting which via <c>OnEventScheduled</c>) and grants a resource windfall. The weighted
/// selection maths itself is proven deterministically in EditMode (EventSelectorTests).
/// </summary>
public class EventSchedulerPlayTests : PlayModeTestBase
{
    private ResourceManager _resources;

    private EventScheduler BuildScheduler()
    {
        _resources = BuildResourceManager();
        return BuildEventScheduler(_resources);
    }

    [UnityTest]
    public IEnumerator TryScheduleDailyEvent_FiresAndReportsAnEvent()
    {
        EventScheduler scheduler = BuildScheduler();
        yield return null;

        var reported = new List<GameEventId>();
        scheduler.OnEventScheduled += reported.Add;

        bool fired = scheduler.TryScheduleDailyEvent(day: 1, RiskLevel.Low);

        Assert.IsTrue(fired, "Day 1 / Low risk should always have an eligible event.");
        Assert.AreEqual(1, reported.Count, "Exactly one event should be reported per schedule.");
    }

    [UnityTest]
    public IEnumerator GrantDailyWindfall_IncreasesAResource()
    {
        EventScheduler scheduler = BuildScheduler();
        yield return null; // let ResourceManager.Start settle the starting totals

        int before =
            _resources.Money
            + _resources.Paper
            + _resources.Ink
            + _resources.Leaflets
            + _resources.Trust;

        scheduler.GrantDailyWindfall();

        int after =
            _resources.Money
            + _resources.Paper
            + _resources.Ink
            + _resources.Leaflets
            + _resources.Trust;
        Assert.Greater(after, before, "A windfall must add to some resource.");
    }

    [UnityTest]
    public IEnumerator TryScheduleDailyEvent_HighRiskEarlyDay_StillFires()
    {
        // Guards the eligibility filter: even at day 1 with elevated risk something remains rollable.
        EventScheduler scheduler = BuildScheduler();
        yield return null;

        Assert.IsTrue(scheduler.TryScheduleDailyEvent(day: 1, RiskLevel.High));
    }
}
