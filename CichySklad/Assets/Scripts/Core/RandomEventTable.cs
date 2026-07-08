using System;
using System.Collections.Generic;

/// <summary>
/// Pure, engine-agnostic helper for picking one option from a fixed set by a random roll. Extracted
/// from <c>DayCycle</c> so the daily event/windfall selection is unit-testable without the engine,
/// and — crucially — so the roll's exclusive upper bound is DERIVED from the option count instead of
/// a hand-written literal.
///
/// This kills a whole bug class: <c>UnityEngine.Random.Range(int, int)</c> has an EXCLUSIVE upper
/// bound, so a literal like <c>Random.Range(1, 3)</c> paired with a three-case <c>switch</c> silently
/// left the last case unreachable. Here <see cref="RollBound{T}"/> == option count by construction,
/// and <see cref="Select{T}"/> throws on any out-of-range roll rather than missing a case.
/// </summary>
public static class RandomEventTable
{
    /// <summary>
    /// The exclusive upper bound to hand <c>UnityEngine.Random.Range(0, N)</c> so that every option in
    /// <paramref name="options"/> is reachable. Equal to the option count by construction — never a
    /// magic literal.
    /// </summary>
    public static int RollBound<T>(IReadOnlyList<T> options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));
        if (options.Count == 0)
            throw new ArgumentException(
                "Event table must contain at least one option.",
                nameof(options)
            );
        return options.Count;
    }

    /// <summary>
    /// Returns the option selected by <paramref name="roll"/>. <paramref name="roll"/> must lie in
    /// <c>[0, RollBound(options))</c> — exactly the range <c>Random.Range(0, RollBound(options))</c>
    /// produces — otherwise it throws, surfacing a bad roll instead of silently missing a case.
    /// </summary>
    public static T Select<T>(IReadOnlyList<T> options, int roll)
    {
        int bound = RollBound(options);
        if (roll < 0 || roll >= bound)
            throw new ArgumentOutOfRangeException(
                nameof(roll),
                roll,
                $"Roll must be in [0, {bound}); every option is then reachable."
            );
        return options[roll];
    }
}
