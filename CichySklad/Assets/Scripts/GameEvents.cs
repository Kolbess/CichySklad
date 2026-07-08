using System;

/// <summary>
/// Global gameplay event channel — a plain static bus that lets systems talk sideways without
/// holding references to each other (e.g. a choice raises <see cref="Arrest"/>; the death and
/// dialogue systems merely listen). Deliberately NOT a <c>MonoBehaviour</c>: it owns no scene
/// state and is never attached to a GameObject. Renamed from the old <c>EventSystem</c> to avoid
/// colliding with <c>UnityEngine.EventSystems.EventSystem</c>.
///
/// Note: static event subscriptions survive scene loads. Every listener that subscribes in
/// <c>OnEnable</c> must unsubscribe in <c>OnDisable</c> (all handlers in this project do) to
/// avoid leaks and duplicate invocations across reloads.
/// </summary>
public static class GameEvents
{
    // 1. Kontrole
    public static event Action OnKnockAtDoor;
    public static event Action OnKnockOpenChoice;
    public static event Action OnKnockHideChoice;

    public static event Action OnNeighborPeeking;
    public static event Action OnNeighborIgnored;
    public static event Action OnNeighborDismissed;

    public static event Action OnOchranaStepsHeard;
    public static event Action OnOchranaRaid;

    public static event Action<int> OnOfficerInspectionStarted;
    public static event Action OnOfficerInspectionSuccessful;
    public static event Action OnOfficerInspectionFailed;

    // 2. Zasoby
    public static event Action OnOutOfInk;
    public static event Action OnLostPaperBatch;
    public static event Action OnMoistureDamage;
    public static event Action OnSecretDonation;

    // 3. Donosiciele i sąsiedzi
    public static event Action OnNeighborSawCourier;
    public static event Action OnInformerAsks;
    public static event Action OnRumorsSpread;

    // 4. Kurier / przesyłki
    public static event Action OnCourierInjured;
    public static event Action OnCourierHelp;
    public static event Action OnCourierIgnore;

    public static event Action OnUrgentDelivery;
    public static event Action OnPackageUncertain;

    // 5. Sabotage / niepewne kontakty
    public static event Action OnStuckHidingSpot;
    public static event Action OnStrangerNeedsHelp;
    public static event Action OnLampExplosion;

    // 6. Fabularne / cutscenki
    public static event Action OnLetterFromPanKowal;
    public static event Action OnMariaWarns;
    public static event Action OnInformerDisappears;

    // 7. Stresujące / natychmiastowe
    public static event Action OnLoudNoise;
    public static event Action OnFireCandle;
    public static event Action OnBrokenLock;

    // 8. Ekonomiczne / łapówki
    public static event Action OnOchranaBribe;
    public static event Action OnBuyPaperOffer;
    public static event Action OnArrest;

    // 9. Gameplay deltas (fired by objects that cannot hold a scene reference, e.g. spawned prefabs)
    public static event Action<float> OnRiskDelta;

    // =======================
    // 1. Kontrole
    public static void KnockAtDoor() => OnKnockAtDoor?.Invoke();

    public static void KnockOpenChoice() => OnKnockOpenChoice?.Invoke();

    public static void KnockHideChoice() => OnKnockHideChoice?.Invoke();

    public static void NeighborPeeking() => OnNeighborPeeking?.Invoke();

    public static void NeighborIgnored() => OnNeighborIgnored?.Invoke();

    public static void NeighborDismissed() => OnNeighborDismissed?.Invoke();

    public static void OchranaStepsHeard() => OnOchranaStepsHeard?.Invoke();

    public static void OchranaRaid() => OnOchranaRaid?.Invoke();

    public static void OfficerInspectionStarted(int itemsToHide) =>
        OnOfficerInspectionStarted?.Invoke(itemsToHide);

    public static void OfficerInspectionSuccessful() => OnOfficerInspectionSuccessful?.Invoke();

    public static void OfficerInspectionFailed() => OnOfficerInspectionFailed?.Invoke();

    // 2. Zasoby
    public static void OutOfInk() => OnOutOfInk?.Invoke();

    public static void LostPaperBatch() => OnLostPaperBatch?.Invoke();

    public static void MoistureDamage() => OnMoistureDamage?.Invoke();

    public static void SecretDonation() => OnSecretDonation?.Invoke();

    // 3. Donosiciele / sąsiedzi
    public static void NeighborSawCourier() => OnNeighborSawCourier?.Invoke();

    public static void InformerAsks() => OnInformerAsks?.Invoke();

    public static void RumorsSpread() => OnRumorsSpread?.Invoke();

    // 4. Kurier / przesyłki
    public static void CourierInjured() => OnCourierInjured?.Invoke();

    public static void CourierHelp() => OnCourierHelp?.Invoke();

    public static void CourierIgnore() => OnCourierIgnore?.Invoke();

    public static void UrgentDelivery() => OnUrgentDelivery?.Invoke();

    public static void PackageUncertain() => OnPackageUncertain?.Invoke();

    // 5. Sabotage / niepewne kontakty
    public static void StuckHidingSpot() => OnStuckHidingSpot?.Invoke();

    public static void StrangerNeedsHelp() => OnStrangerNeedsHelp?.Invoke();

    public static void LampExplosion() => OnLampExplosion?.Invoke();

    // 6. Fabularne
    public static void LetterFromPanKowal() => OnLetterFromPanKowal?.Invoke();

    public static void MariaWarns() => OnMariaWarns?.Invoke();

    public static void InformerDisappears() => OnInformerDisappears?.Invoke();

    // 7. Stresujące / natychmiastowe
    public static void LoudNoise() => OnLoudNoise?.Invoke();

    public static void FireCandle() => OnFireCandle?.Invoke();

    public static void BrokenLock() => OnBrokenLock?.Invoke();

    // 8. Ekonomiczne / łapówki
    public static void OchranaBribe() => OnOchranaBribe?.Invoke();

    // Trigger for the "buy cheaper paper" dialogue. Consumed by EventHandler; awaiting a
    // gameplay producer (economy/day-cycle) — kept dormant like the other narrative triggers.
    public static void BuyPaperOffer() => OnBuyPaperOffer?.Invoke();

    public static void Arrest() => OnArrest?.Invoke();

    // 9. Gameplay deltas
    public static void RaiseRisk(float amount) => OnRiskDelta?.Invoke(amount);
}
