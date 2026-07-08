using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>PlayMode coverage for <see cref="MainMenuUiManger"/>: instructions start hidden and the
/// toggle swaps between the menu buttons and the instructions panel.</summary>
public class MainMenuPlayTests : PlayModeTestBase
{
    private MainMenuUiManger BuildMenu(out GameObject instructions, out GameObject buttons)
    {
        MainMenuUiManger menu = AddInactive<MainMenuUiManger>(out _, "MainMenu");
        instructions = NewGo("Instructions");
        buttons = NewGo("Buttons");
        SetField(menu, "_instructionsPanel", instructions);
        SetField(menu, "_mainMenuButtons", buttons);
        Activate(menu);
        return menu;
    }

    [UnityTest]
    public IEnumerator Start_HidesInstructionsAndShowsButtons()
    {
        BuildMenu(out GameObject instructions, out GameObject buttons);

        yield return null; // Start()

        Assert.IsFalse(instructions.activeSelf);
        Assert.IsTrue(buttons.activeSelf);
    }

    [UnityTest]
    public IEnumerator ToggleInstructions_SwapsPanels()
    {
        MainMenuUiManger menu = BuildMenu(out GameObject instructions, out GameObject buttons);
        yield return null;

        menu.ToggleInstructions();
        Assert.IsTrue(instructions.activeSelf);
        Assert.IsFalse(buttons.activeSelf);

        menu.ToggleInstructions();
        Assert.IsFalse(instructions.activeSelf);
        Assert.IsTrue(buttons.activeSelf);
    }
}
