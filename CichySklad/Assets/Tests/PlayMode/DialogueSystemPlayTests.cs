using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>PlayMode coverage for <see cref="DialogueSystem"/>: showing lines toggles the box,
/// choiceless lines auto-hide, and choice buttons run their action then dismiss the box.</summary>
public class DialogueSystemPlayTests : PlayModeTestBase
{
    [UnityTest]
    public IEnumerator ShowDialogue_ActivatesBoxAndSetsText()
    {
        DialogueSystem dialogue = BuildDialogue(out GameObject box, out _);
        yield return null; // Start() hides the box

        dialogue.ShowDialogue("Hello");

        Assert.IsTrue(box.activeSelf);
    }

    [UnityTest]
    public IEnumerator ShowDialogue_AutoHidesAfterDelay()
    {
        DialogueSystem dialogue = BuildDialogue(out GameObject box, out _);
        SetField(dialogue, "_autoHideSeconds", 0.1f);
        yield return null;

        dialogue.ShowDialogue("Fleeting");
        Assert.IsTrue(box.activeSelf);

        float waited = 0f;
        while (waited < 0.4f)
        {
            waited += Time.deltaTime;
            yield return null;
        }

        Assert.IsFalse(box.activeSelf, "A choiceless line should auto-hide.");
    }

    [UnityTest]
    public IEnumerator ShowDialogueWithChoices_SpawnsAButtonPerChoice()
    {
        DialogueSystem dialogue = BuildDialogue(out _, out Transform choices);
        yield return null;

        var options = new[]
        {
            new DialogueSystem.Choice("A", () => { }),
            new DialogueSystem.Choice("B", () => { }),
        };
        dialogue.ShowDialogueWithChoices("Pick", options);

        Assert.AreEqual(2, choices.childCount);
    }

    [UnityTest]
    public IEnumerator SelectingChoice_RunsActionAndHidesBox()
    {
        DialogueSystem dialogue = BuildDialogue(out GameObject box, out Transform choices);
        yield return null;

        bool picked = false;
        var options = new[] { new DialogueSystem.Choice("Confirm", () => picked = true) };
        dialogue.ShowDialogueWithChoices("Pick", options);

        choices.GetComponentInChildren<Button>().onClick.Invoke();

        Assert.IsTrue(picked, "The chosen option's action should run.");
        Assert.IsFalse(box.activeSelf, "Choosing should dismiss the dialogue.");
    }
}
