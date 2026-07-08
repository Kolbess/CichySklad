using System;
using System.Collections.Generic;

/// <summary>
/// Pure, deterministic weighted selection over a <see cref="GameEventDefinition"/> pool. Engine-free
/// so it is fully unit-testable in EditMode: the runtime <c>EventScheduler</c> supplies the random
/// roll (via <c>UnityEngine.Random</c>) and this class turns it into a chosen event. Filtering
/// (day / risk band / one-shot) is separated from the weighted pick so each can be tested in
/// isolation.
/// </summary>
public static class EventSelector
{
    /// <summary>
    /// Returns every definition in <paramref name="pool"/> eligible on <paramref name="day"/> at
    /// <paramref name="riskLevel"/>, excluding one-shot events whose id is in
    /// <paramref name="firedOnceEvents"/>. Order follows the pool for deterministic weighting.
    /// </summary>
    public static List<GameEventDefinition> Eligible(
        IReadOnlyList<GameEventDefinition> pool,
        int day,
        RiskLevel riskLevel,
        ISet<GameEventId> firedOnceEvents
    )
    {
        if (pool == null)
            throw new ArgumentNullException(nameof(pool));

        var eligible = new List<GameEventDefinition>(pool.Count);
        foreach (GameEventDefinition def in pool)
        {
            bool alreadyFired = firedOnceEvents != null && firedOnceEvents.Contains(def.Id);
            if (def.IsEligible(day, riskLevel, alreadyFired))
                eligible.Add(def);
        }
        return eligible;
    }

    /// <summary>Sum of the weights of <paramref name="eligible"/>; the exclusive upper bound for the roll.</summary>
    public static int TotalWeight(IReadOnlyList<GameEventDefinition> eligible)
    {
        if (eligible == null)
            throw new ArgumentNullException(nameof(eligible));

        int total = 0;
        foreach (GameEventDefinition def in eligible)
            total += def.Weight;
        return total;
    }

    /// <summary>
    /// Maps a <paramref name="weightRoll"/> in <c>[0, TotalWeight(eligible))</c> onto the event whose
    /// cumulative weight band it lands in. This is the range <c>Random.Range(0, TotalWeight)</c>
    /// produces; any other value throws so a bad roll surfaces instead of silently skewing selection.
    /// </summary>
    public static GameEventDefinition PickWeighted(
        IReadOnlyList<GameEventDefinition> eligible,
        int weightRoll
    )
    {
        if (eligible == null)
            throw new ArgumentNullException(nameof(eligible));
        if (eligible.Count == 0)
            throw new ArgumentException("Cannot pick from an empty pool.", nameof(eligible));

        int total = TotalWeight(eligible);
        if (weightRoll < 0 || weightRoll >= total)
            throw new ArgumentOutOfRangeException(
                nameof(weightRoll),
                weightRoll,
                $"Roll must be in [0, {total}) — exactly Random.Range(0, TotalWeight)."
            );

        int cumulative = 0;
        foreach (GameEventDefinition def in eligible)
        {
            cumulative += def.Weight;
            if (weightRoll < cumulative)
                return def;
        }

        // Unreachable: weightRoll < total guarantees a band was hit above.
        throw new InvalidOperationException(
            "Weighted selection fell through — pool weights changed?"
        );
    }
}
