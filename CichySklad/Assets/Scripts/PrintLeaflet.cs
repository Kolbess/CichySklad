using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// A printing station driven by a physical material-flow loop. The player drags a paper object and
/// an ink object onto the station (detected via a trigger, the same way <see cref="HidingSpot"/>
/// swallows dropped objects), then presses Start. Printing spends the resources, runs for
/// <c>_printDuration</c> with a visible progress bar, mints a leaflet, then cools down for a shorter
/// <c>_cooldownDuration</c> before it will accept a new load. The pure <see cref="PrinterCycle"/>
/// owns the state/timing; this component owns the scene wiring, the loaded objects, and the spend.
///
/// Consumption is deliberately twofold: the loaded paper/ink GameObjects are destroyed (the visible
/// "materials disappeared" feedback) and <see cref="ResourceManager.TrySpend"/> debits the ledger
/// counters. If a scene wires the loaded objects to be the very same units the ledger tracks, set
/// the costs so the two don't double-count.
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

    [Header("Costs")]
    [Tooltip("Ink consumed per printed leaflet. Spent when Start is pressed.")]
    [FormerlySerializedAs("costInk")]
    [SerializeField]
    private int _costInk = 1;

    [Tooltip("Paper consumed per printed leaflet. Spent when Start is pressed.")]
    [FormerlySerializedAs("costPaper")]
    [SerializeField]
    private int _costPaper = 2;

    [Header("Load Points (optional)")]
    [Tooltip("World point a loaded paper object snaps to. Optional; falls back to this transform.")]
    [SerializeField]
    private Transform _paperSlot;

    [Tooltip("World point a loaded ink object snaps to. Optional; falls back to this transform.")]
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
        if (_costInk < 0)
            _costInk = 0;
        if (_costPaper < 0)
            _costPaper = 0;
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
    /// with a message when materials are missing or the ledger can't cover the cost; otherwise
    /// spends the resources, destroys the loaded objects, and begins the print phase.
    /// </summary>
    public void StartPrint()
    {
        if (!_cycle.CanStart)
        {
            ShowMessage("Załaduj papier i tusz!");
            return;
        }

        if (!_resourceManager.TrySpend(costPaper: _costPaper, costInk: _costInk))
        {
            ShowMessage("Brak materiałów!");
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
        Transform anchor = slot != null ? slot : transform;
        material.transform.position = anchor.position;

        // Freeze the object so it can't be dragged back out of the loaded slot.
        if (material.TryGetComponent(out InteractableObject interactable))
            interactable.enabled = false;
    }

    private void ConsumeLoadedMaterials()
    {
        if (_loadedPaper != null)
            Destroy(_loadedPaper.gameObject);
        if (_loadedInk != null)
            Destroy(_loadedInk.gameObject);
        _loadedPaper = null;
        _loadedInk = null;
    }

    private void ShowCost()
    {
        _costText.text = $"Tusz: {_costInk}, Papier: {_costPaper}";
        _costText.gameObject.SetActive(true);
    }

    private void ShowMessage(string message)
    {
        _costText.text = message;
        _costText.gameObject.SetActive(true);
    }
}
