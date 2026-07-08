using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>PlayMode coverage for <see cref="DeathManager"/>: the death screen starts hidden and is
/// revealed when the global <see cref="GameEvents.OnArrest"/> event fires.</summary>
public class DeathManagerPlayTests : PlayModeTestBase
{
    private DeathManager BuildDeathManager(out GameObject deathScreen)
    {
        DeathManager death = AddInactive<DeathManager>(out _, "DeathManager");
        deathScreen = NewGo("DeathScreen");
        SetField(death, "_deathScreen", deathScreen);
        Activate(death);
        return death;
    }

    [UnityTest]
    public IEnumerator DeathScreen_StartsHidden()
    {
        BuildDeathManager(out GameObject deathScreen);

        yield return null; // Start() hides it

        Assert.IsFalse(deathScreen.activeSelf);
    }

    [UnityTest]
    public IEnumerator Arrest_ShowsDeathScreen()
    {
        BuildDeathManager(out GameObject deathScreen);
        yield return null;

        GameEvents.Arrest();

        Assert.IsTrue(deathScreen.activeSelf);
    }
}
