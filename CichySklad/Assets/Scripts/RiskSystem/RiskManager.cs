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
    [SerializeField] private float riskDecayRate = 1f; // Risk reduced per second automatically (optional)

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
        // Optional: Decay risk over time if needed
        // ReduceRisk(riskDecayRate * Time.deltaTime);
    }

    public void AddRisk(float amount)
    {
        currentRisk = Mathf.Clamp(currentRisk + amount, 0, maxRisk);
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
