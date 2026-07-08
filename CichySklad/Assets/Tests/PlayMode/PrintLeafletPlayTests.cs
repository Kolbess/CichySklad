using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

/// <summary>PlayMode coverage for the <see cref="PrintLeaflet"/> station: it spends ink/paper to
/// mint a leaflet, blocks re-printing during the cooldown, and recovers once the cooldown elapses.</summary>
public class PrintLeafletPlayTests : PlayModeTestBase
{
    private PrintLeaflet BuildStation(ResourceManager rm, float cooldown = 5f)
    {
        PrintLeaflet station = AddInactive<PrintLeaflet>(out _, "PrintLeaflet");
        SetField(station, "_resourceManager", rm);
        SetField(station, "_cooldownSlider", NewSlider());
        SetField(station, "_costText", NewText());
        SetField(station, "_cooldownDuration", cooldown);
        Activate(station);
        return station;
    }

    [UnityTest]
    public IEnumerator Print_SpendsInkAndPaper_AndAddsLeaflet()
    {
        ResourceManager rm = BuildResourceManager();
        yield return null; // starting: 2 paper, 2 ink
        PrintLeaflet station = BuildStation(rm);
        yield return null;

        station.SendMessage("OnMouseDown");

        Assert.AreEqual(0, rm.Paper, "Default paper cost is 2.");
        Assert.AreEqual(1, rm.Ink, "Default ink cost is 1.");
        Assert.AreEqual(1, rm.Leaflets);
    }

    [UnityTest]
    public IEnumerator Print_DuringCooldown_IsBlocked()
    {
        ResourceManager rm = BuildResourceManager();
        yield return null;
        rm.AddPaper(4);
        rm.AddInk(4);
        PrintLeaflet station = BuildStation(rm);
        yield return null;

        station.SendMessage("OnMouseDown"); // prints, enters cooldown
        int afterFirst = rm.Leaflets;
        station.SendMessage("OnMouseDown"); // blocked by cooldown

        Assert.AreEqual(1, afterFirst);
        Assert.AreEqual(1, rm.Leaflets, "A second print during cooldown must be ignored.");
    }

    [UnityTest]
    public IEnumerator Print_AfterCooldownElapses_IsAllowedAgain()
    {
        ResourceManager rm = BuildResourceManager();
        yield return null;
        rm.AddPaper(4);
        rm.AddInk(4);
        PrintLeaflet station = BuildStation(rm, cooldown: 0.1f);
        yield return null;

        station.SendMessage("OnMouseDown");
        Assert.AreEqual(1, rm.Leaflets);

        // Wait out the cooldown (driven by Update).
        float waited = 0f;
        while (waited < 0.4f)
        {
            waited += UnityEngine.Time.deltaTime;
            yield return null;
        }

        station.SendMessage("OnMouseDown");
        Assert.AreEqual(2, rm.Leaflets, "Printing should resume once the cooldown clears.");
    }
}
