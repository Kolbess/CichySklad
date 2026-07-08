using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

/// <summary>PlayMode coverage for the runtime <see cref="ResourceManager"/> adapter: starting grant,
/// add/spend routed through the ledger, and the counter-text / trust-slider views it drives.</summary>
public class ResourceManagerPlayTests : PlayModeTestBase
{
    [UnityTest]
    public IEnumerator Start_GrantsStartingResources()
    {
        ResourceManager rm = BuildResourceManager();
        yield return null; // let Start() run

        Assert.AreEqual(2, rm.Paper);
        Assert.AreEqual(2, rm.Ink);
        Assert.AreEqual(5, rm.Money);
        Assert.AreEqual(0, rm.Leaflets);
    }

    [UnityTest]
    public IEnumerator AddPaper_UpdatesCountAndCounterText()
    {
        ResourceManager rm = BuildResourceManager();
        yield return null;

        rm.AddPaper(3);

        Assert.AreEqual(5, rm.Paper);
        Assert.AreEqual("5", RmPaperText.text);
    }

    [UnityTest]
    public IEnumerator TrySpend_WhenAffordable_DeductsAcrossResources()
    {
        ResourceManager rm = BuildResourceManager();
        yield return null;

        bool paid = rm.TrySpend(costPaper: 1, costInk: 1);

        Assert.IsTrue(paid);
        Assert.AreEqual(1, rm.Paper);
        Assert.AreEqual(1, rm.Ink);
    }

    [UnityTest]
    public IEnumerator TrySpend_WhenShort_ChangesNothing()
    {
        ResourceManager rm = BuildResourceManager();
        yield return null;

        bool paid = rm.TrySpend(costPaper: 99);

        Assert.IsFalse(paid);
        Assert.AreEqual(2, rm.Paper, "A failed spend must not deduct anything.");
    }

    [UnityTest]
    public IEnumerator AddTrust_MovesTrustSliderFraction()
    {
        ResourceManager rm = BuildResourceManager();
        yield return null;

        rm.AddTrust(40);

        Assert.AreEqual(40, rm.Trust);
        Assert.AreEqual(0.4f, RmTrustSlider.value, 0.001f);
    }
}
