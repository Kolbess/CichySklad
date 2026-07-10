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

    /// <summary>
    /// Coins a courier pays for a delivery, scaled by the current risk band: the four
    /// <see cref="RiskLevel"/>s spread evenly across <paramref name="minPayment"/>..
    /// <paramref name="maxPayment"/> (rounded to the nearest coin), so a riskier run pays more.
    /// An inverted range is treated as a flat <paramref name="minPayment"/>.
    /// </summary>
    public static int PaymentForRisk(RiskLevel level, int minPayment, int maxPayment)
    {
        if (maxPayment < minPayment)
            maxPayment = minPayment;

        int span = maxPayment - minPayment;
        int index = (int)level; // Low = 0 .. Critical = 3
        int offset = (int)((double)span * index / 3.0 + 0.5);
        return minPayment + offset;
    }

    /// <summary>
    /// Whether automatic risk decay is allowed yet. After an increase, risk is held for
    /// <paramref name="freezeSeconds"/> before it may start bleeding off, so decay is permitted only
    /// once <paramref name="timeSinceLastIncrease"/> has reached that hold. A freeze of 0 always
    /// permits decay.
    /// </summary>
    public static bool CanDecay(float timeSinceLastIncrease, float freezeSeconds) =>
        timeSinceLastIncrease >= freezeSeconds;
}
