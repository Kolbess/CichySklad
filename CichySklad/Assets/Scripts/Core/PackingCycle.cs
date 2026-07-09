using System;

/// <summary>
/// Pure, engine-agnostic state machine + timing for one packing station:
/// <c>Idle → Filling → Packing → Ready → Idle</c>. No <c>MonoBehaviour</c> and no scene state, so the
/// whole loop is unit-testable in EditMode. The runtime <c>PackingStation</c> owns the serialized
/// capacity/cost/durations and the physical leaflet objects; it reports each loaded leaflet, spends
/// the coin, picks the (random) pack duration, and pumps <see cref="Tick"/>, then reacts to the
/// events here to drive the progress bar and dispense the finished package.
///
/// Leaflets are captured into the package the instant packing starts: <see cref="TryStartPacking"/>
/// moves the loaded count into <see cref="PackedCount"/>, and <see cref="Dispense"/> hands it back
/// and returns the station to <see cref="PackingState.Idle"/>.
/// </summary>
public class PackingCycle
{
    private readonly int _capacity;

    private PackingState _state = PackingState.Idle;
    private int _count;
    private int _packedCount;
    private float _packDuration;
    private float _elapsed;

    /// <summary>Fired whenever the phase changes, carrying the new state.</summary>
    public event Action<PackingState> OnStateChanged;

    /// <summary>Fired once when packing completes — the package is now ready to collect.</summary>
    public event Action OnPackFinished;

    public PackingCycle(int capacity)
    {
        _capacity = capacity < 1 ? 1 : capacity;
    }

    public PackingState State => _state;
    public int Capacity => _capacity;

    /// <summary>Leaflets currently loaded and waiting to be packed.</summary>
    public int Count => _count;

    /// <summary>Leaflets captured into the in-progress / finished package.</summary>
    public int PackedCount => _packedCount;

    public bool IsFull => _count >= _capacity;
    public bool CanStart => _state == PackingState.Filling;
    public bool CanDispense => _state == PackingState.Ready;

    /// <summary>Fraction (0..1) of the packing phase elapsed; 0 outside that phase.</summary>
    public float PackProgress
    {
        get
        {
            if (_state != PackingState.Packing || _packDuration <= 0f)
                return 0f;

            float progress = _elapsed / _packDuration;
            if (progress < 0f)
                return 0f;
            return progress > 1f ? 1f : progress;
        }
    }

    /// <summary>
    /// Adds one leaflet if the station is idle/filling and not yet full. Returns <c>true</c> when it
    /// was accepted; <c>false</c> when full or mid-cycle.
    /// </summary>
    public bool TryAddLeaflet()
    {
        if (_state != PackingState.Idle && _state != PackingState.Filling)
            return false;
        if (_count >= _capacity)
            return false;

        _count++;
        SetState(PackingState.Filling);
        return true;
    }

    /// <summary>
    /// Begins packing if at least one leaflet is loaded. On success it captures the loaded leaflets
    /// into <see cref="PackedCount"/>, clears the load, enters <see cref="PackingState.Packing"/> for
    /// <paramref name="duration"/> seconds, and returns <c>true</c>; otherwise returns <c>false</c>.
    /// </summary>
    public bool TryStartPacking(float duration)
    {
        if (_state != PackingState.Filling)
            return false;

        _packedCount = _count;
        _count = 0;
        _packDuration = duration > 0f ? duration : 0.01f;
        _elapsed = 0f;
        SetState(PackingState.Packing);
        return true;
    }

    /// <summary>Advances the packing phase by <paramref name="deltaTime"/> seconds.</summary>
    public void Tick(float deltaTime)
    {
        if (deltaTime <= 0f)
            return;
        if (_state != PackingState.Packing)
            return;

        _elapsed += deltaTime;
        if (_elapsed >= _packDuration)
        {
            _elapsed = 0f;
            OnPackFinished?.Invoke();
            SetState(PackingState.Ready);
        }
    }

    /// <summary>
    /// Hands back the finished package's leaflet count and returns the station to idle. Returns 0 if
    /// no package is ready.
    /// </summary>
    public int Dispense()
    {
        if (_state != PackingState.Ready)
            return 0;

        int packed = _packedCount;
        _packedCount = 0;
        SetState(PackingState.Idle);
        return packed;
    }

    private void SetState(PackingState next)
    {
        if (_state == next)
            return;
        _state = next;
        OnStateChanged?.Invoke(_state);
    }
}
