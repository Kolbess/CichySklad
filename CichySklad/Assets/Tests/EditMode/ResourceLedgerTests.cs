using NUnit.Framework;

/// <summary>EditMode coverage for the pure resource accounting rules.</summary>
public class ResourceLedgerTests
{
    [Test]
    public void CountedResources_ClampAtZero()
    {
        var ledger = new ResourceLedger { Paper = 2 };
        ledger.Paper -= 5;
        Assert.AreEqual(0, ledger.Paper);
    }

    [Test]
    public void Trust_IsNotClampedAtZero()
    {
        var ledger = new ResourceLedger { Trust = 3 };
        ledger.Trust -= 5;
        Assert.AreEqual(-2, ledger.Trust);
    }

    [Test]
    public void OnResourceChanged_FiresWithPreviousAndNewValue()
    {
        var ledger = new ResourceLedger();
        ResourceType captured = ResourceType.Trust;
        int previous = -1;
        int next = -1;
        ledger.OnResourceChanged += (type, from, to) =>
        {
            captured = type;
            previous = from;
            next = to;
        };

        ledger.Ink = 4;

        Assert.AreEqual(ResourceType.Ink, captured);
        Assert.AreEqual(0, previous);
        Assert.AreEqual(4, next);
    }

    [Test]
    public void TrySpend_WhenAffordable_DeductsAndReturnsTrue()
    {
        var ledger = new ResourceLedger { Paper = 5, Ink = 5 };

        bool paid = ledger.TrySpend(costPaper: 2, costInk: 1);

        Assert.IsTrue(paid);
        Assert.AreEqual(3, ledger.Paper);
        Assert.AreEqual(4, ledger.Ink);
    }

    [Test]
    public void TrySpend_WhenShort_DeductsNothingAndReturnsFalse()
    {
        var ledger = new ResourceLedger { Paper = 1, Ink = 5 };

        bool paid = ledger.TrySpend(costPaper: 2, costInk: 1);

        Assert.IsFalse(paid);
        Assert.AreEqual(1, ledger.Paper, "Paper must not be deducted on a failed spend.");
        Assert.AreEqual(5, ledger.Ink, "Ink must not be deducted on a failed spend.");
    }

    [Test]
    public void TrySpend_WhenShort_ReportsEveryShortfall()
    {
        var ledger = new ResourceLedger { Paper = 0, Money = 0 };
        var shortfalls = new System.Collections.Generic.List<ResourceType>();
        ledger.OnInsufficientResource += type => shortfalls.Add(type);

        ledger.TrySpend(costPaper: 1, costMoney: 1);

        CollectionAssert.Contains(shortfalls, ResourceType.Paper);
        CollectionAssert.Contains(shortfalls, ResourceType.Money);
    }
}
