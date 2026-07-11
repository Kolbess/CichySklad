using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Turns rising trust into concrete progression: as the player's trust crosses each
/// <see cref="Unlock"/>'s threshold, the unlock fires once — announced through the
/// <see cref="DialogueSystem"/>, applied to the game (coins, a bigger stash), and exposed via
/// <see cref="IsUnlocked"/> and <see cref="OnUnlocked"/> so other systems can gate content on it.
/// Threshold maths live in the pure, tested <see cref="ProgressionLadder"/>. No singletons — the
/// resource/dialogue/stash it touches are injected references.
/// </summary>
public class ProgressionSystem : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip(
        "Trust source watched for threshold crossings, and coin sink for MoneyReward. Required."
    )]
    [SerializeField]
    private ResourceManager _resourceManager;

    [Tooltip("Presents the 'unlocked!' message. Optional (falls back to a log).")]
    [SerializeField]
    private DialogueSystem _dialogueSystem;

    [Tooltip("Hiding spot enlarged by an ExtraHidingCapacity unlock. Optional.")]
    [SerializeField]
    private HidingSpot _hidingSpot;

    [Header("Unlocks")]
    [Tooltip(
        "Trust-gated unlocks. Order does not matter; each fires when its threshold is crossed."
    )]
    [SerializeField]
    private Unlock[] _unlocks;

    private readonly HashSet<string> _unlockedKeys = new HashSet<string>();
    private int _lastTrust;

    /// <summary>Raised once for each unlock the moment it is earned.</summary>
    public event Action<Unlock> OnUnlocked;

    /// <summary>Number of unlocks earned so far.</summary>
    public int UnlockedCount => _unlockedKeys.Count;

    private void Awake()
    {
        Assert.IsNotNull(
            _resourceManager,
            $"[{nameof(ProgressionSystem)}] ResourceManager unassigned on {name}!"
        );
        Assert.IsNotNull(
            _unlocks,
            $"[{nameof(ProgressionSystem)}] Unlocks array unassigned on {name}!"
        );
    }

    private void OnEnable()
    {
        if (_resourceManager != null)
            _resourceManager.OnTrustChanged += HandleTrustChanged;
    }

    private void OnDisable()
    {
        if (_resourceManager != null)
            _resourceManager.OnTrustChanged -= HandleTrustChanged;
    }

    private void Start()
    {
        // Catch any threshold already met by starting trust, then track changes from there.
        int trust = _resourceManager.Trust;
        EvaluateUnlocks(0, trust);
        _lastTrust = trust;
    }

    // =====================================================================
    // Public API
    // =====================================================================

    /// <summary>Whether the unlock with this key has been earned. Other systems gate content on this.</summary>
    public bool IsUnlocked(string key) => _unlockedKeys.Contains(key);

    // =====================================================================
    // Private helpers
    // =====================================================================

    private void HandleTrustChanged(int currentTrust)
    {
        EvaluateUnlocks(_lastTrust, currentTrust);
        _lastTrust = currentTrust;
    }

    private void EvaluateUnlocks(int previousTrust, int currentTrust)
    {
        foreach (Unlock unlock in _unlocks)
        {
            if (unlock == null || _unlockedKeys.Contains(unlock.Key))
                continue;

            if (
                ProgressionLadder.WasJustReached(previousTrust, currentTrust, unlock.TrustThreshold)
            )
                Grant(unlock);
        }
    }

    private void Grant(Unlock unlock)
    {
        _unlockedKeys.Add(unlock.Key);
        ApplyEffect(unlock);
        Announce(unlock);
        OnUnlocked?.Invoke(unlock);
    }

    private void ApplyEffect(Unlock unlock)
    {
        switch (unlock.Effect)
        {
            case UnlockEffect.MoneyReward:
                _resourceManager.AddMoney(unlock.Magnitude);
                break;
            case UnlockEffect.ExtraHidingCapacity:
                if (_hidingSpot != null)
                    _hidingSpot.IncreaseCapacity(unlock.Magnitude);
                break;
        }
    }

    private void Announce(Unlock unlock)
    {
        string message = $"Odblokowano: {unlock.Title}\n{unlock.Description}";
        if (_dialogueSystem != null)
            _dialogueSystem.ShowDialogue(message, _dialogueSystem.GetPortraitSprite("maria"));
        else
            Debug.Log($"[{nameof(ProgressionSystem)}] {message}");
    }
}
