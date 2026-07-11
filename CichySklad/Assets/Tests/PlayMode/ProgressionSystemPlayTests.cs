using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// PlayMode coverage for the <see cref="ProgressionSystem"/>: crossing a trust threshold grants an
/// unlock once, fires the event, applies its effect (a coin reward or a bigger stash), and does not
/// re-grant on a dip-and-rise.
/// </summary>
public class ProgressionSystemPlayTests : PlayModeTestBase
{
    private static Unlock NewUnlock(
        string key,
        int threshold,
        UnlockEffect effect = UnlockEffect.None,
        int magnitude = 0
    )
    {
        var unlock = ScriptableObject.CreateInstance<Unlock>();
        SetField(unlock, "_key", key);
        SetField(unlock, "_trustThreshold", threshold);
        SetField(unlock, "_title", key);
        SetField(unlock, "_description", "test unlock");
        SetField(unlock, "_effect", effect);
        SetField(unlock, "_magnitude", magnitude);
        return unlock;
    }

    private ProgressionSystem BuildProgression(
        ResourceManager rm,
        Unlock[] unlocks,
        HidingSpot spot = null
    )
    {
        ProgressionSystem progression = AddInactive<ProgressionSystem>(out _, "Progression");
        SetField(progression, "_resourceManager", rm);
        SetField(progression, "_unlocks", unlocks);
        if (spot != null)
            SetField(progression, "_hidingSpot", spot);
        Activate(progression);
        return progression;
    }

    private HidingSpot BuildHidingSpot(int maxCapacity)
    {
        HidingSpot spot = AddInactive<HidingSpot>(out _, "HidingSpot"); // RequireComponent adds the rest
        SetField(spot, "_maxCapacity", maxCapacity);
        SetField(spot, "_capacityUI", NewGo("capacityUI"));
        SetField(spot, "_capacityText", NewText());
        SetField(spot, "_contentUI", NewGo("contentUI"));
        Activate(spot);
        return spot;
    }

    [UnityTest]
    public IEnumerator Trust_Crossing_GrantsUnlock_FiresEvent_AppliesMoneyReward()
    {
        ResourceManager rm = BuildResourceManager();
        yield return null; // money 5, trust 0
        ProgressionSystem progression = BuildProgression(
            rm,
            new[] { NewUnlock("wsparcie", 10, UnlockEffect.MoneyReward, 5) }
        );
        yield return null;

        Unlock earned = null;
        progression.OnUnlocked += u => earned = u;

        rm.AddTrust(10);

        Assert.IsTrue(progression.IsUnlocked("wsparcie"), "Reaching the threshold unlocks it.");
        Assert.IsNotNull(earned, "OnUnlocked fires for the earned unlock.");
        Assert.AreEqual("wsparcie", earned.Key);
        Assert.AreEqual(10, rm.Money, "MoneyReward grants coins (5 + 5).");
    }

    [UnityTest]
    public IEnumerator BelowThreshold_DoesNotUnlock()
    {
        ResourceManager rm = BuildResourceManager();
        yield return null;
        ProgressionSystem progression = BuildProgression(rm, new[] { NewUnlock("wsparcie", 10) });
        yield return null;

        rm.AddTrust(5);

        Assert.IsFalse(progression.IsUnlocked("wsparcie"));
        Assert.AreEqual(0, progression.UnlockedCount);
    }

    [UnityTest]
    public IEnumerator Unlock_IsNotRegranted_OnDipAndRise()
    {
        ResourceManager rm = BuildResourceManager();
        yield return null;
        ProgressionSystem progression = BuildProgression(
            rm,
            new[] { NewUnlock("wsparcie", 10, UnlockEffect.MoneyReward, 5) }
        );
        yield return null;

        rm.AddTrust(10); // unlock, money 5 -> 10
        rm.AddTrust(-5); // trust back to 5
        rm.AddTrust(5); // and up to 10 again

        Assert.AreEqual(1, progression.UnlockedCount, "An unlock is earned only once.");
        Assert.AreEqual(10, rm.Money, "The reward is not paid a second time.");
    }

    [UnityTest]
    public IEnumerator BigJump_GrantsEveryCrossedUnlock()
    {
        ResourceManager rm = BuildResourceManager();
        yield return null;
        ProgressionSystem progression = BuildProgression(
            rm,
            new[] { NewUnlock("a", 10), NewUnlock("b", 20), NewUnlock("c", 30) }
        );
        yield return null;

        rm.AddTrust(30);

        Assert.AreEqual(3, progression.UnlockedCount, "One jump crosses and grants all three.");
    }

    [UnityTest]
    public IEnumerator ExtraHidingCapacity_EnlargesTheHidingSpot()
    {
        ResourceManager rm = BuildResourceManager();
        yield return null;
        HidingSpot spot = BuildHidingSpot(3);
        yield return null;
        ProgressionSystem progression = BuildProgression(
            rm,
            new[] { NewUnlock("stash", 10, UnlockEffect.ExtraHidingCapacity, 2) },
            spot
        );
        yield return null;

        rm.AddTrust(10);

        Assert.IsTrue(progression.IsUnlocked("stash"));
        Assert.AreEqual(5, spot.MaxCapacity, "The stash grows by the unlock's magnitude (3 + 2).");
    }
}
