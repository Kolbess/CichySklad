using UnityEngine;

public class EventHandler : MonoBehaviour
{
    [SerializeField] private DialogueSystem dialogueSystem;
    [SerializeField] private InspectionSystem inspectionSystem;

    private void OnEnable()
    {
        // =======================
        // 1. Kontrole
        EventSystem.OnKnockAtDoor += HandleKnockAtDoor;
        EventSystem.OnNeighborPeeking += HandleNeighborPeeking;
        EventSystem.OnOchranaStepsHeard += HandleOchranaSteps;
        EventSystem.OnOfficerInspectionStarted += HandleOfficerInspectionStarted;

        // =======================
        // 2. Zasoby
        EventSystem.OnOutOfInk += HandleOutOfInk;
        EventSystem.OnLostPaperBatch += HandleLostPaper;
        EventSystem.OnMoistureDamage += HandleMoistureDamage;
        EventSystem.OnSecretDonation += HandleSecretDonation;

        // =======================
        // 3. Donosiciele / sąsiedzi
        EventSystem.OnNeighborSawCourier += HandleNeighborSawCourier;
        EventSystem.OnInformerAsks += HandleInformerAsks;
        EventSystem.OnRumorsSpread += HandleRumorsSpread;

        // =======================
        // 4. Kurier / przesyłki
        EventSystem.OnCourierInjured += HandleCourierInjured;
        EventSystem.OnUrgentDelivery += HandleUrgentDelivery;
        EventSystem.OnPackageUncertain += HandlePackageUncertain;

        // =======================
        // 5. Sabotage
        EventSystem.OnStuckHidingSpot += HandleStuckHidingSpot;
        EventSystem.OnStrangerNeedsHelp += HandleStrangerNeedsHelp;
        EventSystem.OnLampExplosion += HandleLampExplosion;

        // =======================
        // 6. Fabularne
        EventSystem.OnLetterFromPanKowal += HandleLetterFromPanKowal;
        EventSystem.OnMariaWarns += HandleMariaWarns;
        EventSystem.OnInformerDisappears += HandleInformerDisappears;

        // =======================
        // 7. Stresujące
        EventSystem.OnLoudNoise += HandleLoudNoise;
        EventSystem.OnFireCandle += HandleFireCandle;
        EventSystem.OnBrokenLock += HandleBrokenLock;

        // =======================
        // 8. Ekonomiczne
        EventSystem.OnOchranaBribe += HandleOchranaBribe;
        EventSystem.OnBuyPaperOffer += HandleBuyPaperOffer;

        EventSystem.OnArrest += HandleArrest;
    }

    private void OnDisable()
    {
        // =======================
        // 1. Kontrole
        EventSystem.OnKnockAtDoor -= HandleKnockAtDoor;
        EventSystem.OnNeighborPeeking -= HandleNeighborPeeking;
        EventSystem.OnOchranaStepsHeard -= HandleOchranaSteps;
        EventSystem.OnOfficerInspectionStarted -= HandleOfficerInspectionStarted;

        // =======================
        // 2. Zasoby
        EventSystem.OnOutOfInk -= HandleOutOfInk;
        EventSystem.OnLostPaperBatch -= HandleLostPaper;
        EventSystem.OnMoistureDamage -= HandleMoistureDamage;
        EventSystem.OnSecretDonation -= HandleSecretDonation;

        // =======================
        // 3. Donosiciele / sąsiedzi
        EventSystem.OnNeighborSawCourier -= HandleNeighborSawCourier;
        EventSystem.OnInformerAsks -= HandleInformerAsks;
        EventSystem.OnRumorsSpread -= HandleRumorsSpread;

        // =======================
        // 4. Kurier / przesyłki
        EventSystem.OnCourierInjured -= HandleCourierInjured;
        EventSystem.OnUrgentDelivery -= HandleUrgentDelivery;
        EventSystem.OnPackageUncertain -= HandlePackageUncertain;

        // =======================
        // 5. Sabotage
        EventSystem.OnStuckHidingSpot -= HandleStuckHidingSpot;
        EventSystem.OnStrangerNeedsHelp -= HandleStrangerNeedsHelp;
        EventSystem.OnLampExplosion -= HandleLampExplosion;

        // =======================
        // 6. Fabularne
        EventSystem.OnLetterFromPanKowal -= HandleLetterFromPanKowal;
        EventSystem.OnMariaWarns -= HandleMariaWarns;
        EventSystem.OnInformerDisappears -= HandleInformerDisappears;

        // =======================
        // 7. Stresujące
        EventSystem.OnLoudNoise -= HandleLoudNoise;
        EventSystem.OnFireCandle -= HandleFireCandle;
        EventSystem.OnBrokenLock -= HandleBrokenLock;

        // =======================
        // 8. Ekonomiczne
        EventSystem.OnOchranaBribe -= HandleOchranaBribe;
        EventSystem.OnBuyPaperOffer -= HandleBuyPaperOffer;

        EventSystem.OnArrest -= HandleArrest;
    }

