using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// PlayMode proof for A3: every rewritten dialogue choice in <see cref="EventHandler"/> now has an
/// observable consequence. Each test triggers the event, clicks a spawned choice button, and asserts
/// the resulting risk/resource/trust change. Effects are deterministic (no <c>Random</c>), so exact
/// values hold. Starting state after Start(): money 5, paper 2, ink 2, leaflets 0, trust 0, risk 0.
/// </summary>
public class EventHandlerChoicePlayTests : PlayModeTestBase
{
    private const float Tol = 0.001f;

    private RiskManager _risk;
    private ResourceManager _resources;
    private Transform _choices;

    private IEnumerator BuildStack()
    {
        _risk = BuildRiskManager();
        _resources = BuildResourceManager();
        DialogueSystem dialogue = BuildDialogue(out _, out _choices);
        InspectionSystem inspection = BuildInspection(_risk, out _);

        EventHandler handler = AddInactive<EventHandler>(out _, "EventHandler");
        SetField(handler, "_dialogueSystem", dialogue);
        SetField(handler, "_inspectionSystem", inspection);
        SetField(handler, "_riskManager", _risk);
        SetField(handler, "_resourceManager", _resources);
        Activate(handler);

        yield return null; // let every Start() settle the starting resources and hide the box
    }

    // Invokes the spawned choice button's onClick directly — no Canvas/EventSystem needed.
    private void ClickChoice(int index)
    {
        Button button = _choices.GetChild(index).GetComponent<Button>();
        Assert.IsNotNull(button, $"No choice button at index {index}.");
        button.onClick.Invoke();
    }

    [UnityTest]
    public IEnumerator OchranaSteps_PauseCoolsRisk_WorkSpikesIt()
    {
        yield return BuildStack();
        _risk.SetRisk(30f);

        // Assert deltas around each click: RiskManager decays risk every frame, so a yield between
        // steps drifts the absolute value — capturing the baseline right before the click is decay-safe.
        GameEvents.OchranaStepsHeard();
        float b0 = _risk.CurrentRisk;
        ClickChoice(0); // Przerwij pracę -> -10
        Assert.AreEqual(b0 - 10f, _risk.CurrentRisk, Tol);

        yield return null;
        GameEvents.OchranaStepsHeard();
        float b1 = _risk.CurrentRisk;
        ClickChoice(1); // Pracuj dalej -> +12
        Assert.AreEqual(b1 + 12f, _risk.CurrentRisk, Tol);
    }

    [UnityTest]
    public IEnumerator OutOfInk_UseNowPrints_SaveKeepsInk()
    {
        yield return BuildStack();

        GameEvents.OutOfInk();
        ClickChoice(0); // Zużyj resztki tuszu -> +2 leaflets
        Assert.AreEqual(2, _resources.Leaflets);

        yield return null;
        GameEvents.OutOfInk();
        ClickChoice(1); // Oszczędzaj tusz -> +1 ink
        Assert.AreEqual(3, _resources.Ink);
    }

    [UnityTest]
    public IEnumerator LostPaper_PayBuysBatch_IgnoreRaisesRisk()
    {
        yield return BuildStack();

        GameEvents.LostPaperBatch();
        ClickChoice(0); // Zapłać donosicielowi (3 ruble) -> money 5->2, paper 2->5
        Assert.AreEqual(2, _resources.Money);
        Assert.AreEqual(5, _resources.Paper);

        yield return null;
        float before = _risk.CurrentRisk;
        GameEvents.LostPaperBatch();
        ClickChoice(1); // Odpuść -> +5 risk
        Assert.AreEqual(before + 5f, _risk.CurrentRisk, Tol);
    }

    [UnityTest]
    public IEnumerator MoistureDamage_ThrowSpendsPaper_UseMakesRiskyLeaflets()
    {
        yield return BuildStack();

        GameEvents.MoistureDamage();
        ClickChoice(0); // Wyrzuć zniszczony papier -> paper 2->1
        Assert.AreEqual(1, _resources.Paper);

        yield return null;
        float before = _risk.CurrentRisk;
        GameEvents.MoistureDamage();
        ClickChoice(1); // Użyj wilgotnego papieru -> +2 leaflets, +8 risk
        Assert.AreEqual(2, _resources.Leaflets);
        Assert.AreEqual(before + 8f, _risk.CurrentRisk, Tol);
    }

    [UnityTest]
    public IEnumerator SecretDonation_TakePaysButRisks_LeaveEarnsTrust()
    {
        yield return BuildStack();

        GameEvents.SecretDonation();
        ClickChoice(0); // Weź datek -> money +5, risk +8
        Assert.AreEqual(10, _resources.Money);
        Assert.AreEqual(8f, _risk.CurrentRisk, Tol);

        yield return null;
        GameEvents.SecretDonation();
        ClickChoice(1); // Zostaw -> trust +5
        Assert.AreEqual(5, _resources.Trust);
    }

    [UnityTest]
    public IEnumerator NeighborSawCourier_BribeBuysSilence_DoNothingCostsTrust()
    {
        yield return BuildStack();
        _risk.SetRisk(30f);

        GameEvents.NeighborSawCourier();
        ClickChoice(0); // Przekup sąsiada (3 ruble) -> money 5->2, risk -10
        Assert.AreEqual(2, _resources.Money);
        Assert.AreEqual(20f, _risk.CurrentRisk, Tol);

        yield return null;
        GameEvents.NeighborSawCourier();
        ClickChoice(1); // Nic nie rób -> trust -5 (unclamped)
        Assert.AreEqual(-5, _resources.Trust);
    }

