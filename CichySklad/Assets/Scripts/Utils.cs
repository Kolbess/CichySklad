using UnityEngine;

public class Utils : MonoBehaviour
{
    private static readonly int OutlineEnabled = Shader.PropertyToID("_OutlineEnabled");

    public static Vector3 MouseToWorldPosition(Camera cam)
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10;
        return cam.ScreenToWorldPoint(mousePos);
    }
    
    public static void EnableOutline(Material material)
    {
        material.SetInt(OutlineEnabled, 1);
        // Debug.Log("EnableOutline" + material.name + " enabled");
    }
    
    public static void DisableOutline(Material material)
    {
        material.SetInt(OutlineEnabled, 0);
        // Debug.Log("EnableOutline" + material.name + " disabled");
    }
    
    public static bool IsAnimationFinished(Animator animator, string animation)
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        Debug.Log($"{state.IsName(animation)}, Hello: {state.normalizedTime >= 1f}");
        return state.IsName(animation) && state.normalizedTime >= 1f;
    }
}
