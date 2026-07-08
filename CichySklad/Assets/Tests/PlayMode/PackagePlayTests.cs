using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>PlayMode coverage for <see cref="Package"/>: items are stashed hidden, a click pops one
/// out at the drop point, and clicking an already-empty package removes it from the scene.</summary>
public class PackagePlayTests : PlayModeTestBase
{
    private Package BuildPackage(out Transform dropPoint)
    {
        Package package = AddInactive<Package>(out _, "Package");
        dropPoint = NewGo("DropPoint").transform;
        SetField(package, "_dropPoint", dropPoint);
        Activate(package);
        return package;
    }

    [Test]
    public void AddItem_HidesItemAndTracksIt()
    {
        Package package = BuildPackage(out _);
        var item = NewGo("Item");

        package.AddItem(item);

        Assert.AreEqual(1, package.Items.Count);
        Assert.IsFalse(item.activeSelf, "Stashed items are hidden inside the package.");
    }

    [Test]
    public void Click_PopsFirstItemToDropPoint()
    {
        Package package = BuildPackage(out Transform dropPoint);
        var item = NewGo("Item");
        package.AddItem(item);

        package.SendMessage("OnMouseDown");

        Assert.AreEqual(0, package.Items.Count);
        Assert.IsTrue(item.activeSelf);
        Assert.AreEqual(dropPoint.position, item.transform.position);
    }

    [UnityTest]
    public IEnumerator Click_OnEmptyPackage_DestroysIt()
    {
        Package package = BuildPackage(out _);

        package.SendMessage("OnMouseDown");
        yield return null; // Destroy resolves at end of frame

        Assert.IsTrue(package == null, "An empty package removes itself from the scene.");
    }
}