    // =======================
    // 1. Kontrole
    private void HandleKnockAtDoor()
    {
        var choices = new DialogueSystem.Choice[]
        {
            new DialogueSystem.Choice("Otwórz", () =>
            {
                RiskManager.Instance.ReduceRisk(10);
                inspectionSystem.StartInspection();
            }),
            new DialogueSystem.Choice("Nie otwieraj", () =>
            {
                RiskManager.Instance.AddRisk(10);
                inspectionSystem.StartInspection();
            })
        };
        dialogueSystem.ShowDialogueWithChoices("Puk... Puk...", choices, dialogueSystem.GetPortraitSprite("ochrana"));
    }

    private void HandleNeighborPeeking()
    {
        var choices = new DialogueSystem.Choice[]
        {
            new DialogueSystem.Choice("Zignoruj", () =>
            {
                RiskManager.Instance.AddRisk(15);
            }),
            new DialogueSystem.Choice("Grzecznie wyproś", () => RiskManager.Instance.ReduceRisk(5))
        };
        dialogueSystem.ShowDialogueWithChoices("Mmm... A co on tam robi, może donosik?", choices, dialogueSystem.GetPortraitSprite("neighbour"));
    }

    private void HandleOchranaSteps()
    {
        var choices = new DialogueSystem.Choice[]
        {
            new DialogueSystem.Choice("Pause actions", () => EventSystem.OchranaPauseAccepted()),
            new DialogueSystem.Choice("Continue working", () => EventSystem.OchranaPauseDeclined())
        };
        dialogueSystem.ShowDialogueWithChoices("You hear Ochrana's footsteps outside.", choices);
    }

    private void HandleOfficerInspectionStarted(int itemsToHide)
    {
        dialogueSystem.ShowDialogue($"Officer is inspecting! Hide {itemsToHide} items quickly!");
        // Here the actual logic for hiding items would be in your game code
    }

    // =======================
    // 2. Zasoby
    private void HandleOutOfInk()
    {
        var choices = new DialogueSystem.Choice[]
        {
            new DialogueSystem.Choice("Use remaining ink", () => EventSystem.OutOfInkUseNow()),
            new DialogueSystem.Choice("Save it", () => EventSystem.OutOfInkSave())
        };
        dialogueSystem.ShowDialogueWithChoices("You're out of ink!", choices);
    }

    private void HandleLostPaper()
    {
        var choices = new DialogueSystem.Choice[]
        {
            new DialogueSystem.Choice("Pay informer", () => EventSystem.LostPaperPayInformer()),
            new DialogueSystem.Choice("Ignore", () => EventSystem.LostPaperIgnore())
        };
        dialogueSystem.ShowDialogueWithChoices("A paper batch was lost!", choices);
    }

    private void HandleMoistureDamage()
    {
        var choices = new DialogueSystem.Choice[]
        {
            new DialogueSystem.Choice("Throw damaged paper", () => EventSystem.MoistureThrow()),
            new DialogueSystem.Choice("Use risky paper", () => EventSystem.MoistureRisk())
        };
        dialogueSystem.ShowDialogueWithChoices("Some paper got wet.", choices);
    }

    private void HandleSecretDonation()
    {
        var choices = new DialogueSystem.Choice[]
        {
            new DialogueSystem.Choice("Take donation", () => EventSystem.SecretDonationTake()),
            new DialogueSystem.Choice("Leave it", () => EventSystem.SecretDonationLeave())
        };
        dialogueSystem.ShowDialogueWithChoices("A secret donation appears!", choices);
    }

    // =======================
    // 3. Donosiciele / sąsiedzi
    private void HandleNeighborSawCourier()
    {
        var choices = new DialogueSystem.Choice[]
        {
            new DialogueSystem.Choice("Bribe neighbor", () => EventSystem.NeighborBribe()),
            new DialogueSystem.Choice("Do nothing", () => { ResourceManager.Instance.trust -= 5; })
        };
        dialogueSystem.ShowDialogueWithChoices("Neighbor saw the courier!", choices);
    }

    private void HandleInformerAsks()
    {
        var choices = new DialogueSystem.Choice[]
        {
            new DialogueSystem.Choice("Lie", () => EventSystem.InformerLie()),
            new DialogueSystem.Choice("Dismiss", () => EventSystem.InformerDismiss()),
            new DialogueSystem.Choice("Ignore", () => EventSystem.InformerIgnore())
        };
        dialogueSystem.ShowDialogueWithChoices("Informer is asking about your work.", choices);
    }

    private void HandleRumorsSpread()
    {
        dialogueSystem.ShowDialogue("Rumors are spreading. Passive risk increased.");
        RiskManager.Instance.AddRisk(2);
    }

