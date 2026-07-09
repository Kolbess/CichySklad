/// <summary>
/// Pure, engine-agnostic slot-occupancy bookkeeping for a fixed-size shelf: which of N slots are
/// filled, the first free one, and the occupied count. No <c>MonoBehaviour</c> and no scene state, so
/// it is trivially unit-testable in EditMode. The runtime <c>PackageShelf</c> owns the slot transforms
/// and the parcel objects and delegates the "which slot" decision here.
/// </summary>
public class ShelfSlotMap
{
    private readonly bool[] _occupied;
    private int _occupiedCount;

    public ShelfSlotMap(int capacity)
    {
        _occupied = new bool[capacity < 0 ? 0 : capacity];
    }

    public int Capacity => _occupied.Length;
    public int OccupiedCount => _occupiedCount;
    public bool IsFull => _occupiedCount >= _occupied.Length;
    public bool IsEmpty => _occupiedCount == 0;

    /// <summary>Marks the first free slot as occupied and returns its index, or -1 when full.</summary>
    public int Occupy()
    {
        for (int i = 0; i < _occupied.Length; i++)
        {
            if (!_occupied[i])
            {
                _occupied[i] = true;
                _occupiedCount++;
                return i;
            }
        }
        return -1;
    }

    public bool IsOccupied(int index) => index >= 0 && index < _occupied.Length && _occupied[index];

    /// <summary>Frees a previously occupied slot. Returns <c>false</c> if it was out of range or already free.</summary>
    public bool Free(int index)
    {
        if (!IsOccupied(index))
            return false;

        _occupied[index] = false;
        _occupiedCount--;
        return true;
    }
}
