using System;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Owns the runtime risk value and its automatic per-frame decay. A plain scene component
/// (no singleton): systems that need it hold a <c>[SerializeField]</c> reference. All banding
/// and decay maths are delegated to the pure, tested <see cref="RiskCalculator"/>.
/// </summary>
public class RiskManager : MonoBehaviour
{
    [Header("Risk State")]
    [Range(0, 100)]
    [Tooltip("Current accumulated risk, 0 = safe. Clamped to [0, Max Risk] at runtime.")]
    [FormerlySerializedAs("currentRisk")]
    [SerializeField]
    private float _currentRisk = 0f;

    [Tooltip("Upper bound for risk. Must be greater than 0.")]
    [FormerlySerializedAs("maxRisk")]
    [SerializeField]
    private float _maxRisk = 100f;

    [Header("Band Thresholds (inclusive lower bounds)")]
    [Tooltip("Risk at or above this enters the Medium band.")]
    [FormerlySerializedAs("mediumRiskThreshold")]
    [SerializeField]
    private float _mediumRiskThreshold = 30f;

    [Tooltip("Risk at or above this enters the High band.")]
    [FormerlySerializedAs("highRiskThreshold")]
    [SerializeField]
    private float _highRiskThreshold = 70f;

    [Tooltip("Risk at or above this enters the Critical band.")]
    [FormerlySerializedAs("criticalRiskThreshold")]
    [SerializeField]
    private float _criticalRiskThreshold = 90f;

    [Header("Decay")]
    [Tooltip("Base risk removed per second before the band multiplier is applied.")]
    [FormerlySerializedAs("riskDecayRate")]
    [SerializeField]
    private float _riskDecayRate = 1f;

    [Space]
    [Tooltip("Decay multiplier while in the Low band (1 = full base rate).")]
    [FormerlySerializedAs("lowRiskDecayMultiplier")]
    [SerializeField]
    private float _lowRiskDecayMultiplier = 1.0f;

    [Tooltip("Decay multiplier while in the Medium band.")]
    [FormerlySerializedAs("mediumRiskDecayMultiplier")]
    [SerializeField]
    private float _mediumRiskDecayMultiplier = 0.75f;

    [Tooltip("Decay multiplier while in the High band.")]
    [FormerlySerializedAs("highRiskDecayMultiplier")]
    [SerializeField]
    private float _highRiskDecayMultiplier = 0.5f;

    [Tooltip("Decay multiplier while in the Critical band (risk sticks longest here).")]
    [FormerlySerializedAs("criticalRiskDecayMultiplier")]
    [SerializeField]
    private float _criticalRiskDecayMultiplier = 0.25f;

    [Header("Decay Freeze")]
    [Tooltip(
        "Seconds risk is held after ANY increase before automatic decay resumes; another increase "
            + "refreshes it. Explicit ReduceRisk (player choices) ignores this. 0 = decay immediately."
    )]
    [SerializeField]
    private float _decayFreezeSeconds = 15f;

    private RiskLevel _currentRiskLevel = RiskLevel.Low;

    // Seconds since risk last went up. Starts large so decay is allowed until the first increase
    // resets it to zero (which begins the hold).
    private float _timeSinceIncrease = float.MaxValue;

    /// <summary>Raised whenever the numeric risk value changes.</summary>
    public event Action<float> OnRiskChanged;

    /// <summary>Raised only when the risk crosses into a different <see cref="RiskLevel"/> band.</summary>
    public event Action<RiskLevel> OnRiskLevelChanged;

    public float CurrentRisk => _currentRisk;
    public float MaxRisk => _maxRisk;
    public RiskLevel CurrentRiskLevel => _currentRiskLevel;

    private void OnEnable() => GameEvents.OnRiskDelta += AddRisk;

    private void OnDisable() => GameEvents.OnRiskDelta -= AddRisk;

    private void OnValidate()
    {
        if (_maxRisk <= 0f)
            _maxRisk = 1f;

        _currentRisk = Mathf.Clamp(_currentRisk, 0f, _maxRisk);

        // Keep thresholds monotonically ordered so banding stays sane as the designer edits.
        _mediumRiskThreshold = Mathf.Clamp(_mediumRiskThreshold, 0f, _maxRisk);
        _highRiskThreshold = Mathf.Clamp(_highRiskThreshold, _mediumRiskThreshold, _maxRisk);
        _criticalRiskThreshold = Mathf.Clamp(_criticalRiskThreshold, _highRiskThreshold, _maxRisk);

        if (_riskDecayRate < 0f)
            _riskDecayRate = 0f;

        if (_decayFreezeSeconds < 0f)
            _decayFreezeSeconds = 0f;
    }

    private void Update()
    {
        _timeSinceIncrease += Time.deltaTime;

        if (_currentRisk <= 0f)
            return;

        // Hold the risk for a beat after any increase so the spike is felt before it bleeds off.
        if (!RiskCalculator.CanDecay(_timeSinceIncrease, _decayFreezeSeconds))
            return;

        float multiplier = RiskCalculator.DecayMultiplier(
            _currentRiskLevel,
            _lowRiskDecayMultiplier,
            _mediumRiskDecayMultiplier,
            _highRiskDecayMultiplier,
            _criticalRiskDecayMultiplier
        );
        ReduceRisk(_riskDecayRate * multiplier * Time.deltaTime);
    }

    public void AddRisk(float amount) => SetRisk(_currentRisk + amount);

    public void ReduceRisk(float amount) => SetRisk(_currentRisk - amount);

    public void SetRisk(float amount)
    {
        float previous = _currentRisk;
        _currentRisk = Mathf.Clamp(amount, 0f, _maxRisk);

        // Any genuine increase (re)starts the decay-freeze hold; decreases (decay, player choices)
        // leave it running so explicit reductions are never blocked.
        if (_currentRisk > previous)
            _timeSinceIncrease = 0f;

        OnRiskChanged?.Invoke(_currentRisk);
        CheckRiskLevel();
    }

    private void CheckRiskLevel()
    {
        RiskLevel newLevel = RiskCalculator.DetermineLevel(
            _currentRisk,
            _mediumRiskThreshold,
            _highRiskThreshold,
            _criticalRiskThreshold
        );

        if (newLevel == _currentRiskLevel)
            return;

        _currentRiskLevel = newLevel;
        OnRiskLevelChanged?.Invoke(_currentRiskLevel);
    }
}