    [UnityTest]
    public IEnumerator InformerAsks_AllThreeChoicesMoveRisk()
    {
        yield return BuildStack();
        _risk.SetRisk(20f);

        GameEvents.InformerAsks();
        float b0 = _risk.CurrentRisk;
        ClickChoice(0); // Skłam -> -5
        Assert.AreEqual(b0 - 5f, _risk.CurrentRisk, Tol);

        yield return null;
        GameEvents.InformerAsks();
        float b1 = _risk.CurrentRisk;
        ClickChoice(1); // Odpraw go -> +5
        Assert.AreEqual(b1 + 5f, _risk.CurrentRisk, Tol);

        yield return null;
        GameEvents.InformerAsks();
        float b2 = _risk.CurrentRisk;
        ClickChoice(2); // Zignoruj -> +8
        Assert.AreEqual(b2 + 8f, _risk.CurrentRisk, Tol);
    }

    [UnityTest]
    public IEnumerator PackageUncertain_OpenYieldsSupplies_WaitCoolsRisk()
    {
        yield return BuildStack();
        _risk.SetRisk(20f);

        GameEvents.PackageUncertain();
        float b0 = _risk.CurrentRisk;
        ClickChoice(0); // Otwórz paczkę -> paper +2, ink +1, risk +5
        Assert.AreEqual(4, _resources.Paper);
        Assert.AreEqual(3, _resources.Ink);
        Assert.AreEqual(b0 + 5f, _risk.CurrentRisk, Tol);

        yield return null;
        GameEvents.PackageUncertain();
        float b1 = _risk.CurrentRisk;
        ClickChoice(1); // Poczekaj -> -5 risk
        Assert.AreEqual(b1 - 5f, _risk.CurrentRisk, Tol);
    }

    [UnityTest]
    public IEnumerator StuckHidingSpot_BothChoicesRaiseRisk()
    {
        yield return BuildStack();

        GameEvents.StuckHidingSpot();
        float b0 = _risk.CurrentRisk;
        ClickChoice(0); // Szarp na siłę -> +10
        Assert.AreEqual(b0 + 10f, _risk.CurrentRisk, Tol);

        yield return null;
        GameEvents.StuckHidingSpot();
        float b1 = _risk.CurrentRisk;
        ClickChoice(1); // Zostaw zacięty schowek -> +4
        Assert.AreEqual(b1 + 4f, _risk.CurrentRisk, Tol);
    }

    [UnityTest]
    public IEnumerator StrangerNeedsHelp_ShareEarnsTrust_DismissCostsTrust()
    {
        yield return BuildStack();

        GameEvents.StrangerNeedsHelp();
        ClickChoice(0); // Podziel się zasobami (2 ruble) -> money 5->3, trust +8
        Assert.AreEqual(3, _resources.Money);
        Assert.AreEqual(8, _resources.Trust);

        yield return null;
        GameEvents.StrangerNeedsHelp();
        ClickChoice(1); // Odpraw nieznajomego -> trust -5 (8 -> 3)
        Assert.AreEqual(3, _resources.Trust);
    }

    [UnityTest]
    public IEnumerator BuyPaperOffer_InvestBuysPaper_DeclineEarnsTrust()
    {
        yield return BuildStack();

        GameEvents.BuyPaperOffer();
        ClickChoice(0); // Zainwestuj (4 ruble) -> money 5->1, paper 2->8
        Assert.AreEqual(1, _resources.Money);
        Assert.AreEqual(8, _resources.Paper);

        yield return null;
        GameEvents.BuyPaperOffer();
        ClickChoice(1); // Odrzuć ofertę -> trust +2
        Assert.AreEqual(2, _resources.Trust);
    }

    [UnityTest]
    public IEnumerator OchranaRaid_BribeBuysSafety_SubmitCostsRiskAndTrust()
    {
        yield return BuildStack();
        _risk.SetRisk(30f);

        GameEvents.OchranaRaid();
        float b0 = _risk.CurrentRisk;
        ClickChoice(0); // Wręcz łapówkę (5 rubli) -> money 5->0, risk -20
        Assert.AreEqual(0, _resources.Money);
        Assert.AreEqual(b0 - 20f, _risk.CurrentRisk, Tol);

        yield return null;
        GameEvents.OchranaRaid();
        float b1 = _risk.CurrentRisk;
        ClickChoice(1); // Poddaj się rewizji -> risk +10, trust -3
        Assert.AreEqual(b1 + 10f, _risk.CurrentRisk, Tol);
        Assert.AreEqual(-3, _resources.Trust);
    }

    [UnityTest]
    public IEnumerator LoudNoise_HideLowersRisk_IgnoreRaisesIt()
    {
        yield return BuildStack();
        _risk.SetRisk(20f);

        GameEvents.LoudNoise();
        float b0 = _risk.CurrentRisk;
        ClickChoice(0); // Błyskawicznie chowaj sprzęt -> -8
        Assert.AreEqual(b0 - 8f, _risk.CurrentRisk, Tol);

        yield return null;
        GameEvents.LoudNoise();
        float b1 = _risk.CurrentRisk;
        ClickChoice(1); // Udawaj, że nic się nie stało -> +10
        Assert.AreEqual(b1 + 10f, _risk.CurrentRisk, Tol);
    }
}
