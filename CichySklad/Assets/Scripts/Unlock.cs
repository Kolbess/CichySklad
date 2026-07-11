using UnityEngine;

/// <summary>The tangible effect an <see cref="Unlock"/> applies when its trust threshold is reached.</summary>
public enum UnlockEffect
{
    /// <summary>No mechanical effect — just the message and the <c>IsUnlocked</c> flag other systems read.</summary>
    None,

    /// <summary>Grants a one-off coin reward (the organisation backs you).</summary>
    MoneyReward,

    /// <summary>Permanently enlarges the assigned hiding spot ("better stash").</summary>
    ExtraHidingCapacity,
}

/// <summary>
/// One progression reward, unlocked once the player's trust reaches <see cref="TrustThreshold"/>:
/// a display title/description for the "unlocked!" message, a <see cref="UnlockEffect"/> and its
/// magnitude. Authored as an asset so designers tune the ladder without touching code. Keep the
/// thresholds at or below the win-trust target so progression leads toward victory (A1).
/// </summary>
[CreateAssetMenu(fileName = "Unlock", menuName = "CichySklad/Unlock")]
public class Unlock : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Stable key other systems check via ProgressionSystem.IsUnlocked. Must be unique.")]
    [SerializeField]
    private string _key;

    [Tooltip("Trust (0..100) at or above which this unlocks. Keep <= the win-trust target.")]
    [SerializeField]
    private int _trustThreshold = 25;

    [Header("Presentation")]
    [Tooltip("Short name shown in the 'unlocked' message.")]
    [SerializeField]
    private string _title;

    [Tooltip("One-line description of what it grants.")]
    [TextArea]
    [SerializeField]
    private string _description;

    [Header("Effect")]
    [Tooltip("What the unlock actually does to the game.")]
    [SerializeField]
    private UnlockEffect _effect = UnlockEffect.None;

    [Tooltip(
        "Magnitude of the effect: coins for MoneyReward, extra slots for ExtraHidingCapacity."
    )]
    [SerializeField]
    private int _magnitude = 1;

    public string Key => _key;
    public int TrustThreshold => _trustThreshold;
    public string Title => _title;
    public string Description => _description;
    public UnlockEffect Effect => _effect;
    public int Magnitude => _magnitude;

    private void OnValidate()
    {
        _trustThreshold = Mathf.Clamp(_trustThreshold, 0, 100);
        if (_magnitude < 0)
            _magnitude = 0;
    }
}
