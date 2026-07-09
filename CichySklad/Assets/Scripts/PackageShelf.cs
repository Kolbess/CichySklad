using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// A shelf that tidies finished <see cref="PackedParcel"/>s into fixed slots until a courier collects
/// them (G26). Modelled on <see cref="HidingSpot"/>: the player drags a parcel onto the shelf (a
/// trigger snaps it to a free slot) and drags it back off (which frees the slot). The pure
/// <see cref="ShelfSlotMap"/> owns the slot-occupancy bookkeeping; this component owns the transforms,
/// the parcel objects, and the trigger wiring. Capacity equals the number of assigned slots.
///
/// Needs a trigger <c>Collider2D</c> covering the shelf, and parcels need a <c>Rigidbody2D</c> for the
/// 2D trigger callbacks to fire — the same setup the other stations rely on.
/// </summary>
public class PackageShelf : MonoBehaviour
{
    [Header("Slots")]
    [Tooltip(
        "Slot transforms parcels snap onto, in order. The number of slots IS the shelf capacity. "
            + "At least one is required."
    )]
    [SerializeField]
    private List<Transform> _slots = new List<Transform>();

    private ShelfSlotMap _slotMap;
    private readonly List<PackedParcel> _parcels = new List<PackedParcel>();
    private readonly Dictionary<PackedParcel, int> _slotByParcel =
        new Dictionary<PackedParcel, int>();

    /// <summary>Fired after a parcel is stored or removed, so a courier (G26) can react to availability.</summary>
    public event Action OnContentsChanged;

    public IReadOnlyList<PackedParcel> StoredParcels => _parcels;
    public int StoredCount => _parcels.Count;
    public int Capacity => _slotMap?.Capacity ?? _slots.Count;
    public bool IsFull => _slotMap != null && _slotMap.IsFull;

    private void Awake()
    {
        Assert.IsTrue(
            _slots.Count > 0,
            $"[{nameof(PackageShelf)}] No slots assigned on {name} — capacity would be zero!"
        );
        foreach (Transform slot in _slots)
            Assert.IsNotNull(
                slot,
                $"[{nameof(PackageShelf)}] A slot transform is unassigned on {name}!"
            );

        _slotMap = new ShelfSlotMap(_slots.Count);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Mirror HidingSpot: only swallow a parcel once the player releases the drag over the shelf.
        if (Input.GetMouseButton(0))
            return;
        if (other.TryGetComponent(out PackedParcel parcel))
            TryStore(parcel);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Dragging a shelved parcel off the shelf frees its slot.
        if (other.TryGetComponent(out PackedParcel parcel))
            RemoveParcel(parcel);
    }

    // =====================================================================
    // Public API
    // =====================================================================

    /// <summary>
    /// Stores a parcel on the first free slot. Returns <c>false</c> if it is already shelved or the
    /// shelf is full (overflow is blocked).
    /// </summary>
    public bool TryStore(PackedParcel parcel)
    {
        if (parcel == null)
            return false;
        if (_slotByParcel.ContainsKey(parcel))
            return false;

        int index = _slotMap.Occupy();
        if (index < 0)
            return false;

        _parcels.Add(parcel);
        _slotByParcel[parcel] = index;
        parcel.transform.position = _slots[index].position;
        parcel.AssignShelf(this);

        OnContentsChanged?.Invoke();
        return true;
    }

    /// <summary>Removes a parcel from the shelf and frees its slot. No-op if it was not shelved here.</summary>
    public void RemoveParcel(PackedParcel parcel)
    {
        if (parcel == null)
            return;
        if (!_slotByParcel.TryGetValue(parcel, out int index))
            return;

        _slotMap.Free(index);
        _slotByParcel.Remove(parcel);
        _parcels.Remove(parcel);
        parcel.ClearShelf();

        OnContentsChanged?.Invoke();
    }
}
