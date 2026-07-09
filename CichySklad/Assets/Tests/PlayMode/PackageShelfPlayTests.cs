using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// PlayMode coverage for the <see cref="PackageShelf"/>: storing parcels up to capacity, blocking
/// overflow, snapping parcels onto slot positions, freeing a slot when a parcel is removed or
/// destroyed, and firing the contents-changed event.
/// </summary>
public class PackageShelfPlayTests : PlayModeTestBase
{
    private PackageShelf BuildShelf(int capacity, out List<Transform> slots)
    {
        PackageShelf shelf = AddInactive<PackageShelf>(out _, "PackageShelf");

        slots = new List<Transform>();
        for (int i = 0; i < capacity; i++)
        {
            var slotGo = NewGo($"Slot{i}");
            slotGo.transform.position = new Vector3(i + 1, 0f, 0f);
            slots.Add(slotGo.transform);
        }

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
    public IEnumerator Store_FillsToCapacity_ThenBlocksOverflow()
    {
        PackageShelf shelf = BuildShelf(2, out _);
        yield return null;

        Assert.IsTrue(shelf.TryStore(NewParcel(1)));
        Assert.IsTrue(shelf.TryStore(NewParcel(2)));
        Assert.AreEqual(2, shelf.StoredCount);
        Assert.IsTrue(shelf.IsFull);

        Assert.IsFalse(shelf.TryStore(NewParcel(1)), "A full shelf rejects further parcels.");
        Assert.AreEqual(2, shelf.StoredCount);
    }

    [UnityTest]
    public IEnumerator StoredParcels_SnapToSlotPositions()
    {
        PackageShelf shelf = BuildShelf(2, out List<Transform> slots);
        yield return null;

        PackedParcel p1 = NewParcel(1);
        PackedParcel p2 = NewParcel(1);
        shelf.TryStore(p1);
        shelf.TryStore(p2);

        Assert.AreEqual(slots[0].position, p1.transform.position, "First parcel takes slot 0.");
        Assert.AreEqual(slots[1].position, p2.transform.position, "Second parcel takes slot 1.");
    }

    [UnityTest]
    public IEnumerator Remove_FreesSlot_AndAllowsStoringAgain()
    {
        PackageShelf shelf = BuildShelf(2, out _);
        yield return null;

        PackedParcel p1 = NewParcel(1);
        shelf.TryStore(p1);
        shelf.TryStore(NewParcel(1));
        Assert.IsTrue(shelf.IsFull);

        shelf.RemoveParcel(p1);
        Assert.AreEqual(1, shelf.StoredCount);
        Assert.IsFalse(shelf.IsFull);
        Assert.IsNull(p1.CurrentShelf, "A removed parcel drops its shelf reference.");

        Assert.IsTrue(shelf.TryStore(NewParcel(1)), "The freed slot can be reused.");
        Assert.AreEqual(2, shelf.StoredCount);
    }

    [UnityTest]
    public IEnumerator DestroyingStoredParcel_FreesItsSlot()
    {
        PackageShelf shelf = BuildShelf(2, out _);
        yield return null;

        PackedParcel p1 = NewParcel(1);
        PackedParcel p2 = NewParcel(1);
        shelf.TryStore(p1);
        shelf.TryStore(p2);
        Assert.AreEqual(2, shelf.StoredCount);

        Object.Destroy(p2.gameObject);
        yield return null; // OnDestroy runs, freeing the slot

        Assert.AreEqual(1, shelf.StoredCount, "Destroying a parcel must free its slot.");
        Assert.IsFalse(shelf.IsFull);
    }

    [UnityTest]
    public IEnumerator OnContentsChanged_FiresOnStoreAndRemove()
    {
        PackageShelf shelf = BuildShelf(2, out _);
        yield return null;

        int changes = 0;
        shelf.OnContentsChanged += () => changes++;

        PackedParcel p1 = NewParcel(1);
        shelf.TryStore(p1); // +1
        shelf.RemoveParcel(p1); // +1

        Assert.AreEqual(2, changes);
    }
}
