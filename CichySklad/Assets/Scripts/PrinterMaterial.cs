using UnityEngine;

/// <summary>The two consumables a <see cref="PrintLeaflet"/> station accepts as physical loads.</summary>
public enum PrinterMaterialType
{
    Paper,
    Ink,
}

/// <summary>
/// Marks a draggable object as a printer input of a given <see cref="PrinterMaterialType"/>. Attach
/// it (alongside the existing drag/collider setup) to the paper and ink prefabs the player feeds
/// into the printer; <see cref="PrintLeaflet"/> reads the type off the object dropped onto it to
/// decide which slot it fills. Kept as a tiny standalone marker so no drag logic is duplicated.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PrinterMaterial : MonoBehaviour
{
    [Tooltip("Which printer input this object satisfies: paper or ink.")]
    [SerializeField]
    private PrinterMaterialType _type = PrinterMaterialType.Paper;

    public PrinterMaterialType Type => _type;
}
