/// <summary>
/// Pure, engine-agnostic threshold maths for trust-gated progression: whether a trust level has
/// reached a threshold, and whether a change just crossed it (so an unlock fires once, on the way up,
/// and never re-fires or fires on a decrease). No <c>MonoBehaviour</c>, so it is unit-testable in
/// EditMode. The runtime <c>ProgressionSystem</c> owns the unlock list and applies effects.
/// </summary>
public static class ProgressionLadder
{
    /// <summary>Whether <paramref name="trust"/> is at or above <paramref name="threshold"/>.</summary>
    public static bool IsReached(int trust, int threshold) => trust >= threshold;

    /// <summary>
    /// Whether trust moving from <paramref name="previousTrust"/> to <paramref name="currentTrust"/>
    /// just crossed <paramref name="threshold"/> upward — true only on the crossing, so an unlock is
    /// granted exactly once and a later dip-and-rise (guarded by "already unlocked") won't repeat it.
    /// </summary>
    public static bool WasJustReached(int previousTrust, int currentTrust, int threshold) =>
        previousTrust < threshold && currentTrust >= threshold;
}
