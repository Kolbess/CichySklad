using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// PlayMode coverage for the <see cref="CutscenePlayer"/>: playing a multi-line sequence through the
/// dialogue box, advancing by input and by auto-timer, ending on a choice, and locking the box so
/// events cannot interrupt it.
/// </summary>
public class CutscenePlayerPlayTests : PlayModeTestBase
{
    private CutscenePlayer BuildPlayer(DialogueSystem dialogue)
    {
        CutscenePlayer player = AddInactive<CutscenePlayer>(out _, "CutscenePlayer");
        SetField(player, "_dialogueSystem", dialogue);
        Activate(player);
        return player;
    }

    private static Cutscene NewCutscene(params CutsceneLine[] lines)
    {
        var cutscene = ScriptableObject.CreateInstance<Cutscene>();
        SetField(cutscene, "_lines", lines);
        return cutscene;
    }

    private static IEnumerator WaitUntil(Func<bool> condition, float timeout = 3f)
    {
        float waited = 0f;
        while (!condition() && waited < timeout)
        {
            waited += Time.deltaTime;
            yield return null;
        }
    }

    [UnityTest]
    public IEnumerator Play_LocksBox_AdvancesThroughLines_ThenFinishesAndUnlocks()
    {
        DialogueSystem dialogue = BuildDialogue(out GameObject box, out _);
        CutscenePlayer player = BuildPlayer(dialogue);
        yield return null; // let Start() settle the box

        bool finished = false;
        player.OnCutsceneFinished += () => finished = true;

        player.Play(
            NewCutscene(
                new CutsceneLine("maria", "Linia 1"),
                new CutsceneLine("kowal", "Linia 2"),
                new CutsceneLine("ochrana", "Linia 3")
            )
        );

        Assert.IsTrue(player.IsPlaying);
        Assert.IsTrue(box.activeSelf, "The box shows while the cutscene plays.");
        Assert.IsTrue(dialogue.IsLocked, "The box is locked against events during a cutscene.");

        player.Advance(); // → line 2
        player.Advance(); // → line 3 (last)
        Assert.IsTrue(player.IsPlaying, "Still on the final line before the last advance.");

        player.Advance(); // past the last line → finish
        Assert.IsFalse(player.IsPlaying);
        Assert.IsFalse(box.activeSelf, "The box hides when the cutscene ends.");
        Assert.IsFalse(dialogue.IsLocked, "The lock is released when the cutscene ends.");
        Assert.IsTrue(finished, "OnCutsceneFinished fires at the end.");
    }

    [UnityTest]
    public IEnumerator AutoAdvance_ProgressesWithoutInput()
    {
        DialogueSystem dialogue = BuildDialogue(out _, out _);
        CutscenePlayer player = BuildPlayer(dialogue);
        yield return null;

        bool finished = false;
        player.OnCutsceneFinished += () => finished = true;

        player.Play(
            NewCutscene(
                new CutsceneLine("maria", "Auto 1", 0.15f),
                new CutsceneLine("maria", "Auto 2", 0.15f)
            )
        );

        yield return WaitUntil(() => finished);
        Assert.IsTrue(finished, "Timed lines advance and end on their own, no click needed.");
        Assert.IsFalse(player.IsPlaying);
    }

    [UnityTest]
    public IEnumerator EndChoices_LastLinePresentsChoices_SelectionEndsCutscene()
    {
        DialogueSystem dialogue = BuildDialogue(out _, out Transform choices);
        CutscenePlayer player = BuildPlayer(dialogue);
        yield return null;

        bool picked = false;
        bool finished = false;
        player.OnCutsceneFinished += () => finished = true;

        player.PlayWithEndChoices(
            NewCutscene(
                new CutsceneLine("maria", "Ostatnie ostrzeżenie."),
                new CutsceneLine("maria", "Co robisz?")
            ),
            new[] { new DialogueSystem.Choice("Zgadzam się", () => picked = true) }
        );

        player.Advance(); // reach the final line, which shows the choices
        Assert.IsFalse(player.IsPlaying, "Line playback stops once choices are up.");
        Assert.IsFalse(dialogue.IsLocked, "The lock releases so the choice is interactive.");

        Button choiceButton = choices.GetComponentInChildren<Button>();
        Assert.IsNotNull(choiceButton, "The end choice spawned a button.");
        choiceButton.onClick.Invoke();

        Assert.IsTrue(picked, "The choice's action ran.");
        Assert.IsTrue(finished, "Picking the end choice finishes the cutscene.");
    }

    [UnityTest]
    public IEnumerator DemoCutscene_AutoPlays_WithFirstLineVisible()
    {
        DialogueSystem dialogue = BuildDialogue(out GameObject box, out _);
        CutscenePlayer player = AddInactive<CutscenePlayer>(out _, "CutscenePlayer");
        SetField(player, "_dialogueSystem", dialogue);
        SetField(
            player,
            "_demoCutscene",
            NewCutscene(new CutsceneLine("maria", "Demo 1"), new CutsceneLine("kowal", "Demo 2"))
        );
        Activate(player);

        // Two frames: Start schedules the demo, then it plays after DialogueSystem.Start hid the box.
        yield return null;
        yield return null;

        Assert.IsTrue(player.IsPlaying, "The demo cutscene auto-plays.");
        Assert.IsTrue(
            box.activeSelf,
            "The first line stays visible — not swallowed by DialogueSystem.Start."
        );
    }

    [UnityTest]
    public IEnumerator LockedBox_IgnoresExternalShowDialogue()
    {
        DialogueSystem dialogue = BuildDialogue(out GameObject box, out _);
        yield return null; // Start() hides the box

        dialogue.SetLocked(true);
        dialogue.ShowDialogue("Event trying to interrupt");
        Assert.IsFalse(box.activeSelf, "A locked box ignores external ShowDialogue.");

        dialogue.SetLocked(false);
        dialogue.ShowDialogue("Now allowed");
        Assert.IsTrue(box.activeSelf, "Unlocking restores normal dialogue.");
    }
}