    // =======================
    // 4. Kurier / przesyłki
    private void HandleCourierInjured()
    {
        var choices = new DialogueSystem.Choice[]
        {
            new DialogueSystem.Choice("Pomóż", () =>
            {
                RiskManager.Instance.AddRisk(10);
                ResourceManager.Instance.trust += 10;
            }),
            new DialogueSystem.Choice("Zignoruj", () =>
            {
                ResourceManager.Instance.trust -= 5;
                RiskManager.Instance.ReduceRisk(5);
            })
        };
        dialogueSystem.ShowDialogueWithChoices("Pomóż mi jestem twoim kurierem", choices, dialogueSystem.GetPortraitSprite("kowal"));
    }

    private void HandleUrgentDelivery()
    {
        dialogueSystem.ShowDialogue("Maria delivered urgent materials. Make space quickly!");
    }

    private void HandlePackageUncertain()
    {
        var choices = new DialogueSystem.Choice[]
        {
            new DialogueSystem.Choice("Open package", () => EventSystem.PackageOpen()),
            new DialogueSystem.Choice("Wait", () => EventSystem.PackageWait())
        };
        dialogueSystem.ShowDialogueWithChoices("Package contents are uncertain.", choices);
    }

    // =======================
    // 5. Sabotage
    private void HandleStuckHidingSpot()
    {
        var choices = new DialogueSystem.Choice[]
        {
            new DialogueSystem.Choice("Risk it", () => EventSystem.StuckRisk()),
            new DialogueSystem.Choice("Ignore", () => EventSystem.StuckIgnore())
        };
        dialogueSystem.ShowDialogueWithChoices("Hiding spot is stuck!", choices);
    }

    private void HandleStrangerNeedsHelp()
    {
        var choices = new DialogueSystem.Choice[]
        {
            new DialogueSystem.Choice("Give resources", () => EventSystem.StrangerGiveResources()),
            new DialogueSystem.Choice("Dismiss", () => EventSystem.StrangerDismiss())
        };
        dialogueSystem.ShowDialogueWithChoices("A stranger asks for help.", choices);
    }

    private void HandleLampExplosion()
    {
        dialogueSystem.ShowDialogue("Small lamp explosion! Risk increased slightly.");
        RiskManager.Instance.AddRisk(2);
    }

    // =======================
    // 6. Fabularne
    private void HandleLetterFromPanKowal()
    {
        dialogueSystem.ShowDialogue("Letter from Pan Kowal received. Contains instructions or moral dilemma.");
    }

    private void HandleMariaWarns()
    {
        dialogueSystem.ShowDialogue("Maria warns you of possible inspection tomorrow.");
        RiskManager.Instance.AddRisk(1);
    }

    private void HandleInformerDisappears()
    {
        dialogueSystem.ShowDialogue("Informer has disappeared. Situation uncertain.");
    }

    // =======================
    // 7. Stresujące
    private void HandleLoudNoise()
    {
        dialogueSystem.ShowDialogue("Loud noise! Quickly hide suspicious items!");
    }

    private void HandleFireCandle()
    {
        ResourceManager.Instance.TrySpend(costPaper: Mathf.Abs(ResourceManager.Instance.paper / 5));
        dialogueSystem.ShowDialogue("Candle caught fire! Lost some paper.");
    }

    private void HandleBrokenLock()
    {
        RiskManager.Instance.AddRisk(3);
        dialogueSystem.ShowDialogue("Broken lock! Risk increased for the next event.");
    }

    // =======================
    // 8. Ekonomiczne
    private void HandleOchranaBribe()
    {
        const int bribe = 5; // w rublach

        var choices = new DialogueSystem.Choice[]
        {
            new DialogueSystem.Choice($"Zapłać łapówkę:\n {bribe} rubli", () =>
            {
                if (ResourceManager.Instance.TrySpend(costMoney: bribe))
                {
                    RiskManager.Instance.ReduceRisk(RiskManager.Instance.CurrentRisk / 2);
                    inspectionSystem.isCatching = false; // koniec eventu
                }
                else
                {
                    Debug.Log("Trigger Arrest!");
                    EventSystem.Arrest();
                }
            }),

            new DialogueSystem.Choice("Odmów", () =>
            {
                RiskManager.Instance.AddRisk(5);
                inspectionSystem.isCatching = false; // koniec eventu
            })
        };

        dialogueSystem.ShowDialogueWithChoices(
            "Płać rublem albo płacz",
            choices,
            dialogueSystem.GetPortraitSprite("ochrana")
        );
    }


    private void HandleBuyPaperOffer()
    {
        var choices = new DialogueSystem.Choice[]
        {
            new DialogueSystem.Choice("Invest in cheaper paper", () => EventSystem.BuyPaperOffer()),
            new DialogueSystem.Choice("Ignore offer", () => { /* nothing */ })
        };
        dialogueSystem.ShowDialogueWithChoices("Opportunity to buy cheaper paper. Choose wisely.", choices);
    }

    private void HandleArrest()
    {
        dialogueSystem.ShowDialogue("Zostałeś Aresztowany! Koniec Gry.");
    }
}
