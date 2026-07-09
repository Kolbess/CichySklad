using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

/// <summary>
/// A packing station: the player drags finished leaflets onto it (up to <c>_capacity</c>, detected by
/// trigger the same way <see cref="PrintLeaflet"/> loads paper/ink), then clicks to start packing —
/// which spends one coin and runs for a random 1..3s with a visible progress bar. When packing
/// finishes, a click dispenses a sealed <see cref="PackedParcel"/> — a standalone, movable item (not
/// openable) stamped with the packed leaflet count for the courier step. The pure
/// <see cref="PackingCycle"/> owns the state/timing/capacity; this component owns the scene wiring.
///
/// Consumption is deliberately physical: the loaded leaflet GameObjects are destroyed at pack start
/// (the "leaflets went into the box" feedback). The only ledger cost is the coin; if a scene wires the
/// leaflet objects to mirror <see cref="ResourceManager.Leaflets"/>, debit that counter yourself to
/// keep the two in sync.
/// </summary>
public class PackingStation : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Resource source charged one coin per pack. Required.")]
    [SerializeField]
    private ResourceManager _resourceManager;

    [Header("UI Elements")]
    [Tooltip("Slider filled 0..1 over the packing phase. Required.")]
    [SerializeField]
    private Slider _progressSlider;

    [Tooltip("Text showing the load count and start/blocked/ready messages. Required.")]
    [SerializeField]
    private TextMeshProUGUI _statusText;

    [Header("Packing")]
    [Tooltip("Maximum leaflets the station holds at once. Must be >= 1.")]
    [SerializeField]
    private int _capacity = 2;

    [Tooltip("Coins spent when packing starts. Must be >= 0.")]
    [SerializeField]
    private int _packingCost = 1;

    [Tooltip("Shortest packing time, in seconds. Must be > 0 and <= max.")]
    [SerializeField]
    private float _minPackTime = 1f;

    [Tooltip("Longest packing time, in seconds. Must be >= min.")]
    [SerializeField]
    private float _maxPackTime = 3f;

    [Header("Output")]
    [Tooltip(
        "Sealed parcel prefab (with a PackedParcel component, draggable via InteractableObject) "
            + "dispensed when the player collects. Required."
    )]
    [SerializeField]
    private GameObject _parcelPrefab;

    [Tooltip("World point the finished parcel appears at. Optional; falls back to this transform.")]
    [SerializeField]
    private Transform _dispensePoint;

    [Header("Load Points (optional)")]
    [Tooltip(
        "Parents that loaded leaflets nest under, in order. Optional; falls back to this transform."
    )]
    [SerializeField]
    private List<Transform> _leafletSlots = new List<Transform>();

    private PackingCycle _cycle;
    private readonly List<PackableLeaflet> _loadedLeaflets = new List<PackableLeaflet>();

    /// <summary>Fired when a finished parcel is dispensed to the player.</summary>
    public event Action<PackedParcel> OnParcelDispensed;

    public PackingState State => _cycle?.State ?? PackingState.Idle;
    public int LoadedCount => _cycle?.Count ?? 0;

    private void OnValidate()
    {
        if (_capacity < 1)
            _capacity = 1;
        if (_packingCost < 0)
            _packingCost = 0;
        if (_minPackTime <= 0f)
            _minPackTime = 0.01f;
        if (_maxPackTime < _minPackTime)
            _maxPackTime = _minPackTime;
    }

    private void Awake()
    {
        Assert.IsNotNull(
            _resourceManager,
            $"[{nameof(PackingStation)}] ResourceManager unassigned on {name}!"
        );
        Assert.IsNotNull(
            _progressSlider,
            $"[{nameof(PackingStation)}] Progress slider unassigned on {name}!"
        );
        Assert.IsNotNull(
            _statusText,
            $"[{nameof(PackingStation)}] Status text unassigned on {name}!"
        );
        Assert.IsNotNull(
            _parcelPrefab,
            $"[{nameof(PackingStation)}] Parcel prefab unassigned on {name}!"
        );

        _cycle = new PackingCycle(_capacity);
        _cycle.OnPackFinished += HandlePackFinished;
        _cycle.OnStateChanged += HandleStateChanged;
    }

    private void OnDestroy()
    {
        if (_cycle == null)
            return;
        _cycle.OnPackFinished -= HandlePackFinished;
        _cycle.OnStateChanged -= HandleStateChanged;
    }

    private void Start()
    {
        _progressSlider.minValue = 0f;
        _progressSlider.maxValue = 1f;
        _progressSlider.value = 0f;
        RefreshStatus();
    }

    private void Update()
    {
        _cycle.Tick(Time.deltaTime);

        if (_cycle.State == PackingState.Packing)
            _progressSlider.value = _cycle.PackProgress;
    }

    // A click starts packing while filling, or collects the package once it is ready.
    private void OnMouseDown()
    {
        if (_cycle.CanDispense)
            CollectPackage();
        else
            StartPacking();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Mirror HidingSpot/PrintLeaflet: only swallow a leaflet once the player releases the drag.
        if (Input.GetMouseButton(0))
            return;
        if (other.TryGetComponent(out PackableLeaflet leaflet))
            LoadLeaflet(leaflet);
    }

    // =====================================================================
    // Public API
    // =====================================================================

    /// <summary>Loads a dragged leaflet if the station is idle/filling and not yet full. No-op otherwise.</summary>
    public void LoadLeaflet(PackableLeaflet leaflet)
    {
        if (leaflet == null)
            return;
        if (_loadedLeaflets.Contains(leaflet))
            return;
        if (!_cycle.TryAddLeaflet())
            return;

        ParkLeaflet(leaflet, _loadedLeaflets.Count);
        _loadedLeaflets.Add(leaflet);
        RefreshStatus();
    }

    /// <summary>
    /// The Start action — a station click or a UI Button. Blocks with a message when nothing is loaded
    /// or the coin can't be paid; otherwise spends the coin, consumes the loaded leaflets, and begins
    /// packing for a random time in [<c>_minPackTime</c>, <c>_maxPackTime</c>].
    /// </summary>
    public void StartPacking()
    {
        if (!_cycle.CanStart)
        {
            ShowMessage("Włóż ulotkę do zapakowania!");
            return;
        }

        if (!_resourceManager.TrySpend(costMoney: _packingCost))
        {
            ShowMessage("Brak monety!");
            return;
        }

        ConsumeLoadedLeaflets();
        float duration = UnityEngine.Random.Range(_minPackTime, _maxPackTime);
        _cycle.TryStartPacking(duration);
        _progressSlider.value = 0f;
    }

    /// <summary>Collects the finished parcel, spawning it as a sealed, movable item for the player.</summary>
    public void CollectPackage()
    {
        int packed = _cycle.Dispense();
        if (packed <= 0)
            return;

        Vector3 position = _dispensePoint != null ? _dispensePoint.position : transform.position;
        var parcelGo = Instantiate(_parcelPrefab, position, Quaternion.identity);

        if (parcelGo.TryGetComponent(out PackedParcel parcel))
        {
            parcel.Initialize(packed);
            OnParcelDispensed?.Invoke(parcel);
        }

        RefreshStatus();
    }

    // =====================================================================
    // Cycle reactions
    // =====================================================================

    private void HandlePackFinished() => ShowMessage("Gotowe! Kliknij, aby odebrać paczkę.");

    private void HandleStateChanged(PackingState state)
    {
        if (state == PackingState.Idle || state == PackingState.Filling)
        {
            _progressSlider.value = 0f;
            RefreshStatus();
        }
    }

    // =====================================================================
    // Private helpers
    // =====================================================================

    private void ParkLeaflet(PackableLeaflet leaflet, int index)
    {
        Transform slot = index >= 0 && index < _leafletSlots.Count ? _leafletSlots[index] : null;

        if (slot != null)
        {
            Transform t = leaflet.transform;
            t.SetParent(slot, worldPositionStays: false);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }
        else
        {
            leaflet.transform.position = transform.position;
        }

        // Lock it in: no clicking or dragging it back out until it is packed.
        if (leaflet.TryGetComponent(out InteractableObject interactable))
            interactable.enabled = false;

        foreach (Collider2D col in leaflet.GetComponents<Collider2D>())
            col.enabled = false;
    }

    private void ConsumeLoadedLeaflets()
    {
        foreach (PackableLeaflet leaflet in _loadedLeaflets)
        {
            if (leaflet != null)
                Destroy(leaflet.gameObject);
        }
        _loadedLeaflets.Clear();
    }

    private void RefreshStatus()
    {
        _statusText.text = $"Ulotki: {_cycle.Count}/{_cycle.Capacity}";
        _statusText.gameObject.SetActive(true);
    }

    private void ShowMessage(string message)
    {
        _statusText.text = message;
        _statusText.gameObject.SetActive(true);
    }
}
