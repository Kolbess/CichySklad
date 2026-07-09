using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// A printing station driven by a physical material-flow loop. The player drags a paper object and
/// an ink object onto the station (detected via a trigger, the same way <see cref="HidingSpot"/>
/// swallows dropped objects), then presses Start. Printing consumes exactly the one paper and one
/// ink that were loaded, runs for <c>_printDuration</c> with a visible progress bar, mints a leaflet,
/// then cools down for a shorter <c>_cooldownDuration</c> before it will accept a new load. The pure
/// <see cref="PrinterCycle"/> owns the state/timing; this component owns the scene wiring.
///
/// Consumption is single-sourced: the loaded paper/ink GameObjects are destroyed AND the ledger is
/// told about exactly those via <see cref="ResourceManager.NotifyConsumed"/> — never a separate
/// <c>TrySpend</c>, so a spend can never destroy a second, unrelated material as a side effect.
/// </summary>
public class PrintLeaflet : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Dependencies")]
    [Tooltip("Resource source that pays for and receives printed leaflets. Required.")]
    [SerializeField]
    private ResourceManager _resourceManager;

    [Header("UI Elements")]
    [Tooltip("Slider filled 0..1 over the print phase, then over the cooldown phase. Required.")]
    [FormerlySerializedAs("_cooldownSlider")]
    [FormerlySerializedAs("cooldownSlider")]
    [SerializeField]
    private Slider _progressSlider;

    [Tooltip("Text showing the ink/paper cost on hover, and load/blocked messages. Required.")]
    [FormerlySerializedAs("costText")]
    [SerializeField]
    private TextMeshProUGUI _costText;

    [Header("Timing")]
    [Tooltip("Seconds the printing phase runs. Must be > 0 and longer than the cooldown.")]
    [SerializeField]
    private float _printDuration = 6f;

    [Tooltip("Seconds of cooldown after a print. Must be > 0 and clearly shorter than the print.")]
    [FormerlySerializedAs("cooldownDuration")]
    [SerializeField]
    private float _cooldownDuration = 2f;

    [Header("Load Points (optional)")]
    [Tooltip(
        "Parent for a loaded paper object; it nests here and takes this slot's position and scale. "
            + "Optional; falls back to the printer's own position."
    )]
    [SerializeField]
    private Transform _paperSlot;

    [Tooltip(
        "Parent for a loaded ink object; it nests here and takes this slot's position and scale. "
            + "Optional; falls back to the printer's own position."
    )]
    [SerializeField]
    private Transform _inkSlot;

    private PrinterCycle _cycle;
    private PrinterMaterial _loadedPaper;
    private PrinterMaterial _loadedInk;

    public PrinterState State => _cycle?.State ?? PrinterState.Idle;
    public bool IsLoaded => _cycle != null && _cycle.IsLoaded;

    private void OnValidate()
    {
        if (_printDuration <= 0f)
            _printDuration = 0.01f;
        if (_cooldownDuration <= 0f)
            _cooldownDuration = 0.01f;
        if (_cooldownDuration >= _printDuration)
        {
            _cooldownDuration = _printDuration * 0.5f;
            Debug.LogWarning(
                $"[{nameof(PrintLeaflet)}] Cooldown must be shorter than the print; "
                    + $"clamped to {_cooldownDuration:0.##}s on {name}."
            );
        }
    }

    private void Awake()
    {
        Assert.IsNotNull(
            _resourceManager,
            $"[{nameof(PrintLeaflet)}] ResourceManager unassigned on {name}!"
        );
        Assert.IsNotNull(
            _progressSlider,
            $"[{nameof(PrintLeaflet)}] Progress slider unassigned on {name}!"
        );
        Assert.IsNotNull(_costText, $"[{nameof(PrintLeaflet)}] Cost text unassigned on {name}!");

        _cycle = new PrinterCycle(_printDuration, _cooldownDuration);
        _cycle.OnPrintFinished += HandlePrintFinished;
        _cycle.OnStateChanged += HandleStateChanged;
    }

    private void OnDestroy()
    {
        if (_cycle == null)
            return;
        _cycle.OnPrintFinished -= HandlePrintFinished;
        _cycle.OnStateChanged -= HandleStateChanged;
    }

    private void Start()
    {
        _progressSlider.minValue = 0f;
        _progressSlider.maxValue = 1f;
        _progressSlider.value = 0f;
        _costText.gameObject.SetActive(false);
    }

    private void Update()
    {
        _cycle.Tick(Time.deltaTime);

        if (_cycle.State == PrinterState.Printing)
            _progressSlider.value = _cycle.PrintProgress;
        else if (_cycle.State == PrinterState.CoolingDown)
            _progressSlider.value = _cycle.CooldownProgress;
    }

    // A click on the station is the "Start" press; a UI Button may also call StartPrint directly.
    private void OnMouseDown() => StartPrint();

    private void OnTriggerStay2D(Collider2D other)
    {
        // Mirror HidingSpot: only swallow a material once the player releases the drag over us.
        if (Input.GetMouseButton(0))
            return;
        if (other.TryGetComponent(out PrinterMaterial material))
            LoadMaterial(material);
    }

    // =====================================================================
    // Public API
    // =====================================================================

    /// <summary>
    /// Loads a dragged paper/ink object into its slot if the station is idle/loaded and that slot is
    /// still free. No-op mid-cycle or when the slot is already filled.
    /// </summary>
    public void LoadMaterial(PrinterMaterial material)
    {
        if (material == null)
            return;
        if (_cycle.State != PrinterState.Idle && _cycle.State != PrinterState.Loaded)
            return;

        switch (material.Type)
        {
            case PrinterMaterialType.Paper:
                if (_loadedPaper != null)
                    return;
                _loadedPaper = material;
                ParkMaterial(material, _paperSlot);
                _cycle.SetPaperLoaded(true);
                break;

            case PrinterMaterialType.Ink:
                if (_loadedInk != null)
                    return;
                _loadedInk = material;
                ParkMaterial(material, _inkSlot);
                _cycle.SetInkLoaded(true);
                break;
        }
    }

    /// <summary>
    /// The Start action — wire it to a UI Button or let the station's own click trigger it. Blocks
    /// with a message when the pair of materials is missing; otherwise consumes exactly the loaded
    /// paper and ink (destroying them and debiting the ledger) and begins the print phase.
    /// </summary>
    public void StartPrint()
    {
        if (!_cycle.CanStart)
        {
            ShowMessage("Załaduj papier i tusz!");
            return;
        }

        ConsumeLoadedMaterials();
        _cycle.TryStartPrinting();
        _progressSlider.value = 0f;
    }

    public void OnPointerEnter(PointerEventData eventData) => ShowCost();

    public void OnPointerExit(PointerEventData eventData) => _costText.gameObject.SetActive(false);

    // =====================================================================
    // Cycle reactions
    // =====================================================================

    private void HandlePrintFinished() => _resourceManager.AddLeaflets(1);

    private void HandleStateChanged(PrinterState state)
    {
        if (state == PrinterState.Idle || state == PrinterState.Loaded)
            _progressSlider.value = 0f;
    }

    // =====================================================================
    // Private helpers
    // =====================================================================

    private void ParkMaterial(PrinterMaterial material, Transform slot)
    {
        if (slot != null)
        {
            // Nest the material under the slot so it inherits the slot's position and scale.
            Transform t = material.transform;
            t.SetParent(slot, worldPositionStays: false);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }
        else
        {
            // No slot wired: fall back to snapping onto the printer's own position.
            material.transform.position = transform.position;
        }

        // Lock the object in: it can no longer be clicked or dragged back out. Disable the drag
        // behaviour and every collider so it stops receiving mouse and trigger events, while its
        // sprite stays visible in the printer until it is consumed at Start.
        if (material.TryGetComponent(out InteractableObject interactable))
            interactable.enabled = false;

        foreach (Collider2D col in material.GetComponents<Collider2D>())
            col.enabled = false;
    }

    private void ConsumeLoadedMaterials()
    {
        // Destroy exactly what was loaded and tell the ledger about precisely those units. Using
        // NotifyConsumed (not TrySpend) means the ledger never destroys a second, unrelated material.
        if (_loadedPaper != null)
        {
            Destroy(_loadedPaper.gameObject);
            _resourceManager.NotifyConsumed(ResourceType.Paper, 1);
        }
        if (_loadedInk != null)
        {
            Destroy(_loadedInk.gameObject);
            _resourceManager.NotifyConsumed(ResourceType.Ink, 1);
        }
        _loadedPaper = null;
        _loadedInk = null;
    }

    private void ShowCost()
    {
        _costText.text = "Koszt: 1 papier + 1 tusz";
        _costText.gameObject.SetActive(true);
    }

    private void ShowMessage(string message)
    {
        _costText.text = message;
        _costText.gameObject.SetActive(true);
    }
}
