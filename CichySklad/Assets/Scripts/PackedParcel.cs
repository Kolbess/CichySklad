using UnityEngine;

/// <summary>
/// A finished, sealed parcel produced by the <see cref="PackingStation"/>. Unlike a
/// <see cref="Package"/>, it does not open on click — it is a standalone item the player moves with
/// the usual <see cref="InteractableObject"/> drag setup, and it simply remembers how many leaflets
/// it holds so a later courier step (G26) can collect it.
/// </summary>
public class PackedParcel : MonoBehaviour
{
    [Tooltip(
        "Leaflets sealed inside this parcel. Set by the packing station when the parcel is made."
    )]
    [SerializeField]
    private int _leafletCount;

    public int LeafletCount => _leafletCount;

    /// <summary>Stamps the number of leaflets sealed inside; clamped to zero or more.</summary>
    public void Initialize(int leafletCount)
    {
        _leafletCount = leafletCount < 0 ? 0 : leafletCount;
    }
}
