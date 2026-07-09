using System;

/// <summary>
/// Pure, engine-agnostic state machine + timing for one printing station:
/// <c>Idle → Loaded → Printing → CoolingDown → Idle</c>. It holds no <c>MonoBehaviour</c> and no
/// scene state, so the whole loop is unit-testable in EditMode. The runtime <c>PrintLeaflet</c>
/// owns the serialized durations and the physical material objects; it reports load/unload and
/// pumps <see cref="Tick"/>, then reacts to the events here to spend resources, drive the progress
/// bar, and mint a leaflet.
///
/// Materials are consumed the instant printing begins: <see cref="TryStartPrinting"/> clears the
/// loaded flags, so the cycle drops back to <see cref="PrinterState.Idle"/> once the cooldown ends.
/// </summary>
public class PrinterCycle
{
    private float _printDuration;
    private float _cooldownDuration;

    private PrinterState _state = PrinterState.Idle;
    private bool _hasPaper;
    private bool _hasInk;
    private float _elapsed;

    /// <summary>Fired whenever the phase changes, carrying the new state.</summary>
    public event Action<PrinterState> OnStateChanged;

    /// <summary>Fired once when printing completes — the exact moment a leaflet is produced.</summary>
    public event Action OnPrintFinished;

    public PrinterCycle(float printDuration, float cooldownDuration)
    {
        Configure(printDuration, cooldownDuration);
    }

    public PrinterState State => _state;
    public bool HasPaper => _hasPaper;
    public bool HasInk => _hasInk;
    public bool IsLoaded => _hasPaper && _hasInk;
    public bool CanStart => _state == PrinterState.Loaded;

    /// <summary>Fraction (0..1) of the printing phase elapsed; 0 outside that phase.</summary>
    public float PrintProgress => PhaseProgress(PrinterState.Printing, _printDuration);

    /// <summary>Fraction (0..1) of the cooldown phase elapsed; 0 outside that phase.</summary>
    public float CooldownProgress => PhaseProgress(PrinterState.CoolingDown, _cooldownDuration);

    /// <summary>Stores the phase durations, forcing each to a small positive minimum.</summary>
    public void Configure(float printDuration, float cooldownDuration)
    {
        _printDuration = printDuration > 0f ? printDuration : 0.01f;
        _cooldownDuration = cooldownDuration > 0f ? cooldownDuration : 0.01f;
    }

    /// <summary>Reports whether paper is loaded. Only moves the idle/loaded gate, never a live job.</summary>
    public void SetPaperLoaded(bool loaded)
    {
        _hasPaper = loaded;
        RefreshLoadGate();
    }

    /// <summary>Reports whether ink is loaded. Only moves the idle/loaded gate, never a live job.</summary>
    public void SetInkLoaded(bool loaded)
    {
        _hasInk = loaded;
        RefreshLoadGate();
    }

    /// <summary>
    /// Begins printing if — and only if — both materials are loaded. On success it consumes the
    /// load (clears the flags), enters <see cref="PrinterState.Printing"/>, and returns <c>true</c>;
    /// otherwise it leaves the cycle untouched and returns <c>false</c>.
    /// </summary>
    public bool TryStartPrinting()
    {
        if (_state != PrinterState.Loaded)
            return false;

        _hasPaper = false;
        _hasInk = false;
        _elapsed = 0f;
        SetState(PrinterState.Printing);
        return true;
    }

    /// <summary>Advances the active phase by <paramref name="deltaTime"/> seconds.</summary>
    public void Tick(float deltaTime)
    {
        if (deltaTime <= 0f)
            return;

        switch (_state)
        {
            case PrinterState.Printing:
                _elapsed += deltaTime;
                if (_elapsed >= _printDuration)
                {
                    _elapsed = 0f;
                    OnPrintFinished?.Invoke();
                    SetState(PrinterState.CoolingDown);
                }
                break;

            case PrinterState.CoolingDown:
                _elapsed += deltaTime;
                if (_elapsed >= _cooldownDuration)
                {
                    _elapsed = 0f;
                    // Materials were consumed at start, so IsLoaded is false → Idle.
                    SetState(IsLoaded ? PrinterState.Loaded : PrinterState.Idle);
                }
                break;
        }
    }

    private void RefreshLoadGate()
    {
        if (_state != PrinterState.Idle && _state != PrinterState.Loaded)
            return;
        SetState(IsLoaded ? PrinterState.Loaded : PrinterState.Idle);
    }

    private float PhaseProgress(PrinterState phase, float duration)
    {
        if (_state != phase || duration <= 0f)
            return 0f;

        float progress = _elapsed / duration;
        if (progress < 0f)
            return 0f;
        return progress > 1f ? 1f : progress;
    }

    private void SetState(PrinterState next)
    {
        if (_state == next)
            return;
        _state = next;
        OnStateChanged?.Invoke(_state);
    }
}
