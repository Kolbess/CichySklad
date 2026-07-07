using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
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
    private int _itemsPerPackage = 3;

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
    private readonly List<Package> _packages = new List<Package>();

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

    private void Update()
    {
        // Debug/testing hotkeys retained from the original build.
        if (Input.GetKeyDown(KeyCode.Alpha1))
            GameEvents.RumorsSpread();

        if (Input.GetKeyDown(KeyCode.Z))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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

    private void HandleResourceChanged(ResourceType type, int previous, int current)
    {
        if (type == ResourceType.Trust)
        {
            UpdateTrust();
            return;
        }

        int delta = current - previous;
        GameObject prefab = PrefabFor(type);
        if (delta > 0)
            SpawnResource(prefab, delta);
        else if (delta < 0)
            DestroyResource(prefab, -delta);

        UpdateCounterText(type, current);
    }

    private void HandleInsufficientResource(ResourceType type)
    {
        Animator animator = AnimatorFor(type);
        if (animator != null)
            animator.SetTrigger(NoResource);
    }

    private void SpawnResource(GameObject prefab, int amount)
    {
        int spawned = 0;
        while (spawned < amount)
        {
            var packageGo = Instantiate(
                _packagePrefab,
                _resourceSpawnPoint.position,
                Quaternion.identity
            );
            var package = packageGo.GetComponent<Package>();
            if (package != null)
                _packages.Add(package);

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
            }

            spawned += count;
        }
    }

    private void DestroyResource(GameObject prefab, int amount)
    {
        int toRemove = amount;

        for (int i = _packages.Count - 1; i >= 0 && toRemove > 0; i--)
        {
            Package package = _packages[i];
            if (package == null)
            {
                _packages.RemoveAt(i);
                continue;
            }

            List<GameObject> items = package.Items;
            for (int j = items.Count - 1; j >= 0 && toRemove > 0; j--)
            {
                if (items[j].name.Contains(prefab.name))
                {
                    Destroy(items[j]);
                    items.RemoveAt(j);
                    toRemove--;
                }
            }
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
