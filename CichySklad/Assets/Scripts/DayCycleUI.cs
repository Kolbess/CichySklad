using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DayCycleUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogueSystem dialogueSystem;

    [Header("Day Transition")]
    [SerializeField] private GameObject dayTransitionPanel;
    [SerializeField] private TextMeshProUGUI dayTitleText;
    [SerializeField] private float transitionDuration = 2f;

    [Header("Morning Events")]
    [SerializeField] private Sprite mariaPortrait;
    [SerializeField] private GameObject kowalReportPanel;
    [SerializeField] private TextMeshProUGUI kowalReportText;

    [Header("Evening Events")]
    [SerializeField] private GameObject eveningReportPanel;
    [SerializeField] private TextMeshProUGUI eveningSummaryText;
    [SerializeField] private Sprite neighborPortrait;

    [Header("Background Visuals")]
    [SerializeField] private Image uiBackgroundImage;
    [SerializeField] private SpriteRenderer worldBackgroundRenderer;
    [SerializeField] private List<Sprite> daysprites;

    private void Start()
    {
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.OnDayStarted += HandleDayStarted;
            DayCycleManager.Instance.OnPhaseChanged += HandlePhaseChanged;
            DayCycleManager.Instance.OnDayEnded += HandleDayEnded;
        }
        
        // Auto-find DialogueSystem if not assigned
        if (dialogueSystem == null)
        {
            dialogueSystem = FindObjectOfType<DialogueSystem>();
        }

        // Initialize background
        if (DayCycleManager.Instance != null)
        {
            // Force update to initial state immediately
            UpdateBackgroundSprite(0); 
        }
    }

    private void OnDestroy()
    {
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.OnDayStarted -= HandleDayStarted;
            DayCycleManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
            DayCycleManager.Instance.OnDayEnded -= HandleDayEnded;
        }
    }

    private void HandleDayStarted(int day)
    {
        StartCoroutine(ShowDayStartSequence(day));
    }

    private IEnumerator ShowDayStartSequence(int day)
    {
        // 1. Show Day Title (Black screen with text)
        dayTransitionPanel.SetActive(true);
        dayTitleText.text = $"Dzień {day}";
        yield return new WaitForSeconds(transitionDuration);
        dayTransitionPanel.SetActive(false);

        // 2. Show Maria's Message via DialogueSystem
        ShowMariaMessage();
    }

    private void HandlePhaseChanged(DayPhase phase)
    {
        UpdateBackground(phase);

        switch (phase)
        {
            case DayPhase.Work:
                // Dialogue auto-hides or we can force hide if needed
                ShowKowalReport();
                break;
            case DayPhase.Evening:
                HideKowalReport();
                ShowEveningSequence();
                break;
            case DayPhase.Night:
                // Handled in HandleDayEnded usually, or here
                break;
        }
    }

    public void HandleDayEnded()
    {
        // Show final "Day Ended" screen or fade out
        dayTransitionPanel.SetActive(true);
        dayTitleText.text = "Noc - Dzień Zakończony";
    }

    // --- Morning Logic ---

    private void ShowMariaMessage()
    {
        if (dialogueSystem != null)
        {
            string message = GetRandomMariaMessage();
            dialogueSystem.ShowDialogue(message, mariaPortrait);
        }
    }

    private string GetRandomMariaMessage()
    {
        if (DayCycleManager.Instance != null)
        {
            int hide = DayCycleManager.Instance.TargetLeafletsToHide;
            int send = DayCycleManager.Instance.TargetLeafletsToSend;
            return $"Schowaj dziś {hide} ulotek.\nPrześlij {send} paczki.\nOchrana jest czujna.";
        }
        
        return "Brak wytycznych na dziś.";
    }

    private void ShowKowalReport()
    {
        kowalReportPanel.SetActive(true);
        kowalReportText.text = Random.value > 0.5f ? "Dziś ważne. Zachowaj ostrożność." : "Czas działa na naszą korzyść. Zajmij się dystrybucją.";
    }

    private void HideKowalReport()
    {
        kowalReportPanel.SetActive(false);
    }

    // --- Evening Logic ---

    private void ShowEveningSequence()
    {
        StartCoroutine(EveningRoutine());
    }

    private IEnumerator EveningRoutine()
    {
        // 1. Patrol Animation/Sound (Abstracted here)
        yield return new WaitForSeconds(2f);

        // 2. Evening Report
        eveningReportPanel.SetActive(true);
        UpdateEveningReport();
        
        // Wait for player to close report or auto-close? 
        // For now, let's show Neighbor message after a delay
        yield return new WaitForSeconds(4f);
        
        eveningReportPanel.SetActive(false);
        ShowNeighborMessage();
        
        yield return new WaitForSeconds(3f);
        
        // End the day automatically for now
        DayCycleManager.Instance.CompleteDay();
    }

    private void UpdateEveningReport()
    {
        if (ResourceManager.Instance != null)
        {
            int hidden = ResourceManager.Instance.LeafletsHiddenToday;
            int sent = ResourceManager.Instance.LeafletsSentToday;
            // Assuming we can get risk change from RiskManager or track it
            float currentRisk = RiskManager.Instance != null ? RiskManager.Instance.CurrentRisk : 0;
            
            eveningSummaryText.text = $"Raport Dnia:\nSchowano: {hidden}\nPrzesłano: {sent}\nRyzyko: {currentRisk:F1}%";
        }
    }

    private void ShowNeighborMessage()
    {
        if (dialogueSystem != null)
        {
            string message = GetRandomNeighborMessage();
            dialogueSystem.ShowDialogue(message, neighborPortrait);
        }
    }

    private string GetRandomNeighborMessage()
    {
        string[] messages = {
            "Ktoś dopytywał o ruch w twoim magazynie.",
            "Mówili, że widziano Marię na rogu. Z Ochroną.",
            "Słyszałem dziwne stuki w nocy."
        };
        return messages[Random.Range(0, messages.Length)];
    }


    private void Update()
    {
        if (DayCycleManager.Instance != null && daysprites != null && daysprites.Count > 0)
        {
            float progress = Mathf.Clamp01(DayCycleManager.Instance.CurrentTime / DayCycleManager.Instance.DayDurationSeconds);
            
            // Map progress (0.0 to 1.0) to sprite index (0 to Count-1)
            // We want the last sprite to show when progress is near 1.0, so we use (Count) * progress and clamp
            int spriteIndex = Mathf.FloorToInt(progress * daysprites.Count);
            spriteIndex = Mathf.Clamp(spriteIndex, 0, daysprites.Count - 1);

            UpdateBackgroundSprite(spriteIndex);
        }
    }

    private void UpdateBackgroundSprite(int index)
    {
        if (daysprites == null || daysprites.Count == 0)
        {
            Debug.LogWarning("DayCycleUI: Daysprites list is empty!");
            return;
        }

        if (index < 0 || index >= daysprites.Count)
        {
            Debug.LogWarning($"DayCycleUI: Invalid sprite index {index}");
            return;
        }

        Sprite targetSprite = daysprites[index];
        
        if (uiBackgroundImage != null)
        {
            if (uiBackgroundImage.sprite != targetSprite)
            {
                uiBackgroundImage.sprite = targetSprite;
                Debug.Log($"DayCycleUI: Changed UI sprite to index {index}");
            }
        }
        else
        {
            Debug.LogWarning("DayCycleUI: Ui Background Image is not assigned!");
        }
            
        if (worldBackgroundRenderer != null)
        {
            if (worldBackgroundRenderer.sprite != targetSprite)
            {
                worldBackgroundRenderer.sprite = targetSprite;
                Debug.Log($"DayCycleUI: Changed World sprite to index {index}");
            }
        }
    }

    // Kept for compatibility if needed, but visuals are now driven by Update
    private void UpdateBackground(DayPhase phase)
    {
        // Optional: Could force a specific sprite for Night/Evening if logic dictates, 
        // but user wants time-based. We'll leave this empty or remove it to avoid conflict.
    }


    }

