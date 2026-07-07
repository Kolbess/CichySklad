using UnityEngine;

/// <summary>
/// Pure movement maths for the inspection NPC. Kept out of any <c>MonoBehaviour</c> so the
/// cardinal-facing logic can be exercised in EditMode without a scene.
/// </summary>
public static class InspectionPath
{
    /// <summary>
    /// Collapses a movement delta onto a single cardinal facing: the dominant axis becomes
    /// ±1 and the other axis becomes 0. Returned as <c>(MoveX, MoveY)</c> for animator blend
    /// parameters. A zero delta yields <c>(0, 0)</c>.
    /// </summary>
    public static Vector2 CardinalFacing(Vector2 delta)
    {
        if (delta == Vector2.zero)
            return Vector2.zero;

        if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
            return new Vector2(0f, delta.y > 0f ? 1f : -1f);

        return new Vector2(delta.x > 0f ? 1f : -1f, 0f);
    }
}
