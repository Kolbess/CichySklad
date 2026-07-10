using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

/// <summary>
/// Closes the production loop: the courier takes one finished <see cref="PackedParcel"/> per visit —
/// either a parcel the player drags onto it, or the front parcel of an assigned
/// <see cref="PackageShelf"/> when clicked — pays coins scaled by the current risk band (via the pure
/// <see cref="RiskCalculator.PaymentForRisk"/>), and raises trust. With no parcel available it simply
/// announces that and takes nothing. No singletons: risk/resources are injected as references.
///
/// He does not linger every day: when <c>_appearsOnSchedule</c> is on he shows up in bounded random
/// gaps of <c>_minGapDays</c>..<c>_maxGapDays</c> days (guaranteed within the max, never pure chance),
/// driven by <see cref="DayCycle.OnDayStarted"/>, and leaves after taking a parcel. The gap cadence is
/// the pure, tested <see cref="CourierSchedule"/>.
///
/// Needs a trigger <c>Collider2D</c> for the drag-drop pickup and a <c>SpriteRenderer</c> to show/hide
/// him; dragged parcels need a <c>Rigidbody2D</c> for the 2D trigger callbacks.
/// </summary>
public class Courier : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Risk source read to scale the payment. Required.")]
    [SerializeField]
    private RiskManager _riskManager;

    [Tooltip("Resource sink that receives the payment and trust. Required.")]
    [SerializeField]
    private ResourceManager _resourceManager;

    [Tooltip(
        "Shelf the courier pulls a parcel from when clicked. Optional (drag-drop still works)."
    )]
    [SerializeField]
    private PackageShelf _packageShelf;

    [Tooltip(
        "Dialogue used to announce a pickup or an empty visit. Optional (falls back to a log)."
    )]
    [SerializeField]
    private DialogueSystem _dialogueSystem;

    [Header("Payment (coins, scaled by risk band)")]
    [Tooltip("Coins paid at the lowest risk band. Must be >= 1.")]
    [SerializeField]
    private int _minPayment = 1;

    [Tooltip("Coins paid at the highest risk band. Must be >= Min Payment.")]
    [SerializeField]
    private int _maxPayment = 5;

    [Header("Reward")]
    [Tooltip("Trust gained on a successful delivery. Must be >= 0.")]
    [SerializeField]
    private int _trustReward = 5;

    [Header("Appearance Schedule")]
    [Tooltip(
        "If on, the courier only shows up on scheduled visit days (needs a Day Cycle). If off, he is always present."
    )]
    [SerializeField]
    private bool _appearsOnSchedule = true;

    [Tooltip("Day source that drives the visit cadence. Required when Appears On Schedule is on.")]
    [SerializeField]
    private DayCycle _dayCycle;

    [Tooltip("Fewest days between visits. Must be >= 1.")]
    [SerializeField]
    private int _minGapDays = 2;

    [Tooltip("Most days between visits. Must be >= Min Gap Days.")]
    [SerializeField]
    private int _maxGapDays = 4;

    [Header("Debug")]
    [Tooltip(
        "Logs the courier's schedule, arrivals and departures to the Console. Turn off for release."
    )]
    [SerializeField]
    private bool _debugLogging = true;

    private CourierSchedule _schedule;
    private bool _present;
    private int _currentDay;
    private SpriteRenderer _spriteRenderer;
    private Collider2D[] _colliders;

    /// <summary>Whether the courier is currently here and able to take a parcel.</summary>
    public bool IsPresent => _present;

    private void OnValidate()
    {
        if (_minPayment < 1)
            _minPayment = 1;
        if (_maxPayment < _minPayment)
            _maxPayment = _minPayment;
        if (_trustReward < 0)
            _trustReward = 0;
        if (_minGapDays < 1)
            _minGapDays = 1;
        if (_maxGapDays < _minGapDays)
            _maxGapDays = _minGapDays;
    }

    private void Awake()
    {
        Assert.IsNotNull(_riskManager, $"[{nameof(Courier)}] RiskManager unassigned on {name}!");
        Assert.IsNotNull(
            _resourceManager,
            $"[{nameof(Courier)}] ResourceManager unassigned on {name}!"
        );

        _spriteRenderer = GetComponent<SpriteRenderer>();
        _colliders = GetComponents<Collider2D>();
        _schedule = new CourierSchedule(_minGapDays, _maxGapDays);

        if (_appearsOnSchedule)
        {
            _schedule.ScheduleAfter(0, RollGap()); // first visit within [min, max] days
            SetPresent(false);
            Log(
                $"Awake — scheduled. First visit on day {_schedule.NextVisitDay} "
                    + $"(gap {_minGapDays}-{_maxGapDays}). Hidden until then. "
                    + $"Renderer={(_spriteRenderer != null ? "found" : "MISSING")}, colliders={_colliders.Length}."
            );
        }
        else
        {
            SetPresent(true);
            Log("Awake — not scheduled, always present.");
        }
    }

    private void OnEnable()
    {
        if (_appearsOnSchedule && _dayCycle == null)
        {
            Debug.LogWarning(
                $"[{nameof(Courier)}] 'Appears On Schedule' is ON but no Day Cycle is assigned on "
                    + $"{name} — the courier will NEVER appear. Assign a DayCycle reference, or turn "
                    + "off 'Appears On Schedule' to keep him always present."
            );
            return;
        }

        if (_dayCycle != null)
        {
            _dayCycle.OnDayStarted += HandleDayStarted;
            Log($"Subscribed to DayCycle '{_dayCycle.name}'.");
        }
    }

    private void OnDisable()
    {
        if (_dayCycle != null)
            _dayCycle.OnDayStarted -= HandleDayStarted;
    }

    // Clicking the courier makes it collect the next parcel from its shelf.
    private void OnMouseDown() => CollectFromShelf();

    private void OnTriggerStay2D(Collider2D other)
    {
        // Mirror the other stations: only accept a parcel once the player releases the drag over us.
        if (Input.GetMouseButton(0))
            return;
        if (other.TryGetComponent(out PackedParcel parcel))
            TryCollect(parcel);
    }

    // =====================================================================
    // Public API
    // =====================================================================

    /// <summary>
    /// Takes the front parcel off the assigned shelf and pays for it. Announces an empty visit and
    /// takes nothing when there is no shelf or it is empty. Exposed so a scheduled event can drive a
    /// courier visit as well as a click.
    /// </summary>
    public void CollectFromShelf()
    {
        if (!_present)
            return; // the courier is not here right now

        if (_packageShelf == null || _packageShelf.StoredCount == 0)
        {
            Announce("Kurier: brak paczki do odebrania.");
            return;
        }

        TryCollect(_packageShelf.StoredParcels[0]);
    }

    /// <summary>
    /// Collects a specific parcel: pays coins scaled by the current risk band, raises trust, and
    /// consumes the parcel (which frees its shelf slot if it was on one). On a scheduled courier this
    /// ends the visit. Returns <c>false</c> for a null parcel or while the courier is away.
    /// </summary>
    public bool TryCollect(PackedParcel parcel)
    {
        if (parcel == null || !_present)
            return false;

        int leaflets = parcel.LeafletCount;
        int payment = RiskCalculator.PaymentForRisk(
            _riskManager.CurrentRiskLevel,
            _minPayment,
            _maxPayment
        );
        _resourceManager.AddMoney(payment);
        _resourceManager.AddTrust(_trustReward);
        // The packed leaflets leave with the courier — clear them from the counter now (they were
        // physically consumed at packing time, so this only reconciles the ledger).
        _resourceManager.NotifyConsumed(ResourceType.Leaflets, leaflets);
        Log(
            $"Collected a parcel ({leaflets} leaflets) at risk {_riskManager.CurrentRiskLevel}: "
                + $"paid {payment}, trust +{_trustReward}."
        );
        Announce($"Kurier odebrał paczkę. Zapłata: {payment}, zaufanie +{_trustReward}.");

        // Deactivate first so a deferred Destroy cannot fire the trigger again this frame.
        parcel.gameObject.SetActive(false);
        Destroy(parcel.gameObject);

        if (_appearsOnSchedule)
            SetPresent(false); // one parcel per visit, then he leaves until the next scheduled day
        return true;
    }

    /// <summary>Day-cycle hook: arrives on a scheduled visit day, otherwise ends any current visit.</summary>
    public void HandleDayStarted(int day)
    {
        if (!_appearsOnSchedule)
            return;

        _currentDay = day;
        Log(
            $"Day {day} started — nextVisitDay={_schedule.NextVisitDay}, "
                + $"isVisitDay={_schedule.IsVisitDay(day)}, present={_present}."
        );

        if (_schedule.IsVisitDay(day) && !_present)
        {
            Arrive();
        }
        else if (_present)
        {
            SetPresent(false); // the visit window has passed
            Log($"Departed after his visit (day {day}).");
        }
    }

    // =====================================================================
    // Private helpers
    // =====================================================================

    private void Arrive()
    {
        SetPresent(true);
        _schedule.ScheduleAfter(_currentDay, RollGap());
        Log($"ARRIVED on day {_currentDay}. Next visit day {_schedule.NextVisitDay}.");
    }

    private void SetPresent(bool present)
    {
        _present = present;
        if (_spriteRenderer != null)
            _spriteRenderer.enabled = present;
        if (_colliders != null)
        {
            foreach (Collider2D col in _colliders)
                col.enabled = present;
        }
    }

    private int RollGap() => Random.Range(_minGapDays, _maxGapDays + 1);

    private void Announce(string message)
    {
        if (_dialogueSystem != null)
            _dialogueSystem.ShowDialogue(message);
        else
            Debug.Log($"[{nameof(Courier)}] {message}");
    }

    private void Log(string message)
    {
        if (_debugLogging)
            Debug.Log($"[{nameof(Courier)}] {message}");
    }
}
