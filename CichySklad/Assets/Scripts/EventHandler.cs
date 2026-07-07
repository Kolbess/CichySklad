using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

/// <summary>
/// Listens to the global <see cref="GameEvents"/> channel and turns each narrative beat into a
/// dialogue prompt (with choices) that mutates risk and resources through injected managers. This
/// is the seam between the abstract event bus and the concrete scene systems.
/// </summary>
public class EventHandler : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Dialogue presenter used to show prompts and choices. Required.")]
    [FormerlySerializedAs("dialogueSystem")]
    [SerializeField]
    private DialogueSystem _dialogueSystem;

    [Tooltip("Inspection controller started/ended by control events. Required.")]
    [FormerlySerializedAs("inspectionSystem")]
    [SerializeField]
    private InspectionSystem _inspectionSystem;

    [Tooltip("Risk source adjusted by player choices. Required.")]
    [SerializeField]
    private RiskManager _riskManager;

    [Tooltip("Resource source adjusted by player choices. Required.")]
    [SerializeField]
    private ResourceManager _resourceManager;

    private void Awake()
    {
        Assert.IsNotNull(
            _dialogueSystem,
            $"[{nameof(EventHandler)}] DialogueSystem unassigned on {name}!"
        );
        Assert.IsNotNull(
            _inspectionSystem,
            $"[{nameof(EventHandler)}] InspectionSystem unassigned on {name}!"
        );
        Assert.IsNotNull(
            _riskManager,
            $"[{nameof(EventHandler)}] RiskManager unassigned on {name}!"
        );
        Assert.IsNotNull(
            _resourceManager,
            $"[{nameof(EventHandler)}] ResourceManager unassigned on {name}!"
        );
    }

    private void OnEnable()
    {
        // 1. Kontrole
        GameEvents.OnKnockAtDoor += HandleKnockAtDoor;
        GameEvents.OnNeighborPeeking += HandleNeighborPeeking;
        GameEvents.OnOchranaStepsHeard += HandleOchranaSteps;
        GameEvents.OnOfficerInspectionStarted += HandleOfficerInspectionStarted;

        // 2. Zasoby
        GameEvents.OnOutOfInk += HandleOutOfInk;
        GameEvents.OnLostPaperBatch += HandleLostPaper;
        GameEvents.OnMoistureDamage += HandleMoistureDamage;
        GameEvents.OnSecretDonation += HandleSecretDonation;

        // 3. Donosiciele / sąsiedzi
        GameEvents.OnNeighborSawCourier += HandleNeighborSawCourier;
        GameEvents.OnInformerAsks += HandleInformerAsks;
        GameEvents.OnRumorsSpread += HandleRumorsSpread;

        // 4. Kurier / przesyłki
        GameEvents.OnCourierInjured += HandleCourierInjured;
        GameEvents.OnUrgentDelivery += HandleUrgentDelivery;
        GameEvents.OnPackageUncertain += HandlePackageUncertain;

        // 5. Sabotage
        GameEvents.OnStuckHidingSpot += HandleStuckHidingSpot;
        GameEvents.OnStrangerNeedsHelp += HandleStrangerNeedsHelp;
        GameEvents.OnLampExplosion += HandleLampExplosion;

        // 6. Fabularne
        GameEvents.OnLetterFromPanKowal += HandleLetterFromPanKowal;
        GameEvents.OnMariaWarns += HandleMariaWarns;
        GameEvents.OnInformerDisappears += HandleInformerDisappears;

        // 7. Stresujące
        GameEvents.OnLoudNoise += HandleLoudNoise;
        GameEvents.OnFireCandle += HandleFireCandle;
        GameEvents.OnBrokenLock += HandleBrokenLock;

        // 8. Ekonomiczne
        GameEvents.OnOchranaBribe += HandleOchranaBribe;
        GameEvents.OnBuyPaperOffer += HandleBuyPaperOffer;

        GameEvents.OnArrest += HandleArrest;
    }

    private void OnDisable()
    {
        // 1. Kontrole
        GameEvents.OnKnockAtDoor -= HandleKnockAtDoor;
        GameEvents.OnNeighborPeeking -= HandleNeighborPeeking;
        GameEvents.OnOchranaStepsHeard -= HandleOchranaSteps;
        GameEvents.OnOfficerInspectionStarted -= HandleOfficerInspectionStarted;

        // 2. Zasoby
        GameEvents.OnOutOfInk -= HandleOutOfInk;
        GameEvents.OnLostPaperBatch -= HandleLostPaper;
        GameEvents.OnMoistureDamage -= HandleMoistureDamage;
        GameEvents.OnSecretDonation -= HandleSecretDonation;

        // 3. Donosiciele / sąsiedzi
        GameEvents.OnNeighborSawCourier -= HandleNeighborSawCourier;
        GameEvents.OnInformerAsks -= HandleInformerAsks;
        GameEvents.OnRumorsSpread -= HandleRumorsSpread;

        // 4. Kurier / przesyłki
        GameEvents.OnCourierInjured -= HandleCourierInjured;
        GameEvents.OnUrgentDelivery -= HandleUrgentDelivery;
        GameEvents.OnPackageUncertain -= HandlePackageUncertain;

        // 5. Sabotage
        GameEvents.OnStuckHidingSpot -= HandleStuckHidingSpot;
        GameEvents.OnStrangerNeedsHelp -= HandleStrangerNeedsHelp;
        GameEvents.OnLampExplosion -= HandleLampExplosion;

        // 6. Fabularne
        GameEvents.OnLetterFromPanKowal -= HandleLetterFromPanKowal;
        GameEvents.OnMariaWarns -= HandleMariaWarns;
        GameEvents.OnInformerDisappears -= HandleInformerDisappears;

        // 7. Stresujące
        GameEvents.OnLoudNoise -= HandleLoudNoise;
        GameEvents.OnFireCandle -= HandleFireCandle;
        GameEvents.OnBrokenLock -= HandleBrokenLock;

        // 8. Ekonomiczne
        GameEvents.OnOchranaBribe -= HandleOchranaBribe;
        GameEvents.OnBuyPaperOffer -= HandleBuyPaperOffer;

        GameEvents.OnArrest -= HandleArrest;
    }

    // =======================
    // 1. Kontrole
    private void HandleKnockAtDoor()
    {
        var choices = new DialogueSystem.Choice[]
        {
            new DialogueSystem.Choice(
                "Otwórz",
                () =>
                {
                    _riskManager.ReduceRisk(10);
                    _inspectionSystem.StartInspection();
                }
            ),
            new DialogueSystem.Choice(
                "Nie otwieraj",
                () =>
                {
                    _riskManager.AddRisk(10);
                    _inspectionSystem.StartInspection();
                }
            ),
        };
        _dialogueSystem.ShowDialogueWithChoices(
            "Puk... Puk...",
            choices,
            _dialogueSystem.GetPortraitSprite("ochrana")
        );
    }

    private void HandleNeighborPeeking()
    {
        var choices = new DialogueSystem.Choice[]
        {
            new DialogueSystem.Choice("Zignoruj", () => _riskManager.AddRisk(15)),
            new DialogueSystem.Choice("Grzecznie wyproś", () => _riskManager.ReduceRisk(5)),
        };
        _dialogueSystem.ShowDialogueWithChoices(
            "Mmm... A co on tam robi, może donosik?",
            choices,
            _dialogueSystem.GetPortraitSprite("neighbour")
        );
    }

    private void HandleOchranaSteps()
    {
        var choices = new DialogueSystem.Choice[]
        {
            new DialogueSystem.Choice("Pause actions", GameEvents.OchranaPauseAccepted),
            new DialogueSystem.Choice("Continue working", GameEvents.OchranaPauseDeclined),
        };
        _dialogueSystem.ShowDialogueWithChoices("You hear Ochrana's footsteps outside.", choices);
    }

    private void HandleOfficerInspectionStarted(int itemsToHide)
    {
        _dialogueSystem.ShowDialogue($"Officer is inspecting! Hide {itemsToHide} items quickly!");
    }

    // =======================
    // 2. Zasoby
    private void HandleOutOfInk()
    {
        var choices = new DialogueSystem.Choice[]
        {
            new DialogueSystem.Choice("Use remaining ink", GameEvents.OutOfInkUseNow),
            new DialogueSystem.Choice("Save it", GameEvents.OutOfInkSave),
        };
        _dialogueSystem.ShowDialogueWithChoices("You're out of ink!", choices);
    }

    private void HandleLostPaper()
    {
        var choices = new DialogueSystem.Choice[]
        {
            new DialogueSystem.Choice("Pay informer", GameEvents.LostPaperPayInformer),
            new DialogueSystem.Choice("Ignore", GameEvents.LostPaperIgnore),
        };
        _dialogueSystem.ShowDialogueWithChoices("A paper batch was lost!", choices);
    }

    private void HandleMoistureDamage()
    {
        var choices = new DialogueSystem.Choice[]
        {
            new DialogueSystem.Choice("Throw damaged paper", GameEvents.MoistureThrow),
            new DialogueSystem.Choice("Use risky paper", GameEvents.MoistureRisk),
        };
        _dialogueSystem.ShowDialogueWithChoices("Some paper got wet.", choices);
    }

    private void HandleSecretDonation()
    {
        var choices = new DialogueSystem.Choice[]
        {
            new DialogueSystem.Choice("Take donation", GameEvents.SecretDonationTake),
            new DialogueSystem.Choice("Leave it", GameEvents.SecretDonationLeave),
        };
        _dialogueSystem.ShowDialogueWithChoices("A secret donation appears!", choices);
    }

    // =======================
    // 3. Donosiciele / sąsiedzi
    private void HandleNeighborSawCourier()
    {
        var choices = new DialogueSystem.Choice[]
        {
            new DialogueSystem.Choice("Bribe neighbor", GameEvents.NeighborBribe),
            new DialogueSystem.Choice("Do nothing", () => _resourceManager.AddTrust(-5)),
        };
        _dialogueSystem.ShowDialogueWithChoices("Neighbor saw the courier!", choices);
    }

    private void HandleInformerAsks()
    {
        var choices = new DialogueSystem.Choice[]
        {
            new DialogueSystem.Choice("Lie", GameEvents.InformerLie),
            new DialogueSystem.Choice("Dismiss", GameEvents.InformerDismiss),
            new DialogueSystem.Choice("Ignore", GameEvents.InformerIgnore),
        };
        _dialogueSystem.ShowDialogueWithChoices("Informer is asking about your work.", choices);
    }

    private void HandleRumorsSpread()
    {
        _dialogueSystem.ShowDialogue("Rumors are spreading. Passive risk increased.");
        _riskManager.AddRisk(2);
    }

    // =======================
    // 4. Kurier / przesyłki
    private void HandleCourierInjured()
    {
        var choices = new DialogueSystem.Choice[]
        {
            new DialogueSystem.Choice(
                "Pomóż",
                () =>
                {
                    _riskManager.AddRisk(10);
                    _resourceManager.AddTrust(10);
                }
            ),
            new DialogueSystem.Choice(
                "Zignoruj",
                () =>
                {
                    _resourceManager.AddTrust(-5);
                    _riskManager.ReduceRisk(5);
                }
            ),
        };
        _dialogueSystem.ShowDialogueWithChoices(
            "Pomóż mi jestem twoim kurierem",
            choices,
            _dialogueSystem.GetPortraitSprite("kowal")
        );
    }

    private void HandleUrgentDelivery()
    {
        _dialogueSystem.ShowDialogue("Maria delivered urgent materials. Make space quickly!");
    }

    private void HandlePackageUncertain()
    {
        var choices = new DialogueSystem.Choice[]
        {
            new DialogueSystem.Choice("Open package", GameEvents.PackageOpen),
            new DialogueSystem.Choice("Wait", GameEvents.PackageWait),
        };
        _dialogueSystem.ShowDialogueWithChoices("Package contents are uncertain.", choices);
    }

    // =======================
    // 5. Sabotage
    private void HandleStuckHidingSpot()
    {
        var choices = new DialogueSystem.Choice[]
        {
            new DialogueSystem.Choice("Risk it", GameEvents.StuckRisk),
            new DialogueSystem.Choice("Ignore", GameEvents.StuckIgnore),
        };
        _dialogueSystem.ShowDialogueWithChoices("Hiding spot is stuck!", choices);
    }

    private void HandleStrangerNeedsHelp()
    {
        var choices = new DialogueSystem.Choice[]
        {
            new DialogueSystem.Choice("Give resources", GameEvents.StrangerGiveResources),
            new DialogueSystem.Choice("Dismiss", GameEvents.StrangerDismiss),
        };
        _dialogueSystem.ShowDialogueWithChoices("A stranger asks for help.", choices);
    }

    private void HandleLampExplosion()
    {
        _dialogueSystem.ShowDialogue("Small lamp explosion! Risk increased slightly.");
        _riskManager.AddRisk(2);
    }

    // =======================
    // 6. Fabularne
    private void HandleLetterFromPanKowal()
    {
        _dialogueSystem.ShowDialogue(
            "Letter from Pan Kowal received. Contains instructions or moral dilemma."
        );
    }

    private void HandleMariaWarns()
    {
        _dialogueSystem.ShowDialogue("Maria warns you of possible inspection tomorrow.");
        _riskManager.AddRisk(1);
    }

    private void HandleInformerDisappears()
    {
        _dialogueSystem.ShowDialogue("Informer has disappeared. Situation uncertain.");
    }

    // =======================
    // 7. Stresujące
    private void HandleLoudNoise()
    {
        _dialogueSystem.ShowDialogue("Loud noise! Quickly hide suspicious items!");
    }

    private void HandleFireCandle()
    {
        _resourceManager.TrySpend(costPaper: Mathf.Abs(_resourceManager.Paper / 5));
        _dialogueSystem.ShowDialogue("Candle caught fire! Lost some paper.");
    }

    private void HandleBrokenLock()
    {
        _riskManager.AddRisk(3);
        _dialogueSystem.ShowDialogue("Broken lock! Risk increased for the next event.");
    }

    // =======================
    // 8. Ekonomiczne
    private void HandleOchranaBribe()
    {
        const int bribe = 5; // w rublach

        var choices = new DialogueSystem.Choice[]
        {
            new DialogueSystem.Choice(
                $"Zapłać łapówkę:\n {bribe} rubli",
                () =>
                {
                    if (_resourceManager.TrySpend(costMoney: bribe))
                    {
                        _riskManager.ReduceRisk(_riskManager.CurrentRisk / 2);
                        _inspectionSystem.EndCatching();
                    }
                    else
                    {
                        GameEvents.Arrest();
                    }
                }
            ),
            new DialogueSystem.Choice(
                "Odmów",
                () =>
                {
                    _riskManager.AddRisk(5);
                    _inspectionSystem.EndCatching();
                }
            ),
        };

        _dialogueSystem.ShowDialogueWithChoices(
            "Płać rublem albo płacz",
            choices,
            _dialogueSystem.GetPortraitSprite("ochrana")
        );
    }

    private void HandleBuyPaperOffer()
    {
        var choices = new DialogueSystem.Choice[]
        {
            new DialogueSystem.Choice("Invest in cheaper paper", GameEvents.BuyPaperOffer),
            new DialogueSystem.Choice("Ignore offer", () => { }),
        };
        _dialogueSystem.ShowDialogueWithChoices(
            "Opportunity to buy cheaper paper. Choose wisely.",
            choices
        );
    }

    private void HandleArrest()
    {
        _dialogueSystem.ShowDialogue("Zostałeś Aresztowany! Koniec Gry.");
    }
}
