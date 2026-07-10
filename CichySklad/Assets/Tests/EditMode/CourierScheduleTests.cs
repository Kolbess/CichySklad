using NUnit.Framework;

/// <summary>EditMode coverage for the pure courier visit cadence.</summary>
public class CourierScheduleTests
{
    [Test]
    public void ClampGap_KeepsRollWithinRange()
    {
        var schedule = new CourierSchedule(2, 4);

        Assert.AreEqual(2, schedule.ClampGap(1), "Below-min rolls clamp up to the minimum.");
        Assert.AreEqual(3, schedule.ClampGap(3));
        Assert.AreEqual(4, schedule.ClampGap(9), "Above-max rolls clamp down to the maximum.");
    }

    [Test]
    public void ScheduleAfter_SetsNextVisit_ToDayPlusClampedGap()
    {
        var schedule = new CourierSchedule(2, 4);

        schedule.ScheduleAfter(5, 3);
        Assert.AreEqual(8, schedule.NextVisitDay);

        schedule.ScheduleAfter(10, 99); // clamped to max gap 4
        Assert.AreEqual(14, schedule.NextVisitDay);
    }

    [Test]
    public void IsVisitDay_TrueOnceTheScheduledDayIsReached()
    {
        var schedule = new CourierSchedule(2, 4);
        schedule.ScheduleAfter(0, 3); // next visit day 3

        Assert.IsFalse(schedule.IsVisitDay(2));
        Assert.IsTrue(schedule.IsVisitDay(3));
        Assert.IsTrue(schedule.IsVisitDay(4), "A missed day still counts as due.");
    }

    [Test]
    public void Gap_IsGuaranteedWithinMax_NeverPureChance()
    {
        var schedule = new CourierSchedule(2, 4);

        // Whatever the roll, the next visit is at most maxGap days away.
        schedule.ScheduleAfter(0, int.MaxValue);
        Assert.LessOrEqual(schedule.NextVisitDay, schedule.MaxGap);

        schedule.ScheduleAfter(0, int.MinValue);
        Assert.GreaterOrEqual(schedule.NextVisitDay, schedule.MinGap);
    }

    [Test]
    public void Constructor_ClampsNonsenseBounds()
    {
        var schedule = new CourierSchedule(0, -5);

        Assert.AreEqual(1, schedule.MinGap, "Min gap floors at 1.");
        Assert.AreEqual(1, schedule.MaxGap, "Max gap cannot be below min gap.");
    }
}
