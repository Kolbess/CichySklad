using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

/// <summary>
/// Runtime adapter that turns the pure <see cref="EventSelector"/> decisions into fired
/// <see cref="GameEvents"/> beats. Each day the day loop asks it to (a) schedule one narrative event
/// picked by weight from the pool, respecting day / risk band / one-shot rules, and (b) grant a
/// random resource windfall. This is the single place the daily pool lives — adding an event needs a
/// new <see cref="GameEventId"/>, a line in <see cref="GameEventPool"/>, and a trigger below; the
/// day loop never changes.
/// </summary>
public class EventScheduler : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Resource source granted the daily windfall. Required.")]
    [SerializeField]
    private ResourceManager _resourceManager;

    [Tooltip("Story-flag store consulted to gate multi-stage story beats. Required.")]
    [SerializeField]
    private StoryState _storyState;

    // Maps every schedulable id to the GameEvents beat it raises. Static: GameEvents methods are
    // static, and Awake asserts this covers the whole GameEventId enum so a selected id can never
    // land on a missing trigger.
    private static readonly IReadOnlyDictionary<GameEventId, Action> Triggers = new Dictionary<
        GameEventId,
        Action
    >
    {
        { GameEventId.KnockAtDoor, GameEvents.KnockAtDoor },
        { GameEventId.NeighborPeeking, GameEvents.NeighborPeeking },
        { GameEventId.OchranaStepsHeard, GameEvents.OchranaStepsHeard },
        { GameEventId.OchranaRaid, GameEvents.OchranaRaid },
        { GameEventId.OutOfInk, GameEvents.OutOfInk },
        { GameEventId.LostPaperBatch, GameEvents.LostPaperBatch },
        { GameEventId.MoistureDamage, GameEvents.MoistureDamage },
        { GameEventId.SecretDonation, GameEvents.SecretDonation },
        { GameEventId.NeighborSawCourier, GameEvents.NeighborSawCourier },
        { GameEventId.InformerAsks, GameEvents.InformerAsks },
        { GameEventId.RumorsSpread, GameEvents.RumorsSpread },
        { GameEventId.CourierInjured, GameEvents.CourierInjured },
        { GameEventId.UrgentDelivery, GameEvents.UrgentDelivery },
        { GameEventId.PackageUncertain, GameEvents.PackageUncertain },
        { GameEventId.StuckHidingSpot, GameEvents.StuckHidingSpot },
        { GameEventId.StrangerNeedsHelp, GameEvents.StrangerNeedsHelp },
        { GameEventId.LampExplosion, GameEvents.LampExplosion },
        { GameEventId.MariaWarns, GameEvents.MariaWarns },
        { GameEventId.MariaRequest, GameEvents.MariaRequest },
        { GameEventId.LetterFromPanKowal, GameEvents.LetterFromPanKowal },
        { GameEventId.KowalTask, GameEvents.KowalTask },
        { GameEventId.InformerSuspicion, GameEvents.InformerSuspicion },
        { GameEventId.InformerDisappears, GameEvents.InformerDisappears },
        { GameEventId.LoudNoise, GameEvents.LoudNoise },
        { GameEventId.FireCandle, GameEvents.FireCandle },
        { GameEventId.BrokenLock, GameEvents.BrokenLock },
        { GameEventId.BuyPaperOffer, GameEvents.BuyPaperOffer },
    };

    private List<GameEventDefinition> _pool;
    private readonly HashSet<GameEventId> _firedOnceEvents = new HashSet<GameEventId>();
    private Action[] _windfalls;

    /// <summary>Raised after a narrative event is scheduled, with the id that fired (for logging/tests).</summary>
    public event Action<GameEventId> OnEventScheduled;

    private void Awake()
    {
        Assert.IsNotNull(
            _resourceManager,
            $"[{nameof(EventScheduler)}] ResourceManager unassigned on {name}!"
        );
        Assert.IsNotNull(
            _storyState,
            $"[{nameof(EventScheduler)}] StoryState unassigned on {name}!"
        );

        foreach (GameEventId id in Enum.GetValues(typeof(GameEventId)))
            Assert.IsTrue(
                Triggers.ContainsKey(id),
                $"[{nameof(EventScheduler)}] No trigger mapped for {id}!"
            );

        _pool = GameEventPool.Default();

        // Silent resource windfalls (no dialogue). One entry per outcome; the table length is the
        // roll's exclusive bound (see RandomEventTable), so every windfall stays reachable.
        _windfalls = new Action[]
        {
            () => _resourceManager.AddMoney(3),
            () => _resourceManager.AddPaper(3),
            () => _resourceManager.AddInk(3),
            () => _resourceManager.AddLeaflets(3),
            () => _resourceManager.AddTrust(10),
        };
    }

    /// <summary>
    /// Rolls one narrative event eligible on <paramref name="day"/> at <paramref name="riskLevel"/>
    /// and fires it, recording one-shot beats so they never repeat. Returns false when nothing is
    /// eligible (e.g. every candidate is a spent one-shot), in which case no event fires.
    /// </summary>
    public bool TryScheduleDailyEvent(int day, RiskLevel riskLevel)
    {
        List<GameEventDefinition> eligible = EventSelector.Eligible(
            _pool,
            day,
            riskLevel,
            _firedOnceEvents,
            _storyState.ActiveFlags
        );
        if (eligible.Count == 0)
            return false;

        int roll = Random.Range(0, EventSelector.TotalWeight(eligible));
        GameEventDefinition chosen = EventSelector.PickWeighted(eligible, roll);

        if (chosen.Once)
            _firedOnceEvents.Add(chosen.Id);

        Triggers[chosen.Id].Invoke();
        OnEventScheduled?.Invoke(chosen.Id);
        return true;
    }

    /// <summary>Grants one random resource windfall (money / paper / ink / leaflets / trust).</summary>
    public void GrantDailyWindfall()
    {
        int roll = Random.Range(0, RandomEventTable.RollBound(_windfalls));
        RandomEventTable.Select(_windfalls, roll).Invoke();
    }

    /// <summary>Clears one-shot history so a fresh run can roll story beats again.</summary>
    public void ResetSchedule() => _firedOnceEvents.Clear();
}
