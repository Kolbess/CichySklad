using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>PlayMode coverage for <see cref="DayCycle"/>: the day loop runs while the game is live
/// and freezes (<see cref="DayCycle.IsGameOver"/>) once an arrest ends the run.</summary>
public class DayCyclePlayTests : PlayModeTestBase
{
    private DayCycle BuildDayCycle()
    {
        DayCycle day = AddInactive<DayCycle>(out _, "DayCycle");

        var sprites = new List<Sprite>();
        for (int i = 0; i < 6; i++)
            sprites.Add(NewSprite());

        ResourceManager resources = BuildResourceManager();
        SetField(day, "_riskManager", BuildRiskManager());
        SetField(day, "_resourceManager", resources);
        SetField(day, "_dialogueSystem", BuildDialogue(out _, out _));
        SetField(day, "_eventScheduler", BuildEventScheduler(resources));
        SetField(day, "_daySprites", sprites);
        SetField(day, "_dayImage", NewImage());
        SetField(day, "_dayScreen", NewGo("DayScreen"));
        SetField(day, "_dayText", NewText());
        SetField(day, "_dayDuration", 100f);

        Activate(day);
        return day;
    }

    [UnityTest]
    public IEnumerator DayCycle_IsLiveBeforeArrest()
    {
        DayCycle day = BuildDayCycle();

        yield return null; // Start + first Update
        yield return null;

        Assert.IsFalse(day.IsGameOver);
    }

    [UnityTest]
    public IEnumerator Arrest_FreezesTheDayLoop()
    {
        DayCycle day = BuildDayCycle();
        yield return null;
        yield return null;
        Assert.IsFalse(day.IsGameOver);

        GameEvents.Arrest();

        Assert.IsTrue(day.IsGameOver, "An arrest must stop the day loop.");
    }
}
