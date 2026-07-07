using UnityEngine;

/// <summary>
/// Stateless helper functions shared across the project: mouse-to-world projection, sprite outline
/// toggling, and animation-completion checks. A plain static class — never a component.
/// </summary>
public static class Utils
{
    private const float DefaultMouseDepth = 10f;
    private static readonly int OutlineEnabled = Shader.PropertyToID("_OutlineEnabled");

    public static Vector3 MouseToWorldPosition(Camera cam, float depth = DefaultMouseDepth)
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = depth;
        return cam.ScreenToWorldPoint(mousePos);
    }

    public static void EnableOutline(Material material) => material.SetInt(OutlineEnabled, 1);

    public static void DisableOutline(Material material) => material.SetInt(OutlineEnabled, 0);

    public static bool IsAnimationFinished(Animator animator, string animation)
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        return state.IsName(animation) && state.normalizedTime >= 1f;
    }
}
