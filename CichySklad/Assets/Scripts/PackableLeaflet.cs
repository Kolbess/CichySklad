using UnityEngine;

/// <summary>
/// Marks a draggable object as a leaflet the <see cref="PackingStation"/> will accept as input.
/// Attach it (alongside the existing drag/collider setup) to the leaflet prefab the printer produces;
/// the packing station detects it on drop, exactly the way <see cref="PrinterMaterial"/> feeds the
/// printer. Kept as a tiny standalone marker so no drag logic is duplicated.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PackableLeaflet : MonoBehaviour { }
