using System;

/// <summary>
/// Pure, immutable metadata describing when one <see cref="GameEventId"/> may be rolled from the
/// daily pool: its selection weight, the day window it is active in, the risk band it requires, and
/// whether it is a one-shot story beat. No engine dependency, so the whole pool and its selection
/// are unit-testable in EditMode. The runtime <c>EventScheduler</c> pairs each definition with the
/// concrete trigger that raises the matching <c>GameEvents</c> beat.
/// </summary>
public sealed class GameEventDefinition
{
    /// <summary>Which event this describes.</summary>
    public GameEventId Id { get; }

    /// <summary>Relative selection weight among eligible events. Must be &gt; 0.</summary>
    public int Weight { get; }

    /// <summary>Earliest day (inclusive, 1-based) the event may fire.</summary>
    public int MinDay { get; }

    /// <summary>Latest day (inclusive) the event may fire. <see cref="int.MaxValue"/> = no cap.</summary>
    public int MaxDay { get; }

    /// <summary>Lowest risk band (inclusive) at which the event is eligible.</summary>
    public RiskLevel MinRiskLevel { get; }

    /// <summary>Highest risk band (inclusive) at which the event is eligible.</summary>
    public RiskLevel MaxRiskLevel { get; }

    /// <summary>When true, the event may fire at most once per run (story / cutscene beats).</summary>
    public bool Once { get; }

    public GameEventDefinition(
        GameEventId id,
        int weight,
        int minDay = 1,
        int maxDay = int.MaxValue,
        RiskLevel minRiskLevel = RiskLevel.Low,
        RiskLevel maxRiskLevel = RiskLevel.Critical,
        bool once = false
    )
    {
        if (weight <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(weight),
                weight,
                "Event weight must be positive so the event stays reachable."
            );
        if (minDay < 1)
            throw new ArgumentOutOfRangeException(nameof(minDay), minDay, "MinDay is 1-based.");
        if (maxDay < minDay)
            throw new ArgumentOutOfRangeException(
                nameof(maxDay),
                maxDay,
                "MaxDay must be >= MinDay."
            );
        if (maxRiskLevel < minRiskLevel)
            throw new ArgumentException(
                "MaxRiskLevel must be >= MinRiskLevel.",
                nameof(maxRiskLevel)
            );

        Id = id;
        Weight = weight;
        MinDay = minDay;
        MaxDay = maxDay;
        MinRiskLevel = minRiskLevel;
        MaxRiskLevel = maxRiskLevel;
        Once = once;
    }

    /// <summary>
    /// True when this event may be rolled on <paramref name="day"/> at <paramref name="riskLevel"/>,
    /// given whether it has <paramref name="alreadyFired"/> (only meaningful for <see cref="Once"/>).
    /// </summary>
    public bool IsEligible(int day, RiskLevel riskLevel, bool alreadyFired)
    {
        if (Once && alreadyFired)
            return false;
        if (day < MinDay || day > MaxDay)
            return false;
        if (riskLevel < MinRiskLevel || riskLevel > MaxRiskLevel)
            return false;
        return true;
    }
}
