/// <summary>
/// The phases of one printing-station work cycle:
/// <c>Idle → Loaded → Printing → CoolingDown → Idle</c>. <see cref="Idle"/> waits for both
/// materials; <see cref="Loaded"/> is armed and can start; <see cref="Printing"/> is the active
/// job; <see cref="CoolingDown"/> is the short recovery before the station accepts a new load.
/// </summary>
public enum PrinterState
{
    Idle,
    Loaded,
    Printing,
    CoolingDown,
}
