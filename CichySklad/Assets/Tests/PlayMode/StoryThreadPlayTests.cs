using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// PlayMode proof for the B7 story threads: each thread's stage-1 decision is recorded in
/// <see cref="StoryState"/> and reshapes stage 2 (continuity). Drives the beats by firing their
/// <see cref="GameEvents"/> triggers directly and clicking the spawned choice buttons; the pool
/// gating that orders the stages is covered deterministically in EditMode (EventSelectorTests).
/// Starting resources after Start(): money 5, paper 2, ink 2, leaflets 0, trust 0, risk 0.
/// </summary>
public class StoryThreadPlayTests : PlayModeTestBase
{
    private const float Tol = 0.001f;

    private RiskManager _risk;
    private ResourceManager _resources;
    private StoryState _story;
    private Transform _choices;

    private IEnumerator BuildStack()
    {
        _risk = BuildRiskManager();
        _resources = BuildResourceManager();
        _story = BuildStoryState();
        DialogueSystem dialogue = BuildDialogue(out _, out _choices);
        InspectionSystem inspection = BuildInspection(_risk, out _);

        EventHandler handler = AddInactive<EventHandler>(out _, "EventHandler");
        SetField(handler, "_dialogueSystem", dialogue);
        SetField(handler, "_inspectionSystem", inspection);
        SetField(handler, "_riskManager", _risk);
        SetField(handler, "_resourceManager", _resources);
        SetField(handler, "_storyState", _story);
        Activate(handler);

        yield return null;
    }

    private void ClickChoice(int index) =>
        _choices.GetChild(index).GetComponent<Button>().onClick.Invoke();

    // ---- Maria thread ------------------------------------------------------

    [UnityTest]
    public IEnumerator Maria_HeededStageOne_LeadsToWarmRequest()
    {
        yield return BuildStack();

        GameEvents.MariaWarns();
        Assert.AreEqual(2, _choices.childCount);
        ClickChoice(0); // Zaufaj i przygotuj się
        Assert.IsTrue(_story.Has(StoryFlag.MariaStage1Done));
        Assert.IsTrue(_story.Has(StoryFlag.MariaHeeded));

        yield return null;
        GameEvents.MariaRequest();
        ClickChoice(0); // warm branch: Pomóż jej w dostawie -> trust +10
        Assert.AreEqual(10, _resources.Trust);
    }

    [UnityTest]
    public IEnumerator Maria_DismissedStageOne_LeadsToColdRequest()
    {
        yield return BuildStack();

        GameEvents.MariaWarns();
        ClickChoice(1); // Zlekceważ ostrzeżenie
        Assert.IsTrue(_story.Has(StoryFlag.MariaStage1Done));
        Assert.IsFalse(_story.Has(StoryFlag.MariaHeeded));

        yield return null;
        GameEvents.MariaRequest();
        ClickChoice(1); // cold branch: Zignoruj ją -> trust -5
        Assert.AreEqual(-5, _resources.Trust);
    }

    // ---- Kowal thread ------------------------------------------------------

    [UnityTest]
    public IEnumerator Kowal_AcceptedTask_LeadsToDistributionJob()
    {
        yield return BuildStack();

        GameEvents.LetterFromPanKowal();
        ClickChoice(0); // Podejmij zadanie -> trust +10, sets KowalAcceptedTask
        Assert.IsTrue(_story.Has(StoryFlag.KowalStage1Done));
        Assert.IsTrue(_story.Has(StoryFlag.KowalAcceptedTask));
        Assert.AreEqual(10, _resources.Trust);

        yield return null;
        GameEvents.KowalTask();
        ClickChoice(0); // accepted branch: Rozprowadź ulotki -> trust +10 (=> 20)
        Assert.AreEqual(20, _resources.Trust);
    }

    [UnityTest]
    public IEnumerator Kowal_DeclinedTask_LeadsToMinorRole()
    {
        yield return BuildStack();

        GameEvents.LetterFromPanKowal();
        ClickChoice(1); // Odmów -> trust -5, no KowalAcceptedTask
        Assert.IsFalse(_story.Has(StoryFlag.KowalAcceptedTask));
        Assert.AreEqual(-5, _resources.Trust);

        yield return null;
        GameEvents.KowalTask();
        ClickChoice(0); // declined branch: Przyjmij drobną robotę -> trust +4 (=> -1)
        Assert.AreEqual(-1, _resources.Trust);
    }

    // ---- Informer thread ---------------------------------------------------

    [UnityTest]
    public IEnumerator Informer_Appeased_VanishesQuietly()
    {
        yield return BuildStack();
        _risk.SetRisk(30f);

        GameEvents.InformerSuspicion();
        ClickChoice(0); // Przekup go (4 ruble) -> money 5->1, sets InformerAppeased
        Assert.AreEqual(1, _resources.Money);
        Assert.IsTrue(_story.Has(StoryFlag.InformerAppeased));

        yield return null;
        float before = _risk.CurrentRisk;
        GameEvents.InformerDisappears(); // appeased climax: choiceless relief, risk -10
        Assert.AreEqual(before - 10f, _risk.CurrentRisk, Tol);
        Assert.AreEqual(0, _choices.childCount);
    }

    [UnityTest]
    public IEnumerator Informer_NotAppeased_Denounces()
    {
        yield return BuildStack();

        GameEvents.InformerSuspicion();
        ClickChoice(1); // Udawaj obojętność -> no InformerAppeased
        Assert.IsFalse(_story.Has(StoryFlag.InformerAppeased));

        yield return null;
        float before = _risk.CurrentRisk;
        GameEvents.InformerDisappears(); // denounce climax: risk +30
        Assert.AreEqual(before + 30f, _risk.CurrentRisk, Tol);
    }
}
