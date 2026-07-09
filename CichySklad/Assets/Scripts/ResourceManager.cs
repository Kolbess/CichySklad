using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Runtime view/adapter for the pure <see cref="ResourceLedger"/>. The ledger owns the accounting
/// (clamping, spend rules — unit-tested in EditMode); this component reacts to its events to spawn
/// or destroy resource prefabs, update counter text, and flash "out of resource" warnings.
///
/// De-singletoned: consumers hold a <c>[SerializeField]</c> reference instead of a static Instance.
/// </summary>
public class ResourceManager : MonoBehaviour
{
    [Header("Counter Text")]
    [Tooltip("Text showing the current paper count. Required.")]
    [FormerlySerializedAs("paperText")]
    [SerializeField]
    private TextMeshProUGUI _paperText;

    [Tooltip("Text showing the current ink count. Required.")]
    [FormerlySerializedAs("inkText")]
    [SerializeField]
    private TextMeshProUGUI _inkText;

    [Tooltip("Text showing the current leaflet count. Required.")]
    [FormerlySerializedAs("leafletsText")]
    [SerializeField]
    private TextMeshProUGUI _leafletsText;

    [Tooltip("Text showing the current money count. Required.")]
    [FormerlySerializedAs("moneyText")]
    [SerializeField]
    private TextMeshProUGUI _moneyText;

    [Tooltip("Slider showing trust as a 0..1 fraction of 100. Required.")]
    [FormerlySerializedAs("trustSlider")]
    [SerializeField]
    private Slider _trustSlider;

    [Header("Shortage Warnings (each needs an Animator)")]
    [Tooltip("Object flashed when paper is too low to spend. Required.")]
    [FormerlySerializedAs("noPaperWarning")]
    [SerializeField]
    private GameObject _noPaperWarning;

    [Tooltip("Object flashed when ink is too low to spend. Required.")]
    [FormerlySerializedAs("noInkWarning")]
    [SerializeField]
    private GameObject _noInkWarning;

    [Tooltip("Object flashed when leaflets are too low to spend. Required.")]
    [FormerlySerializedAs("noLeafletsWarning")]
    [SerializeField]
    private GameObject _noLeafletsWarning;

    [Tooltip("Object flashed when money is too low to spend. Required.")]
    [FormerlySerializedAs("noMoneyWarning")]
    [SerializeField]
    private GameObject _noMoneyWarning;

    [Header("Spawning")]
    [Tooltip("Prefab spawned/destroyed to visualise a unit of paper. Required.")]
    [FormerlySerializedAs("paperPrefab")]
    [SerializeField]
    private GameObject _paperPrefab;

    [Tooltip("Prefab spawned/destroyed to visualise a unit of ink. Required.")]
    [FormerlySerializedAs("inkPrefab")]
    [SerializeField]
    private GameObject _inkPrefab;

    [Tooltip("Prefab spawned/destroyed to visualise a unit of leaflets. Required.")]
    [FormerlySerializedAs("leafletsPrefab")]
    [SerializeField]
    private GameObject _leafletsPrefab;

    [Tooltip("Prefab spawned/destroyed to visualise a unit of money. Required.")]
    [FormerlySerializedAs("moneyPrefab")]
    [SerializeField]
    private GameObject _moneyPrefab;

    [Tooltip("Package prefab that spawned resource items are bundled into. Required.")]
    [FormerlySerializedAs("packagePrefab")]
    [SerializeField]
    private GameObject _packagePrefab;

    [Tooltip("World point where new resource packages appear. Required.")]
    [FormerlySerializedAs("resourceSpawnPoint")]
    [SerializeField]
    private Transform _resourceSpawnPoint;

    [Tooltip("Maximum resource items bundled into a single package. Must be >= 1.")]
    [FormerlySerializedAs("itemsPerPackage")]
    [SerializeField]
    private int _itemsPerPackage = 2;

    [Header("Starting Resources")]
    [Tooltip("Paper granted at the start of a run.")]
    [SerializeField]
    private int _startingPaper = 2;

    [Tooltip("Ink granted at the start of a run.")]
    [SerializeField]
    private int _startingInk = 2;

    [Tooltip("Money granted at the start of a run.")]
    [SerializeField]
    private int _startingMoney = 5;

    private static readonly int NoResource = Animator.StringToHash("NoResource");

    private readonly ResourceLedger _ledger = new ResourceLedger();

