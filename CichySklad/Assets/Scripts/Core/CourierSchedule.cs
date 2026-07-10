/// <summary>
/// Pure, engine-agnostic cadence for the courier's visits: it appears in bounded random gaps of
/// <c>minGap</c>..<c>maxGap</c> days, so a visit is guaranteed within <c>maxGap</c> days and is never
/// left entirely to chance. No <c>MonoBehaviour</c>, so it is unit-testable in EditMode. The runtime
/// <c>Courier</c> rolls the gap (engine randomness) and feeds it in; this class only decides the next
/// visit day and whether a given day has reached it.
/// </summary>
public class CourierSchedule
{
    private readonly int _minGap;
    private readonly int _maxGap;
    private int _nextVisitDay;

    public CourierSchedule(int minGap, int maxGap)
    {
        _minGap = minGap < 1 ? 1 : minGap;
        _maxGap = maxGap < _minGap ? _minGap : maxGap;
    }

    public int MinGap => _minGap;
    public int MaxGap => _maxGap;
    public int NextVisitDay => _nextVisitDay;

    /// <summary>Clamps a raw gap roll into the configured [minGap, maxGap] range.</summary>
    public int ClampGap(int gap)
    {
        if (gap < _minGap)
            return _minGap;
        return gap > _maxGap ? _maxGap : gap;
    }

    /// <summary>Schedules the next visit for <paramref name="day"/> plus a clamped gap.</summary>
    public void ScheduleAfter(int day, int gap)
    {
        _nextVisitDay = day + ClampGap(gap);
    }

    /// <summary>True once <paramref name="day"/> has reached (or passed) the scheduled visit day.</summary>
    public bool IsVisitDay(int day) => day >= _nextVisitDay;
}
