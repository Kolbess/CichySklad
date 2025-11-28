using UnityEngine;
using UnityEngine.UI;

public class RiskUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider riskSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private GameObject alertObject;

    [Header("Colors")]
    [SerializeField] private Color lowRiskColor = Color.green;
    [SerializeField] private Color mediumRiskColor = new Color(1f, 0.5f, 0f); // Orange
    [SerializeField] private Color highRiskColor = Color.red;
    [SerializeField] private Color criticalRiskColor = new Color(0.5f, 0f, 0f); // Dark Red

    private void Start()
    {
        if (RiskManager.Instance != null)
        {
            RiskManager.Instance.OnRiskChanged += UpdateRiskUI;
            RiskManager.Instance.OnRiskLevelChanged += UpdateRiskLevelUI;
            
            // Initialize UI
            UpdateRiskUI(RiskManager.Instance.CurrentRisk);
            UpdateRiskLevelUI(RiskLevel.Low); // Default, or get from manager if exposed
        }
        
        if (alertObject != null)
            alertObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (RiskManager.Instance != null)
        {
            RiskManager.Instance.OnRiskChanged -= UpdateRiskUI;
            RiskManager.Instance.OnRiskLevelChanged -= UpdateRiskLevelUI;
        }
    }

    private void UpdateRiskUI(float currentRisk)
    {
        if (riskSlider != null)
        {
            riskSlider.value = currentRisk / RiskManager.Instance.MaxRisk;
        }
    }

    private void UpdateRiskLevelUI(RiskLevel level)
    {
        if (fillImage != null)
        {
            switch (level)
            {
                case RiskLevel.Low:
                    fillImage.color = lowRiskColor;
                    break;
                case RiskLevel.Medium:
                    fillImage.color = mediumRiskColor;
                    break;
                case RiskLevel.High:
                    fillImage.color = highRiskColor;
                    break;
                case RiskLevel.Critical:
                    fillImage.color = criticalRiskColor;
                    break;
            }
        }

        if (alertObject != null)
        {
            alertObject.SetActive(level == RiskLevel.Critical);
        }
    }
}
