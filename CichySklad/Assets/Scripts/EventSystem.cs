using System;
using UnityEngine;

public class EventSystem : MonoBehaviour
{
    // 1. Kontrole
    public static event Action OnKnockAtDoor;
    public static event Action OnKnockOpenChoice;
    public static event Action OnKnockHideChoice;

    public static event Action OnNeighborPeeking;
    public static event Action OnNeighborIgnored;
    public static event Action OnNeighborDismissed;

    public static event Action OnOchranaStepsHeard;
    public static event Action OnOchranaPauseAccepted;
    public static event Action OnOchranaPauseDeclined;

    public static event Action<int> OnOfficerInspectionStarted;
    public static event Action OnOfficerInspectionSuccessful;
    public static event Action OnOfficerInspectionFailed;

    // 2. Zasoby
    public static event Action OnOutOfInk;
    public static event Action OnOutOfInkUseNow;
    public static event Action OnOutOfInkSave;

    public static event Action OnLostPaperBatch;
    public static event Action OnLostPaperPayInformer;
    public static event Action OnLostPaperIgnore;

    public static event Action OnMoistureDamage;
    public static event Action OnMoistureThrow;
    public static event Action OnMoistureRisk;

    public static event Action OnSecretDonation;
    public static event Action OnSecretDonationTake;
    public static event Action OnSecretDonationLeave;

    // 3. Donosiciele i sąsiedzi
    public static event Action OnNeighborSawCourier;
    public static event Action OnNeighborBribe;
    public static event Action OnInformerAsks;
    public static event Action OnInformerLie;
    public static event Action OnInformerDismiss;
    public static event Action OnInformerIgnore;
    public static event Action OnRumorsSpread;

    // 4. Kurier / przesyłki
    public static event Action OnCourierInjured;
    public static event Action OnCourierHelp;
    public static event Action OnCourierIgnore;

    public static event Action OnUrgentDelivery;
    public static event Action OnPackageUncertain;
    public static event Action OnPackageOpen;
    public static event Action OnPackageWait;

    // 5. Sabotage / niepewne kontakty
    public static event Action OnStuckHidingSpot;
    public static event Action OnStuckRisk;
    public static event Action OnStuckIgnore;

    public static event Action OnStrangerNeedsHelp;
    public static event Action OnStrangerGiveResources;
    public static event Action OnStrangerDismiss;

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

    // =======================
    // 1. Kontrole
    public static void KnockAtDoor() => OnKnockAtDoor?.Invoke();
    public static void KnockOpenChoice() => OnKnockOpenChoice?.Invoke();
    public static void KnockHideChoice() => OnKnockHideChoice?.Invoke();

    public static void NeighborPeeking() => OnNeighborPeeking?.Invoke();
    public static void NeighborIgnored() => OnNeighborIgnored?.Invoke();
    public static void NeighborDismissed() => OnNeighborDismissed?.Invoke();

    public static void OchranaStepsHeard() => OnOchranaStepsHeard?.Invoke();
    public static void OchranaPauseAccepted() => OnOchranaPauseAccepted?.Invoke();
    public static void OchranaPauseDeclined() => OnOchranaPauseDeclined?.Invoke();

    public static void OfficerInspectionStarted(int itemsToHide) => OnOfficerInspectionStarted?.Invoke(itemsToHide);
    public static void OfficerInspectionSuccessful() => OnOfficerInspectionSuccessful?.Invoke();
    public static void OfficerInspectionFailed() => OnOfficerInspectionFailed?.Invoke();

    // 2. Zasoby
    public static void OutOfInk() => OnOutOfInk?.Invoke();
    public static void OutOfInkUseNow() => OnOutOfInkUseNow?.Invoke();
    public static void OutOfInkSave() => OnOutOfInkSave?.Invoke();

    public static void LostPaperBatch() => OnLostPaperBatch?.Invoke();
    public static void LostPaperPayInformer() => OnLostPaperPayInformer?.Invoke();
    public static void LostPaperIgnore() => OnLostPaperIgnore?.Invoke();

    public static void MoistureDamage() => OnMoistureDamage?.Invoke();
    public static void MoistureThrow() => OnMoistureThrow?.Invoke();
    public static void MoistureRisk() => OnMoistureRisk?.Invoke();

    public static void SecretDonation() => OnSecretDonation?.Invoke();
    public static void SecretDonationTake() => OnSecretDonationTake?.Invoke();
    public static void SecretDonationLeave() => OnSecretDonationLeave?.Invoke();

    // 3. Donosiciele / sąsiedzi
    public static void NeighborSawCourier() => OnNeighborSawCourier?.Invoke();
    public static void NeighborBribe() => OnNeighborBribe?.Invoke();

    public static void InformerAsks() => OnInformerAsks?.Invoke();
    public static void InformerLie() => OnInformerLie?.Invoke();
    public static void InformerDismiss() => OnInformerDismiss?.Invoke();
    public static void InformerIgnore() => OnInformerIgnore?.Invoke();

    public static void RumorsSpread() => OnRumorsSpread?.Invoke();

    // 4. Kurier / przesyłki
    public static void CourierInjured() => OnCourierInjured?.Invoke();
    public static void CourierHelp() => OnCourierHelp?.Invoke();
    public static void CourierIgnore() => OnCourierIgnore?.Invoke();

    public static void UrgentDelivery() => OnUrgentDelivery?.Invoke();
    public static void PackageUncertain() => OnPackageUncertain?.Invoke();
    public static void PackageOpen() => OnPackageOpen?.Invoke();
    public static void PackageWait() => OnPackageWait?.Invoke();

    // 5. Sabotage / niepewne kontakty
    public static void StuckHidingSpot() => OnStuckHidingSpot?.Invoke();
    public static void StuckRisk() => OnStuckRisk?.Invoke();
    public static void StuckIgnore() => OnStuckIgnore?.Invoke();

    public static void StrangerNeedsHelp() => OnStrangerNeedsHelp?.Invoke();
    public static void StrangerGiveResources() => OnStrangerGiveResources?.Invoke();
    public static void StrangerDismiss() => OnStrangerDismiss?.Invoke();

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
    public static void BuyPaperOffer() => OnBuyPaperOffer?.Invoke();

    public static void Arrest() => OnArrest?.Invoke();
}
