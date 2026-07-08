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
            // Lay low: printing stops, so accumulated suspicion cools off.
            new DialogueSystem.Choice("Przerwij pracę", () => _riskManager.ReduceRisk(10)),
            // Keep printing while they are right outside — reckless, spikes risk.
            new DialogueSystem.Choice("Pracuj dalej", () => _riskManager.AddRisk(12)),
        };
        _dialogueSystem.ShowDialogueWithChoices(
            "Słyszysz kroki Ochrany za drzwiami.",
            choices,
            _dialogueSystem.GetPortraitSprite("ochrana")
        );
    }

    private void HandleOfficerInspectionStarted(int itemsToHide)
    {
        _dialogueSystem.ShowDialogue($"Kontrola! Szybko schowaj {itemsToHide} przedmiotów!");
    }

    // =======================
    // 2. Zasoby
    private void HandleOutOfInk()
    {
        var choices = new DialogueSystem.Choice[]
        {
            // Burn the last drops on one final print run.
            new DialogueSystem.Choice("Zużyj resztki tuszu", () => _resourceManager.AddLeaflets(2)),
            // Stretch the supply — a little ink kept in reserve.
            new DialogueSystem.Choice("Oszczędzaj tusz", () => _resourceManager.AddInk(1)),
        };
        _dialogueSystem.ShowDialogueWithChoices("Skończył ci się tusz!", choices);
    }

    private void HandleLostPaper()
    {
        var choices = new DialogueSystem.Choice[]
        {
            // Buy the batch back — costs money, otherwise the loss draws suspicion.
            new DialogueSystem.Choice(
                "Zapłać donosicielowi (3 ruble)",
                () =>
                {
                    if (_resourceManager.TrySpend(costMoney: 3))
                        _resourceManager.AddPaper(3);
                    else
                        _riskManager.AddRisk(10);
                }
            ),
            new DialogueSystem.Choice("Odpuść", () => _riskManager.AddRisk(5)),
        };
        _dialogueSystem.ShowDialogueWithChoices("Przepadła partia papieru!", choices);
    }

    private void HandleMoistureDamage()
    {
        var choices = new DialogueSystem.Choice[]
        {
            // Bin the ruined sheet — safe, but a unit of paper is gone.
            new DialogueSystem.Choice(
                "Wyrzuć zniszczony papier",
                () => _resourceManager.TrySpend(costPaper: 1)
            ),
            // Print on damp paper anyway: leaflets now, but smudged output is risky.
            new DialogueSystem.Choice(
                "Użyj wilgotnego papieru",
                () =>
                {
                    _resourceManager.AddLeaflets(2);
                    _riskManager.AddRisk(8);
                }
            ),
        };
        _dialogueSystem.ShowDialogueWithChoices("Część papieru zawilgła.", choices);
    }

    private void HandleSecretDonation()
    {
        var choices = new DialogueSystem.Choice[]
        {
            // Pocket the cash, but an unknown benefactor could be a trap.
            new DialogueSystem.Choice(
                "Weź datek",
                () =>
                {
                    _resourceManager.AddMoney(5);
                    _riskManager.AddRisk(8);
                }
            ),
            // Refuse it — cautious, and the network respects the restraint.
            new DialogueSystem.Choice("Zostaw", () => _resourceManager.AddTrust(5)),
        };
        _dialogueSystem.ShowDialogueWithChoices("Pojawił się tajny datek!", choices);
    }

    // =======================
    // 3. Donosiciele / sąsiedzi
    private void HandleNeighborSawCourier()
    {
        var choices = new DialogueSystem.Choice[]
        {
            // Buy silence — costs money, otherwise the neighbour talks and risk climbs.
            new DialogueSystem.Choice(
                "Przekup sąsiada (3 ruble)",
                () =>
                {
                    if (_resourceManager.TrySpend(costMoney: 3))
                        _riskManager.ReduceRisk(10);
                    else
                        _riskManager.AddRisk(10);
                }
            ),
            new DialogueSystem.Choice("Nic nie rób", () => _resourceManager.AddTrust(-5)),
        };
        _dialogueSystem.ShowDialogueWithChoices("Sąsiad widział kuriera!", choices);
    }

    private void HandleInformerAsks()
    {
        var choices = new DialogueSystem.Choice[]
        {
            // A convincing lie deflects the immediate suspicion.
            new DialogueSystem.Choice("Skłam", () => _riskManager.ReduceRisk(5)),
            // Send him off curtly — a rebuff that still draws some attention.
            new DialogueSystem.Choice("Odpraw go", () => _riskManager.AddRisk(5)),
            // Say nothing at all — silence reads as guilt and stokes suspicion most.
            new DialogueSystem.Choice("Zignoruj", () => _riskManager.AddRisk(8)),
        };
        _dialogueSystem.ShowDialogueWithChoices("Donosiciel wypytuje o twoją pracę.", choices);
    }

    private void HandleRumorsSpread()
    {
        _dialogueSystem.ShowDialogue("Rozchodzą się plotki. Ryzyko rośnie.");
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
        _dialogueSystem.ShowDialogue("Maria dostarczyła pilne materiały. Szybko zrób miejsce!");
    }

    private void HandlePackageUncertain()
    {
        var choices = new DialogueSystem.Choice[]
        {
            // Open it now: useful supplies, but handling unknown contents is exposure.
            new DialogueSystem.Choice(
                "Otwórz paczkę",
                () =>
                {
                    _resourceManager.AddPaper(2);
                    _resourceManager.AddInk(1);
                    _riskManager.AddRisk(5);
                }
            ),
            // Hold off until it's safe — patience lowers the heat.
            new DialogueSystem.Choice("Poczekaj", () => _riskManager.ReduceRisk(5)),
        };
        _dialogueSystem.ShowDialogueWithChoices("Zawartość paczki jest niepewna.", choices);
    }

    // =======================
    // 5. Sabotage
    private void HandleStuckHidingSpot()
    {
        var choices = new DialogueSystem.Choice[]
        {
            // Force it open — the noise is a big risk spike.
            new DialogueSystem.Choice("Szarp na siłę", () => _riskManager.AddRisk(10)),
            // Leave it jammed — contraband stays poorly hidden, a smaller lingering risk.
            new DialogueSystem.Choice("Zostaw zacięty schowek", () => _riskManager.AddRisk(4)),
        };
        _dialogueSystem.ShowDialogueWithChoices("Schowek się zaciął!", choices);
    }

    private void HandleStrangerNeedsHelp()
    {
        var choices = new DialogueSystem.Choice[]
        {
            // Share what you can spare: earns trust, or draws attention if you're broke.
            new DialogueSystem.Choice(
                "Podziel się zasobami (2 ruble)",
                () =>
                {
                    if (_resourceManager.TrySpend(costMoney: 2))
                        _resourceManager.AddTrust(8);
                    else
                        _riskManager.AddRisk(5);
                }
            ),
            // Turn them away — safe, but word of the cold shoulder costs standing.
            new DialogueSystem.Choice("Odpraw nieznajomego", () => _resourceManager.AddTrust(-5)),
        };
        _dialogueSystem.ShowDialogueWithChoices("Nieznajomy prosi o pomoc.", choices);
    }

    private void HandleLampExplosion()
    {
        _dialogueSystem.ShowDialogue("Mała eksplozja lampy! Ryzyko nieco wzrosło.");
        _riskManager.AddRisk(2);
    }

    // =======================
    // 6. Fabularne
    private void HandleLetterFromPanKowal()
    {
        _dialogueSystem.ShowDialogue(
            "Nadszedł list od Pana Kowala. Zawiera instrukcje i moralny dylemat."
        );
    }

    private void HandleMariaWarns()
    {
        _dialogueSystem.ShowDialogue("Maria ostrzega przed możliwą kontrolą jutro.");
        _riskManager.AddRisk(1);
    }

    private void HandleInformerDisappears()
    {
        _dialogueSystem.ShowDialogue("Donosiciel zniknął. Sytuacja jest niepewna.");
    }

    // =======================
    // 7. Stresujące
    private void HandleLoudNoise()
    {
        _dialogueSystem.ShowDialogue("Głośny hałas! Szybko schowaj podejrzane przedmioty!");
    }

    private void HandleFireCandle()
    {
        _resourceManager.TrySpend(costPaper: Mathf.Abs(_resourceManager.Paper / 5));
        _dialogueSystem.ShowDialogue("Świeca zajęła się ogniem! Straciłeś trochę papieru.");
    }

    private void HandleBrokenLock()
    {
        _riskManager.AddRisk(3);
        _dialogueSystem.ShowDialogue("Zepsuty zamek! Ryzyko wzrosło do następnego zdarzenia.");
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
            // A bargain if you can pay; if you can't, the dealer's grumbling raises your profile.
            new DialogueSystem.Choice(
                "Zainwestuj w tańszy papier (4 ruble)",
                () =>
                {
                    if (_resourceManager.TrySpend(costMoney: 4))
                        _resourceManager.AddPaper(6);
                    else
                        _riskManager.AddRisk(3);
                }
            ),
            // Walking away from a shady contact keeps you discreet — a small trust gain.
            new DialogueSystem.Choice("Odrzuć ofertę", () => _resourceManager.AddTrust(2)),
        };
        _dialogueSystem.ShowDialogueWithChoices("Okazja: tańszy papier. Wybierz mądrze.", choices);
    }

    private void HandleArrest()
    {
        _dialogueSystem.ShowDialogue("Zostałeś Aresztowany! Koniec Gry.");
    }
}
