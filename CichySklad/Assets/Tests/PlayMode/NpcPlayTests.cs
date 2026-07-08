using NUnit.Framework;

/// <summary>PlayMode coverage for <see cref="Npc"/>: it surfaces its owning inspection controller
/// and drives footstep playback through its AudioSource without error.</summary>
public class NpcPlayTests : PlayModeTestBase
{
    private Npc BuildNpc(out InspectionSystem owner)
    {
        // An inactive InspectionSystem is a valid non-null reference whose own Awake never runs,
        // so the Npc can be tested in isolation from the full inspection rig.
        owner = AddInactive<InspectionSystem>(out _, "InspectionOwner");

        Npc npc = AddInactive<Npc>(out _, "Npc");
        SetField(npc, "_inspectionSystem", owner);
        Activate(npc);
        return npc;
    }

    [Test]
    public void InspectionSystem_ExposesTheAssignedOwner()
    {
        Npc npc = BuildNpc(out InspectionSystem owner);

        Assert.AreSame(owner, npc.InspectionSystem);
    }

    [Test]
    public void PlayThenStopSound_LeavesNpcNotPlaying()
    {
        Npc npc = BuildNpc(out _);

        Assert.DoesNotThrow(() => npc.PlaySound());
        npc.StopSound();

        Assert.IsFalse(npc.IsPlayingSound);
    }
}
