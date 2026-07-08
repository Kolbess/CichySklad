using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>PlayMode coverage for <see cref="RiskUI"/>: it mirrors the risk value onto a slider and
/// recolours the fill per band, showing the alert object only while risk is Critical.</summary>
public class RiskUiPlayTests : PlayModeTestBase
{
    private readonly Color _low = Color.green;
    private readonly Color _critical = new Color(0.5f, 0f, 0f);

    private RiskUI BuildRiskUi(
        RiskManager risk,
        out Slider slider,
        out Image fill,
        out GameObject alert
    )
    {
        RiskUI ui = AddInactive<RiskUI>(out _, "RiskUI");
        slider = NewSlider();
        fill = NewImage();
        alert = NewGo("Alert");

        SetField(ui, "_riskManager", risk);
        SetField(ui, "_riskSlider", slider);
        SetField(ui, "_fillImage", fill);
        SetField(ui, "_alertObject", alert);
        SetField(ui, "_lowRiskColor", _low);
        SetField(ui, "_criticalRiskColor", _critical);

        Activate(ui);
        return ui;
    }

    [UnityTest]
    public IEnumerator RiskChange_UpdatesSliderFraction()
    {
        RiskManager risk = BuildRiskManager();
        BuildRiskUi(risk, out Slider slider, out _, out _);
        yield return null;

        risk.SetRisk(50f); // MaxRisk defaults to 100

        Assert.AreEqual(0.5f, slider.value, 0.001f);
    }

    [UnityTest]
    public IEnumerator CriticalBand_RecoloursFillAndShowsAlert()
    {
        RiskManager risk = BuildRiskManager();
        BuildRiskUi(risk, out _, out Image fill, out GameObject alert);
        yield return null;

        risk.SetRisk(95f); // default Critical threshold is 90

        Assert.AreEqual(_critical, fill.color);
        Assert.IsTrue(alert.activeSelf);
    }

    [UnityTest]
    public IEnumerator LowBand_HidesAlert_AndUsesLowColor()
    {
        RiskManager risk = BuildRiskManager();
        BuildRiskUi(risk, out _, out Image fill, out GameObject alert);
        yield return null;

        risk.SetRisk(95f);
        risk.SetRisk(0f); // back to Low

        Assert.AreEqual(_low, fill.color);
        Assert.IsFalse(alert.activeSelf);
    }
}
