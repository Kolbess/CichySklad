using NUnit.Framework;

/// <summary>PlayMode coverage for the static <see cref="GameEvents"/> bus: raising a beat invokes
/// its subscribers, and the parameterised beats carry their payload through.</summary>
public class GameEventsPlayTests : PlayModeTestBase
{
    [Test]
    public void RaisingBeat_InvokesSubscriber()
    {
        int calls = 0;
        System.Action handler = () => calls++;
        GameEvents.OnRumorsSpread += handler;

        try
        {
            GameEvents.RumorsSpread();
        }
        finally
        {
            GameEvents.OnRumorsSpread -= handler;
        }

        Assert.AreEqual(1, calls);
    }

    [Test]
    public void RaiseRisk_CarriesTheDeltaPayload()
    {
        float received = float.NaN;
        System.Action<float> handler = amount => received = amount;
        GameEvents.OnRiskDelta += handler;

        try
        {
            GameEvents.RaiseRisk(7.5f);
        }
        finally
        {
            GameEvents.OnRiskDelta -= handler;
        }

        Assert.AreEqual(7.5f, received, 0.001f);
    }

    [Test]
    public void OfficerInspectionStarted_CarriesItemCount()
    {
        int itemsToHide = -1;
        System.Action<int> handler = count => itemsToHide = count;
        GameEvents.OnOfficerInspectionStarted += handler;

        try
        {
            GameEvents.OfficerInspectionStarted(4);
        }
        finally
        {
            GameEvents.OnOfficerInspectionStarted -= handler;
        }

        Assert.AreEqual(4, itemsToHide);
    }

    [Test]
    public void UnsubscribedHandler_IsNotInvoked()
    {
        int calls = 0;
        System.Action handler = () => calls++;
        GameEvents.OnArrest += handler;
        GameEvents.OnArrest -= handler;

        GameEvents.Arrest();

        Assert.AreEqual(0, calls);
    }
}