    // Every spawned physical item, per type, wherever it currently lives (in a package, loose in the
    // scene, or stashed in a HidingSpot). A spend destroys from here so nothing is left behind as a
    // "ghost" when the object has been dragged out of its original package.
    private readonly Dictionary<ResourceType, List<GameObject>> _liveItems = new Dictionary<
        ResourceType,
        List<GameObject>
    >
    {
        { ResourceType.Paper, new List<GameObject>() },
        { ResourceType.Ink, new List<GameObject>() },
        { ResourceType.Leaflets, new List<GameObject>() },
        { ResourceType.Money, new List<GameObject>() },
    };

    // Set while reconciling the ledger to items a caller already destroyed itself (NotifyConsumed),
    // so the change handler updates the counter text without destroying yet more items.
    private bool _reconciling;

    private Animator _paperAnimator;
    private Animator _inkAnimator;
    private Animator _leafletsAnimator;
    private Animator _moneyAnimator;

    public int Paper => _ledger.Paper;
    public int Ink => _ledger.Ink;
    public int Leaflets => _ledger.Leaflets;
    public int Money => _ledger.Money;
    public int Trust => _ledger.Trust;

    private void OnValidate()
    {
        if (_itemsPerPackage < 1)
            _itemsPerPackage = 1;
    }

    private void Awake()
    {
        Assert.IsNotNull(
            _resourceSpawnPoint,
            $"[{nameof(ResourceManager)}] Spawn point unassigned on {name}!"
        );
        Assert.IsNotNull(
            _packagePrefab,
            $"[{nameof(ResourceManager)}] Package prefab unassigned on {name}!"
        );
        Assert.IsNotNull(
            _noPaperWarning,
            $"[{nameof(ResourceManager)}] Paper warning unassigned on {name}!"
        );
        Assert.IsNotNull(
            _noInkWarning,
            $"[{nameof(ResourceManager)}] Ink warning unassigned on {name}!"
        );
        Assert.IsNotNull(
            _noLeafletsWarning,
            $"[{nameof(ResourceManager)}] Leaflets warning unassigned on {name}!"
        );
        Assert.IsNotNull(
            _noMoneyWarning,
            $"[{nameof(ResourceManager)}] Money warning unassigned on {name}!"
        );

        _paperAnimator = _noPaperWarning.GetComponent<Animator>();
        _inkAnimator = _noInkWarning.GetComponent<Animator>();
        _leafletsAnimator = _noLeafletsWarning.GetComponent<Animator>();
        _moneyAnimator = _noMoneyWarning.GetComponent<Animator>();

        _ledger.OnResourceChanged += HandleResourceChanged;
        _ledger.OnInsufficientResource += HandleInsufficientResource;
    }

    private void OnDestroy()
    {
        _ledger.OnResourceChanged -= HandleResourceChanged;
        _ledger.OnInsufficientResource -= HandleInsufficientResource;
    }

    private void Start()
    {
        _ledger.Paper += _startingPaper;
        _ledger.Ink += _startingInk;
        _ledger.Money += _startingMoney;
        RefreshAllText();
        UpdateTrust();
    }

    public void AddPaper(int amount) => _ledger.Paper += amount;

    public void AddInk(int amount) => _ledger.Ink += amount;

    public void AddLeaflets(int amount) => _ledger.Leaflets += amount;

    public void AddMoney(int amount) => _ledger.Money += amount;

    public void AddTrust(int amount) => _ledger.Trust += amount;

    public bool TrySpend(
        int costPaper = 0,
        int costInk = 0,
        int costLeaflets = 0,
        int costMoney = 0
    ) => _ledger.TrySpend(costPaper, costInk, costLeaflets, costMoney);

    /// <summary>
    /// Live count of spawned physical items of a resource, wherever they are (in a package, loose in
    /// the scene, or stashed in a hiding spot). Stays in sync with the ledger after spends.
    /// </summary>
    public int LiveItemCount(ResourceType type)
    {
        if (!_liveItems.TryGetValue(type, out List<GameObject> items))
            return 0;
        items.RemoveAll(item => item == null);
        return items.Count;
    }

    /// <summary>
    /// Decrements the ledger to account for physical items a station has ALREADY destroyed itself
    /// (e.g. the printer consuming its loaded paper/ink). Unlike <see cref="TrySpend"/>, this does not
    /// destroy any further items — the caller owns exactly the objects it consumed, so this only keeps
    /// the counter honest without double-spending.
    /// </summary>
    public void NotifyConsumed(ResourceType type, int count)
    {
        if (count <= 0)
            return;

        _reconciling = true;
        AdjustLedger(type, -count);
        _reconciling = false;
    }

