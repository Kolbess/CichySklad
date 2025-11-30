using System;
using UnityEngine;

public class RiskManager : MonoBehaviour
{
    public static RiskManager Instance { get; private set; }

    [Header("Risk Settings")]
    [Range(0, 100)]
    [SerializeField] private float currentRisk = 0f;
    [SerializeField] private float maxRisk = 100f;
    
    [Header("Thresholds")]
    [SerializeField] private float mediumRiskThreshold = 30f;
    [SerializeField] private float highRiskThreshold = 70f;
    [SerializeField] private float criticalRiskThreshold = 90f;

    [Header("Decay")]
    [SerializeField] private float riskDecayRate = 1f; // Risk reduced per second automatically
    
    [Header("Decay Multipliers")]
    [SerializeField] private float lowRiskDecayMultiplier = 1.0f;
    [SerializeField] private float mediumRiskDecayMultiplier = 0.75f;
    [SerializeField] private float highRiskDecayMultiplier = 0.5f;
    [SerializeField] private float criticalRiskDecayMultiplier = 0.25f;

    public event Action<float> OnRiskChanged;
    public event Action<RiskLevel> OnRiskLevelChanged;

    public float CurrentRisk => currentRisk;
    public float MaxRisk => maxRisk;

    private RiskLevel _currentRiskLevel = RiskLevel.Low;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Decay risk over time with progressive multiplier
        if (currentRisk > 0)
        {
            float multiplier = GetDecayMultiplier();
            ReduceRisk(riskDecayRate * multiplier * Time.deltaTime);
        }
    }

    private float GetDecayMultiplier()
    {
        switch (_currentRiskLevel)
        {
            case RiskLevel.Low:
                return lowRiskDecayMultiplier;
            case RiskLevel.Medium:
                return mediumRiskDecayMultiplier;
            case RiskLevel.High:
                return highRiskDecayMultiplier;
            case RiskLevel.Critical:
                return criticalRiskDecayMultiplier;
            default:
                return 1.0f;
        }
    }

    public void AddRisk(float amount)
    {
        currentRisk = Mathf.Clamp(currentRisk + amount, 0, maxRisk);
        OnRiskChanged?.Invoke(currentRisk);
        CheckRiskLevel();
    }

    public void SetRisk(float amount)
    {
        currentRisk = Mathf.Clamp(amount, 0, maxRisk);
        OnRiskChanged?.Invoke(currentRisk);
        CheckRiskLevel();
    }

    public void ReduceRisk(float amount)
    {
        currentRisk = Mathf.Clamp(currentRisk - amount, 0, maxRisk);
        OnRiskChanged?.Invoke(currentRisk);
        CheckRiskLevel();
    }

    private void CheckRiskLevel()
    {
        RiskLevel newLevel;

        if (currentRisk >= criticalRiskThreshold)
        {
            newLevel = RiskLevel.Critical;
        }
        else if (currentRisk >= highRiskThreshold)
        {
            newLevel = RiskLevel.High;
        }
        else if (currentRisk >= mediumRiskThreshold)
        {
            newLevel = RiskLevel.Medium;
        }
        else
        {
            newLevel = RiskLevel.Low;
        }

        if (newLevel != _currentRiskLevel)
        {
            _currentRiskLevel = newLevel;
            OnRiskLevelChanged?.Invoke(_currentRiskLevel);
            Debug.Log($"Risk Level Changed to: {_currentRiskLevel}");
        }
    }
}
