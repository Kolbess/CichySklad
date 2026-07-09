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

    [UnityTest]
    public IEnumerator LiveItems_StayInSyncWithLedger_OnSpawnAndSpend()
    {
        ResourceManager rm = BuildResourceManager();
        yield return null;

        Assert.AreEqual(rm.Money, rm.LiveItemCount(ResourceType.Money));
        Assert.AreEqual(rm.Paper, rm.LiveItemCount(ResourceType.Paper));

        rm.TrySpend(costMoney: 2);

        Assert.AreEqual(3, rm.Money);
        Assert.AreEqual(
            3,
            rm.LiveItemCount(ResourceType.Money),
            "No ghost money items should linger after a spend."
        );
    }

    [UnityTest]
    public IEnumerator Spend_DestroysLooseItems_NotJustPackagedOnes()
    {
        ResourceManager rm = BuildResourceManager();
        yield return null;

        // These spawned money items are not held by any package (the ghost case: an item dragged out
        // into a hiding spot or left loose). The registry still tracks them, so a spend must reach them.
        int before = rm.LiveItemCount(ResourceType.Money);
        Assert.AreEqual(rm.Money, before);

        rm.TrySpend(costMoney: 2);
        yield return null; // let the destroyed items clear

        Assert.AreEqual(3, rm.Money);
        Assert.AreEqual(
            3,
            rm.LiveItemCount(ResourceType.Money),
            "Loose items must be destroyed on spend — no ghost left behind."
        );
    }

    [UnityTest]
    public IEnumerator FailedSpend_LeavesLiveItemsIntact()
    {
        ResourceManager rm = BuildResourceManager();
        yield return null;

        int paperItems = rm.LiveItemCount(ResourceType.Paper);

        Assert.IsFalse(rm.TrySpend(costPaper: 99));
        Assert.AreEqual(
            paperItems,
            rm.LiveItemCount(ResourceType.Paper),
            "A failed spend must destroy nothing."
        );
    }

    [UnityTest]
    public IEnumerator NotifyConsumed_DecrementsLedger_WithoutDestroyingMoreItems()
    {
        ResourceManager rm = BuildResourceManager();
        yield return null; // paper 2, two live paper items

        int itemsBefore = rm.LiveItemCount(ResourceType.Paper);

        rm.NotifyConsumed(ResourceType.Paper, 1);

        Assert.AreEqual(1, rm.Paper, "The ledger drops by the consumed count.");
        Assert.AreEqual(
            itemsBefore,
            rm.LiveItemCount(ResourceType.Paper),
            "NotifyConsumed must not destroy items itself — the caller already consumed them."
        );
    }
}