    private void HandleResourceChanged(ResourceType type, int previous, int current)
    {
        if (type == ResourceType.Trust)
        {
            UpdateTrust();
            return;
        }

        int delta = current - previous;
        // During a reconcile the caller has already destroyed the physical items, so only refresh
        // the counter — spawning/destroying here would double-handle them.
        if (!_reconciling)
        {
            if (delta > 0)
                SpawnResource(type, delta);
            else if (delta < 0)
                DestroyResource(type, -delta);
        }

        UpdateCounterText(type, current);
    }

    private void HandleInsufficientResource(ResourceType type)
    {
        Animator animator = AnimatorFor(type);
        if (animator != null)
            animator.SetTrigger(NoResource);
    }

    private void SpawnResource(ResourceType type, int amount)
    {
        GameObject prefab = PrefabFor(type);
        if (prefab == null)
            return;

        List<GameObject> live = _liveItems[type];
        int spawned = 0;
        while (spawned < amount)
        {
            var packageGo = Instantiate(
                _packagePrefab,
                _resourceSpawnPoint.position,
                Quaternion.identity
            );
            var package = packageGo.GetComponent<Package>();

            int remaining = amount - spawned;
            int count = Mathf.Min(_itemsPerPackage, remaining);
            for (int i = 0; i < count; i++)
            {
                Vector3 offset = new Vector3(
                    Random.Range(-0.15f, 0.15f),
                    Random.Range(-0.15f, 0.15f),
                    0f
                );
                GameObject item = Instantiate(
                    prefab,
                    packageGo.transform.position + offset,
                    Quaternion.identity
                );
                package?.AddItem(item);
                live.Add(item);
            }

            spawned += count;
        }
    }

    private void DestroyResource(ResourceType type, int amount)
    {
        if (!_liveItems.TryGetValue(type, out List<GameObject> items))
            return;

        int removed = 0;
        // Newest-first so freshly spawned items are consumed before older ones. Destroying reaches
        // items wherever they sit — a HidingSpot object frees its spot via InteractableObject.OnDestroy,
        // and a bundled item leaves a null the owning Package prunes on its next click.
        for (int i = items.Count - 1; i >= 0 && removed < amount; i--)
        {
            GameObject item = items[i];
            items.RemoveAt(i);
            if (item == null)
                continue; // already destroyed elsewhere; just drop the stale entry

            Destroy(item);
            removed++;
        }
    }

    private void AdjustLedger(ResourceType type, int delta)
    {
        switch (type)
        {
            case ResourceType.Paper:
                _ledger.Paper += delta;
                break;
            case ResourceType.Ink:
                _ledger.Ink += delta;
                break;
            case ResourceType.Leaflets:
                _ledger.Leaflets += delta;
                break;
            case ResourceType.Money:
                _ledger.Money += delta;
                break;
        }
    }

    private GameObject PrefabFor(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Paper:
                return _paperPrefab;
            case ResourceType.Ink:
                return _inkPrefab;
            case ResourceType.Leaflets:
                return _leafletsPrefab;
            case ResourceType.Money:
                return _moneyPrefab;
            default:
                return null;
        }
    }

    private Animator AnimatorFor(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Paper:
                return _paperAnimator;
            case ResourceType.Ink:
                return _inkAnimator;
            case ResourceType.Leaflets:
                return _leafletsAnimator;
            case ResourceType.Money:
                return _moneyAnimator;
            default:
                return null;
        }
    }

    private void UpdateCounterText(ResourceType type, int value)
    {
        switch (type)
        {
            case ResourceType.Paper:
                _paperText.text = $"{value}";
                break;
            case ResourceType.Ink:
                _inkText.text = $"{value}";
                break;
            case ResourceType.Leaflets:
                _leafletsText.text = $"{value}";
                break;
            case ResourceType.Money:
                _moneyText.text = $"{value}";
                break;
        }
    }

    private void RefreshAllText()
    {
        _paperText.text = $"{_ledger.Paper}";
        _inkText.text = $"{_ledger.Ink}";
        _leafletsText.text = $"{_ledger.Leaflets}";
        _moneyText.text = $"{_ledger.Money}";
    }

    private void UpdateTrust()
    {
        _trustSlider.value = _ledger.Trust / 100f;
    }
}
