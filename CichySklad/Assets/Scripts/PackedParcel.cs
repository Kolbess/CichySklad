using UnityEngine;

/// <summary>
/// A finished, sealed parcel produced by the <see cref="PackingStation"/>. Unlike a
/// <see cref="Package"/>, it does not open on click — it is a standalone item the player moves with
/// the usual <see cref="InteractableObject"/> drag setup, and it simply remembers how many leaflets
/// it holds so a later courier step (G26) can collect it.
///
/// It may rest in a <see cref="PackageShelf"/> slot; it keeps a back-reference only so it can free
/// that slot if it is destroyed (mirrors how <see cref="InteractableObject"/> notifies a
/// <see cref="HidingSpot"/>).
/// </summary>
public class PackedParcel : MonoBehaviour
{
    [Tooltip(
        "Leaflets sealed inside this parcel. Set by the packing station when the parcel is made."
    )]
    [SerializeField]
    private int _leafletCount;

    private PackageShelf _shelf;

    public int LeafletCount => _leafletCount;
    public PackageShelf CurrentShelf => _shelf;

    private void OnDestroy()
    {
        if (_shelf != null)
            _shelf.RemoveParcel(this);
    }

    /// <summary>Stamps the number of leaflets sealed inside; clamped to zero or more.</summary>
    public void Initialize(int leafletCount)
    {
        _leafletCount = leafletCount < 0 ? 0 : leafletCount;
    }

    /// <summary>Records the shelf this parcel is resting on, so it can free the slot when destroyed.</summary>
    public void AssignShelf(PackageShelf shelf) => _shelf = shelf;

    /// <summary>Clears the shelf back-reference when the parcel is taken off the shelf.</summary>
    public void ClearShelf() => _shelf = null;
}
