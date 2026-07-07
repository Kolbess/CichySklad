using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Presents the current risk on a slider and recolours it per band, showing an alert object when
/// risk is Critical. Listens to an injected <see cref="RiskManager"/>; holds no reference to any
/// other system.
/// </summary>
public class RiskUI : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Risk source this UI reflects. Required — assign the scene's RiskManager.")]
    [SerializeField]
    private RiskManager _riskManager;

    [Header("UI References")]
    [Tooltip("Slider showing normalized risk (0..1). Required.")]
    [FormerlySerializedAs("riskSlider")]
    [SerializeField]
    private Slider _riskSlider;

    [Tooltip("Fill image recoloured to match the current risk band. Required.")]
    [FormerlySerializedAs("fillImage")]
    [SerializeField]
    private Image _fillImage;

    [Tooltip("Object shown only while risk is Critical (e.g. flashing alert). Required.")]
    [FormerlySerializedAs("alertObject")]
    [SerializeField]
    private GameObject _alertObject;

    [Header("Band Colors")]
    [Tooltip("Fill color while in the Low band.")]
    [FormerlySerializedAs("lowRiskColor")]
    [SerializeField]
    private Color _lowRiskColor = Color.green;

    [Tooltip("Fill color while in the Medium band.")]
    [FormerlySerializedAs("mediumRiskColor")]
    [SerializeField]
    private Color _mediumRiskColor = new Color(1f, 0.5f, 0f);

    [Tooltip("Fill color while in the High band.")]
    [FormerlySerializedAs("highRiskColor")]
    [SerializeField]
    private Color _highRiskColor = Color.red;

    [Tooltip("Fill color while in the Critical band.")]
    [FormerlySerializedAs("criticalRiskColor")]
    [SerializeField]
    private Color _criticalRiskColor = new Color(0.5f, 0f, 0f);

    private void Awake()
    {
        Assert.IsNotNull(_riskManager, $"[{nameof(RiskUI)}] RiskManager unassigned on {name}!");
        Assert.IsNotNull(_riskSlider, $"[{nameof(RiskUI)}] Risk Slider unassigned on {name}!");
        Assert.IsNotNull(_fillImage, $"[{nameof(RiskUI)}] Fill Image unassigned on {name}!");
        Assert.IsNotNull(_alertObject, $"[{nameof(RiskUI)}] Alert Object unassigned on {name}!");
    }

    private void OnEnable()
    {
        _riskManager.OnRiskChanged += UpdateRiskUI;
        _riskManager.OnRiskLevelChanged += UpdateRiskLevelUI;

        UpdateRiskUI(_riskManager.CurrentRisk);
        UpdateRiskLevelUI(_riskManager.CurrentRiskLevel);
    }

    private void OnDisable()
    {
        _riskManager.OnRiskChanged -= UpdateRiskUI;
        _riskManager.OnRiskLevelChanged -= UpdateRiskLevelUI;
    }

    private void UpdateRiskUI(float currentRisk)
    {
        _riskSlider.value = currentRisk / _riskManager.MaxRisk;
    }

    private void UpdateRiskLevelUI(RiskLevel level)
    {
        _fillImage.color = ColorForLevel(level);
        _alertObject.SetActive(level == RiskLevel.Critical);
    }

    private Color ColorForLevel(RiskLevel level)
    {
        switch (level)
        {
            case RiskLevel.Low:
                return _lowRiskColor;
            case RiskLevel.Medium:
                return _mediumRiskColor;
            case RiskLevel.High:
                return _highRiskColor;
            case RiskLevel.Critical:
                return _criticalRiskColor;
            default:
                return _lowRiskColor;
        }
    }
}
