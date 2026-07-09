/// <summary>
/// The phases of one packing-station work cycle:
/// <c>Idle → Filling → Packing → Ready → Idle</c>. <see cref="Idle"/> is empty; <see cref="Filling"/>
/// holds 1..capacity leaflets and can start; <see cref="Packing"/> is the timed job; <see cref="Ready"/>
/// is a finished package waiting to be collected with a click, which returns the station to idle.
/// </summary>
public enum PackingState
{
    Idle,
    Filling,
    Packing,
    Ready,
}
