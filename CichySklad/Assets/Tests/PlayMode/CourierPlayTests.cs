using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// PlayMode coverage for the <see cref="Courier"/>: a collected parcel pays coins scaled by the
/// current risk band and raises trust, a click pulls exactly one parcel off the shelf (freeing its
/// slot), and an empty visit pays nothing.
/// </summary>
public class CourierPlayTests : PlayModeTestBase
{
    private Courier BuildCourier(
        ResourceManager rm,
        RiskManager risk,
        PackageShelf shelf = null,
        bool scheduled = false
    )
    {
        Courier courier = AddInactive<Courier>(out _, "Courier");
        SetField(courier, "_riskManager", risk);
        SetField(courier, "_resourceManager", rm);
        if (shelf != null)
            SetField(courier, "_packageShelf", shelf);
        SetField(courier, "_minPayment", 1);
        SetField(courier, "_maxPayment", 5);
        SetField(courier, "_trustReward", 5);
        SetField(courier, "_appearsOnSchedule", scheduled);
        SetField(courier, "_minGapDays", 2);
        SetField(courier, "_maxGapDays", 4);
        Activate(courier);
        return courier;
    }

    private PackageShelf BuildShelf(int capacity)
    {
        PackageShelf shelf = AddInactive<PackageShelf>(out _, "PackageShelf");
        var slots = new List<Transform>();
        for (int i = 0; i < capacity; i++)
            slots.Add(NewGo($"Slot{i}").transform);
        SetField(shelf, "_slots", slots);
        Activate(shelf);
        return shelf;
    }

    private static PackedParcel NewParcel(int leaflets)
    {
        var go = new GameObject("Parcel");
        PackedParcel parcel = go.AddComponent<PackedParcel>();
        parcel.Initialize(leaflets);
        return parcel;
    }

    [UnityTest]
    public IEnumerator Collect_AtLowRisk_PaysMinimum_AndRaisesTrust()
    {
        ResourceManager rm = BuildResourceManager();
        yield return null; // money 5, trust 0
        RiskManager risk = BuildRiskManager();
        risk.SetRisk(0f); // Low band
        Courier courier = BuildCourier(rm, risk);

        PackedParcel parcel = NewParcel(2);
        Assert.IsTrue(courier.TryCollect(parcel));

        Assert.AreEqual(6, rm.Money, "Low risk pays the minimum, 1 (5 + 1).");
        Assert.AreEqual(5, rm.Trust, "A delivery raises trust by the reward.");

        yield return null; // let the consumed parcel be destroyed
        Assert.IsTrue(parcel == null, "The collected parcel is consumed.");
    }

    [UnityTest]
    public IEnumerator Collect_RemovesTheParcelsLeafletsFromTheCounter()
    {
        ResourceManager rm = BuildResourceManager();
        yield return null;
        rm.AddLeaflets(3); // three packed-but-counted leaflets on the ledger
        RiskManager risk = BuildRiskManager();
        risk.SetRisk(0f);
        Courier courier = BuildCourier(rm, risk);

        courier.TryCollect(NewParcel(2)); // parcel holds 2 leaflets

        Assert.AreEqual(1, rm.Leaflets, "The courier clears the parcel's leaflets (3 - 2).");
    }

    [UnityTest]
    public IEnumerator Collect_AtCriticalRisk_PaysMaximum()
    {
        ResourceManager rm = BuildResourceManager();
        yield return null;
        RiskManager risk = BuildRiskManager();
        risk.SetRisk(95f); // Critical band
        Courier courier = BuildCourier(rm, risk);

        courier.TryCollect(NewParcel(1));

        Assert.AreEqual(10, rm.Money, "Critical risk pays the maximum, 5 (5 + 5).");
    }

    [UnityTest]
    public IEnumerator CollectFromShelf_TakesExactlyOneParcel()
    {
        ResourceManager rm = BuildResourceManager();
        yield return null;
        RiskManager risk = BuildRiskManager();
        risk.SetRisk(0f);
        PackageShelf shelf = BuildShelf(2);
        shelf.TryStore(NewParcel(1));
        shelf.TryStore(NewParcel(1));
        Assert.AreEqual(2, shelf.StoredCount);

        Courier courier = BuildCourier(rm, risk, shelf);
        courier.CollectFromShelf();
        yield return null; // parcel destroyed → its slot freed

        Assert.AreEqual(1, shelf.StoredCount, "Only one parcel is taken per visit.");
        Assert.AreEqual(6, rm.Money);
    }

    [UnityTest]
    public IEnumerator CollectFromShelf_WhenEmpty_PaysNothing()
    {
        ResourceManager rm = BuildResourceManager();
        yield return null;
        RiskManager risk = BuildRiskManager();
        PackageShelf shelf = BuildShelf(2); // empty
        Courier courier = BuildCourier(rm, risk, shelf);

        courier.CollectFromShelf();

        Assert.AreEqual(5, rm.Money, "No parcel → no payment.");
        Assert.AreEqual(0, rm.Trust);
    }

    [UnityTest]
    public IEnumerator Scheduled_StartsAbsent_ArrivesOnVisitDay_ThenDeparts()
    {
        ResourceManager rm = BuildResourceManager();
        yield return null;
        RiskManager risk = BuildRiskManager();
        Courier courier = BuildCourier(rm, risk, scheduled: true);

        Assert.IsFalse(courier.IsPresent, "A scheduled courier starts away.");

        // Day maxGap (4) is at or beyond the first visit day (rolled within [2, 4]).
        courier.HandleDayStarted(4);
        Assert.IsTrue(courier.IsPresent, "He arrives once the visit day is reached.");

        courier.HandleDayStarted(5);
        Assert.IsFalse(courier.IsPresent, "The visit lasts the day, then he leaves.");
    }

    [UnityTest]
    public IEnumerator Scheduled_WhileAway_CollectsNothing()
    {
        ResourceManager rm = BuildResourceManager();
        yield return null;
        RiskManager risk = BuildRiskManager();
        Courier courier = BuildCourier(rm, risk, scheduled: true);

        Assert.IsFalse(courier.IsPresent);
        Assert.IsFalse(courier.TryCollect(NewParcel(1)), "An absent courier takes nothing.");
        Assert.AreEqual(5, rm.Money, "No payment while the courier is away.");
    }
}
