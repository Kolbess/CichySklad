/// <summary>
/// Pure, engine-agnostic risk maths. No <c>MonoBehaviour</c>, no scene state — so it is
/// trivially unit-testable in EditMode. The runtime <c>RiskManager</c> owns the serialized
/// configuration and delegates the actual band/decay decisions here.
/// </summary>
public static class RiskCalculator
{
    /// <summary>
    /// Maps a continuous risk value onto its <see cref="RiskLevel"/> band using the supplied
    /// thresholds. Thresholds are inclusive lower bounds and are expected to satisfy
    /// medium &lt;= high &lt;= critical.
    /// </summary>
    public static RiskLevel DetermineLevel(
        float risk,
        float mediumThreshold,
        float highThreshold,
        float criticalThreshold
    )
    {
        if (risk >= criticalThreshold)
            return RiskLevel.Critical;
        if (risk >= highThreshold)
            return RiskLevel.High;
        if (risk >= mediumThreshold)
            return RiskLevel.Medium;
        return RiskLevel.Low;
    }

    /// <summary>
    /// Returns the decay multiplier for a band. Higher bands decay slower, so risk that has
    /// climbed high "sticks" longer before it bleeds off.
    /// </summary>
    public static float DecayMultiplier(
        RiskLevel level,
        float lowMultiplier,
        float mediumMultiplier,
        float highMultiplier,
        float criticalMultiplier
    )
    {
        switch (level)
        {
            case RiskLevel.Low:
                return lowMultiplier;
            case RiskLevel.Medium:
                return mediumMultiplier;
            case RiskLevel.High:
                return highMultiplier;
            case RiskLevel.Critical:
                return criticalMultiplier;
            default:
                return lowMultiplier;
        }
    }
}
