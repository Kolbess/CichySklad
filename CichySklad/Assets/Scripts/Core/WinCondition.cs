/// <summary>
/// Pure, engine-agnostic win check. No <c>MonoBehaviour</c>, no scene state — so it is trivially
/// unit-testable in EditMode. The runtime <c>DayCycle</c> owns the serialized targets (win day,
/// trust threshold) and delegates the actual decision here.
///
/// A run is won by surviving <em>to or past</em> the target day while holding <em>at least</em>
/// the target trust. Both comparisons are inclusive lower bounds (<c>&gt;=</c>), never equality —
/// an exact-value check (the old <c>Trust == 100</c>) is practically impossible for a player to hit.
/// </summary>
public static class WinCondition
{
    /// <summary>
    /// Returns <c>true</c> when the run's win condition is satisfied: the finished day has reached
    /// <paramref name="winDay"/> and trust has reached <paramref name="winTrustThreshold"/>.
    /// </summary>
    public static bool IsMet(int currentDay, int winDay, int trust, int winTrustThreshold)
    {
        return currentDay >= winDay && trust >= winTrustThreshold;
    }
}
