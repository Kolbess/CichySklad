using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// PlayMode coverage for the <see cref="PrintLeaflet"/> station's material-flow loop: loading paper
/// and ink arms it, Start spends the resources and runs the print phase with a leaflet produced at
/// the end, then a shorter cooldown returns it to idle. Start without a full load is a no-op.
/// </summary>
public class PrintLeafletPlayTests : PlayModeTestBase
{
    private PrintLeaflet BuildStation(
        ResourceManager rm,
        float printDuration = 6f,
        float cooldown = 2f
    )
    {
        PrintLeaflet station = AddInactive<PrintLeaflet>(out _, "PrintLeaflet");
        SetField(station, "_resourceManager", rm);
        SetField(station, "_progressSlider", NewSlider());
        SetField(station, "_costText", NewText());
        SetField(station, "_printDuration", printDuration);
        SetField(station, "_cooldownDuration", cooldown);
        Activate(station);
        return station;
    }

    private static PrinterMaterial NewMaterial(PrinterMaterialType type)
    {
        var go = new GameObject($"Material_{type}");
        go.SetActive(false);
        go.AddComponent<BoxCollider2D>();
        PrinterMaterial material = go.AddComponent<PrinterMaterial>();
        SetField(material, "_type", type);
        go.SetActive(true);
        return material;
    }

    private static IEnumerator WaitForState(
        PrintLeaflet station,
        PrinterState target,
        float timeout = 3f
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
    public IEnumerator Loading_BothMaterials_ArmsTheStation()
    {
        ResourceManager rm = BuildResourceManager();
        yield return null;
        PrintLeaflet station = BuildStation(rm);
        yield return null;

        station.LoadMaterial(NewMaterial(PrinterMaterialType.Paper));
        Assert.AreEqual(PrinterState.Idle, station.State, "Paper alone must not arm the station.");

        station.LoadMaterial(NewMaterial(PrinterMaterialType.Ink));
        Assert.AreEqual(PrinterState.Loaded, station.State);
        Assert.IsTrue(station.IsLoaded);
    }

    [UnityTest]
    public IEnumerator StartPrint_WithoutMaterials_IsBlocked()
    {
        ResourceManager rm = BuildResourceManager();
        yield return null; // starting: 2 paper, 2 ink
        PrintLeaflet station = BuildStation(rm);
        yield return null;

        station.StartPrint();

        Assert.AreEqual(PrinterState.Idle, station.State, "Nothing loaded → no print.");
        Assert.AreEqual(0, rm.Leaflets);
        Assert.AreEqual(2, rm.Paper, "Resources must not be spent without a load.");
        Assert.AreEqual(2, rm.Ink);
    }

    [UnityTest]
    public IEnumerator FullFlow_SpendsAtStart_ProducesLeafletAfterPrint_ThenCoolsToIdle()
    {
        ResourceManager rm = BuildResourceManager();
        yield return null; // starting: 2 paper, 2 ink
        PrintLeaflet station = BuildStation(rm, printDuration: 0.3f, cooldown: 0.1f);
        yield return null;

        station.LoadMaterial(NewMaterial(PrinterMaterialType.Paper));
        station.LoadMaterial(NewMaterial(PrinterMaterialType.Ink));
        Assert.AreEqual(PrinterState.Loaded, station.State);

        station.StartPrint();
        Assert.AreEqual(PrinterState.Printing, station.State);
        Assert.AreEqual(0, rm.Paper, "Default paper cost 2 is spent at start.");
        Assert.AreEqual(1, rm.Ink, "Default ink cost 1 is spent at start.");
        Assert.AreEqual(0, rm.Leaflets, "No leaflet until the print phase completes.");

        yield return WaitForState(station, PrinterState.CoolingDown);
        Assert.AreEqual(1, rm.Leaflets, "A leaflet is minted when printing finishes.");

        yield return WaitForState(station, PrinterState.Idle);
    }
}
