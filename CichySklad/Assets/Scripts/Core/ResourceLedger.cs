using System;

/// <summary>
/// The five tracked resources. Used to route change/shortage notifications to the correct
/// visual (prefab pool, warning animator, counter text) in the runtime view.
/// </summary>
public enum ResourceType
{
    Paper,
    Ink,
    Leaflets,
    Money,
    Trust,
}

/// <summary>
/// Pure resource accounting: the arithmetic and spend rules for paper/ink/leaflets/money/trust
/// with no engine dependency, so it is fully unit-testable in EditMode. The runtime
/// <c>ResourceManager</c> wraps one of these and listens to its events to drive visuals.
///
/// Counted resources (paper, ink, leaflets, money) clamp at zero. Trust is a reputation gauge
/// and is intentionally left unclamped by the ledger.
/// </summary>
public class ResourceLedger
{
    private int _paper;
    private int _ink;
    private int _leaflets;
    private int _money;
    private int _trust;

    /// <summary>Fired after a value changes: (which resource, previous value, new value).</summary>
    public event Action<ResourceType, int, int> OnResourceChanged;

    /// <summary>Fired for each resource that was too low to cover a <see cref="TrySpend"/> attempt.</summary>
    public event Action<ResourceType> OnInsufficientResource;

    public int Paper
    {
        get => _paper;
        set => SetCounted(ref _paper, value, ResourceType.Paper);
    }

    public int Ink
    {
        get => _ink;
        set => SetCounted(ref _ink, value, ResourceType.Ink);
    }

    public int Leaflets
    {
        get => _leaflets;
        set => SetCounted(ref _leaflets, value, ResourceType.Leaflets);
    }

    public int Money
    {
        get => _money;
        set => SetCounted(ref _money, value, ResourceType.Money);
    }

    public int Trust
    {
        get => _trust;
        set
        {
            int previous = _trust;
            _trust = value;
            if (_trust != previous)
                OnResourceChanged?.Invoke(ResourceType.Trust, previous, _trust);
        }
    }

    /// <summary>
    /// Attempts to pay the given costs atomically. If any single resource is short, no resource
    /// is deducted, an <see cref="OnInsufficientResource"/> event fires for every shortfall, and
    /// the method returns <c>false</c>.
    /// </summary>
    public bool TrySpend(
        int costPaper = 0,
        int costInk = 0,
        int costLeaflets = 0,
        int costMoney = 0
    )
    {
        bool affordable = true;

        if (_paper < costPaper)
        {
            OnInsufficientResource?.Invoke(ResourceType.Paper);
            affordable = false;
        }
        if (_ink < costInk)
        {
            OnInsufficientResource?.Invoke(ResourceType.Ink);
            affordable = false;
        }
        if (_leaflets < costLeaflets)
        {
            OnInsufficientResource?.Invoke(ResourceType.Leaflets);
            affordable = false;
        }
        if (_money < costMoney)
        {
            OnInsufficientResource?.Invoke(ResourceType.Money);
            affordable = false;
        }

        if (!affordable)
            return false;

        Paper -= costPaper;
        Ink -= costInk;
        Leaflets -= costLeaflets;
        Money -= costMoney;
        return true;
    }

    private void SetCounted(ref int field, int value, ResourceType type)
    {
        if (value < 0)
            value = 0;

        int previous = field;
        field = value;
        if (field != previous)
            OnResourceChanged?.Invoke(type, previous, field);
    }
}
