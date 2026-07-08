using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>PlayMode coverage for <see cref="EventHandler"/>: it translates <see cref="GameEvents"/>
/// beats into dialogue prompts and risk/resource mutations through the injected managers.</summary>
public class EventHandlerPlayTests : PlayModeTestBase
{
    private RiskManager _risk;
    private GameObject _dialogueBox;
    private Transform _choices;

    private void BuildStack()
    {
        _risk = BuildRiskManager();
        ResourceManager resources = BuildResourceManager();
        DialogueSystem dialogue = BuildDialogue(out _dialogueBox, out _choices);
        InspectionSystem inspection = BuildInspection(_risk, out _);

        EventHandler handler = AddInactive<EventHandler>(out _, "EventHandler");
        SetField(handler, "_dialogueSystem", dialogue);
        SetField(handler, "_inspectionSystem", inspection);
        SetField(handler, "_riskManager", _risk);
        SetField(handler, "_resourceManager", resources);
        SetField(handler, "_storyState", BuildStoryState());
        Activate(handler);
    }

    [UnityTest]
    public IEnumerator RumorsSpread_ShowsDialogueAndRaisesRisk()
    {
        BuildStack();
        yield return null; // let every Start() settle (dialogue box hidden, risk at 0)

        GameEvents.RumorsSpread();

        Assert.IsTrue(_dialogueBox.activeSelf);
        Assert.AreEqual(2f, _risk.CurrentRisk, 0.001f);
    }

    [UnityTest]
    public IEnumerator Arrest_ShowsGameOverDialogue()
    {
        BuildStack();
        yield return null;

        GameEvents.Arrest();

        Assert.IsTrue(_dialogueBox.activeSelf);
    }

    [UnityTest]
    public IEnumerator NeighborPeeking_PresentsTwoChoices()
    {
        BuildStack();
        yield return null;

        GameEvents.NeighborPeeking();

        Assert.AreEqual(2, _choices.childCount);
    }

    [UnityTest]
    public IEnumerator KnockAtDoor_PresentsTwoChoices()
    {
        BuildStack();
        yield return null;

        GameEvents.KnockAtDoor();

        Assert.AreEqual(2, _choices.childCount);
    }
}
