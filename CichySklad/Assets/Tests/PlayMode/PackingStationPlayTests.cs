using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// PlayMode coverage for the <see cref="PackingStation"/> loop: loading leaflets up to capacity,
/// blocking Start without leaflets or without a coin, and the full path where Start spends a coin,
/// consumes the leaflets, runs the pack phase, and a collect click dispenses a package with the
/// packed leaflets inside.
/// </summary>
public class PackingStationPlayTests : PlayModeTestBase
{
    private PackingStation BuildStation(ResourceManager rm, float min = 0.2f, float max = 0.2f)
    {
        PackingStation station = AddInactive<PackingStation>(out _, "PackingStation");
        SetField(station, "_resourceManager", rm);
        SetField(station, "_progressSlider", NewSlider());
        SetField(station, "_statusText", NewText());
        SetField(station, "_minPackTime", min);
        SetField(station, "_maxPackTime", max);
        SetField(station, "_parcelPrefab", NewParcelPrefab());
        Activate(station);
        return station;
    }

    private static PackableLeaflet NewLeaflet()
    {
        var go = new GameObject("Leaflet");
        go.SetActive(false);
        go.AddComponent<BoxCollider2D>();
        PackableLeaflet leaflet = go.AddComponent<PackableLeaflet>();
        go.SetActive(true);
        return leaflet;
    }

    private static GameObject NewParcelPrefab()
    {
        // A minimal sealed-parcel template: PackedParcel only, no Awake asserts to defer.
        var go = new GameObject("ParcelPrefab");
        go.AddComponent<PackedParcel>();
        return go;
    }

    private static IEnumerator WaitForState(
        PackingStation station,
        PackingState target,
        float timeout = 5f
    )
    {
        float waited = 0f;
        while (station.State != target && waited < timeout)
        {
            waited += Time.deltaTime;
            yield return null;
        }

        Assert.AreEqual(target, station.State, $"Did not reach {target} within {timeout}s.");
    }

    [UnityTest]
    public IEnumerator Loading_FillsToCapacity_ThenBlocks()
    {
        ResourceManager rm = BuildResourceManager();
        yield return null;
        PackingStation station = BuildStation(rm);
        yield return null;

        station.LoadLeaflet(NewLeaflet());
        station.LoadLeaflet(NewLeaflet());
        Assert.AreEqual(2, station.LoadedCount);
        Assert.AreEqual(PackingState.Filling, station.State);

        station.LoadLeaflet(NewLeaflet());
        Assert.AreEqual(2, station.LoadedCount, "Capacity 2 must reject a third leaflet.");
    }

    [UnityTest]
    public IEnumerator StartPacking_WithoutLeaflets_IsBlocked()
    {
        ResourceManager rm = BuildResourceManager();
        yield return null; // starting money 5
        PackingStation station = BuildStation(rm);
        yield return null;

        station.StartPacking();

        Assert.AreEqual(PackingState.Idle, station.State);
        Assert.AreEqual(5, rm.Money, "No coin is spent when there is nothing to pack.");
    }

    [UnityTest]
    public IEnumerator StartPacking_WithoutMoney_IsBlocked()
    {
        ResourceManager rm = BuildResourceManager();
        yield return null;
        rm.TrySpend(costMoney: 5); // drain the starting money
        PackingStation station = BuildStation(rm);
        yield return null;

        station.LoadLeaflet(NewLeaflet());
        station.StartPacking();

        Assert.AreEqual(PackingState.Filling, station.State, "No coin → stays filling.");
        Assert.AreEqual(1, station.LoadedCount, "The leaflet is not consumed on a blocked start.");
    }

    [UnityTest]
    public IEnumerator FullFlow_SpendsCoin_ConsumesLeaflets_DispensesPackage()
    {
        ResourceManager rm = BuildResourceManager();
        yield return null; // starting money 5
        PackingStation station = BuildStation(rm);
        yield return null;

        PackableLeaflet l1 = NewLeaflet();
        PackableLeaflet l2 = NewLeaflet();
        station.LoadLeaflet(l1);
        station.LoadLeaflet(l2);
        Assert.AreEqual(PackingState.Filling, station.State);

        PackedParcel dispensed = null;
        station.OnParcelDispensed += p => dispensed = p;

        station.StartPacking();
        Assert.AreEqual(PackingState.Packing, station.State);
        Assert.AreEqual(4, rm.Money, "Packing costs one coin (5 - 1).");

        yield return null; // let the consumed leaflet objects be destroyed
        Assert.IsTrue(l1 == null && l2 == null, "Loaded leaflets are consumed at pack start.");

        yield return WaitForState(station, PackingState.Ready);

        station.CollectPackage();
        Assert.AreEqual(PackingState.Idle, station.State);
        Assert.IsNotNull(dispensed, "A sealed parcel is handed to the player on collect.");
        Assert.AreEqual(
            2,
            dispensed.LeafletCount,
            "The parcel is stamped with both packed leaflets."
        );
    }
}
