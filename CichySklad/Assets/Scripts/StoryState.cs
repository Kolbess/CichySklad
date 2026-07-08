using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Per-run store of <see cref="StoryFlag"/>s driving the multi-stage story threads. A plain scene
/// component (no singleton): <c>EventHandler</c> sets flags from the player's choices and
/// <c>EventScheduler</c> reads <see cref="ActiveFlags"/> to gate which story stage is eligible next.
/// Flags live only as long as the scene, so every fresh run starts a clean story.
/// </summary>
public class StoryState : MonoBehaviour
{
    private readonly HashSet<StoryFlag> _flags = new HashSet<StoryFlag>();

    /// <summary>The currently-set flags, for pool eligibility gating.</summary>
    public IReadOnlyCollection<StoryFlag> ActiveFlags => _flags;

    public bool Has(StoryFlag flag) => _flags.Contains(flag);

    public void Set(StoryFlag flag) => _flags.Add(flag);

    public void Clear(StoryFlag flag) => _flags.Remove(flag);

    /// <summary>Wipes all story progress (e.g. to restart a run without reloading the scene).</summary>
    public void ResetStory() => _flags.Clear();
}
