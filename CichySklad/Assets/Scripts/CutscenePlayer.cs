using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Plays a <see cref="Cutscene"/> line by line through the shared <see cref="DialogueSystem"/>: each
/// line shows its speaker's portrait and text, and the player advances by clicking (or a bound "Next"
/// button / gamepad submit via <see cref="Advance"/>), or automatically after a line's timer. The
/// final line may instead present a row of choices whose actions are supplied in code. While a
/// cutscene plays it locks the dialogue box, so day-loop and event dialogue cannot overwrite it.
///
/// No singletons: the dialogue box is injected. Consumers (e.g. a B7 story beat) call
/// <see cref="Play"/> / <see cref="PlayWithEndChoices"/> and listen to <see cref="OnCutsceneFinished"/>.
/// </summary>
public class CutscenePlayer : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Dialogue box used to present each cutscene line. Required.")]
    [SerializeField]
    private DialogueSystem _dialogueSystem;

    [Header("Demo")]
    [Tooltip("Cutscene auto-played once on Start, for a quick in-scene demo. Optional.")]
    [SerializeField]
    private Cutscene _demoCutscene;

    private IReadOnlyList<CutsceneLine> _lines;
    private int _index;
    private bool _isPlaying;
    private float _lineTimer;
    private DialogueSystem.Choice[] _endChoices;
    private Action _onComplete;

    /// <summary>Whether a cutscene is currently being played, line by line.</summary>
    public bool IsPlaying => _isPlaying;

    /// <summary>Raised once the final line is done (or its end-choice is picked).</summary>
    public event Action OnCutsceneFinished;

    private void Awake()
    {
        Assert.IsNotNull(
            _dialogueSystem,
            $"[{nameof(CutscenePlayer)}] DialogueSystem unassigned on {name}!"
        );
    }

    private void Start()
    {
        if (_demoCutscene != null)
            Play(_demoCutscene);
    }

    private void Update()
    {
        if (!_isPlaying)
            return;

        CutsceneLine line = _lines[_index];
        if (line.AutoAdvanceSeconds > 0f)
        {
            _lineTimer += Time.deltaTime;
            if (_lineTimer >= line.AutoAdvanceSeconds)
            {
                Advance();
                return;
            }
        }

        if (Input.GetMouseButtonDown(0))
            Advance();
    }

    // =====================================================================
    // Public API
    // =====================================================================

    /// <summary>Plays a cutscene that simply ends (box hidden, <paramref name="onComplete"/> fired).</summary>
    public void Play(Cutscene cutscene, Action onComplete = null) =>
        StartCutscene(cutscene, null, onComplete);

    /// <summary>Plays a cutscene whose last line presents <paramref name="endChoices"/>.</summary>
    public void PlayWithEndChoices(Cutscene cutscene, DialogueSystem.Choice[] endChoices) =>
        StartCutscene(cutscene, endChoices, null);

    /// <summary>Moves to the next line, or ends the cutscene past the last. Bind to a "Next" button
    /// or a gamepad submit for non-mouse control.</summary>
    public void Advance()
    {
        if (!_isPlaying)
            return;

        _index++;
        _lineTimer = 0f;

        if (_index >= _lines.Count)
            Finish();
        else
            ShowCurrent();
    }

    // =====================================================================
    // Private helpers
    // =====================================================================

    private void StartCutscene(
        Cutscene cutscene,
        DialogueSystem.Choice[] endChoices,
        Action onComplete
    )
    {
        if (cutscene == null || cutscene.Lines.Count == 0)
            return;

        _lines = cutscene.Lines;
        _index = 0;
        _lineTimer = 0f;
        _endChoices = endChoices;
        _onComplete = onComplete;
        _isPlaying = true;
        _dialogueSystem.SetLocked(true); // events cannot clobber the box while the cutscene runs
        ShowCurrent();
    }

    private void ShowCurrent()
    {
        CutsceneLine line = _lines[_index];
        Sprite portrait = _dialogueSystem.GetPortraitSprite(line.SpeakerKey);

        bool isLast = _index == _lines.Count - 1;
        if (isLast && _endChoices != null && _endChoices.Length > 0)
        {
            // The final line presents the choices; picking one is the cutscene's outcome.
            _isPlaying = false;
            _dialogueSystem.SetLocked(false); // release so the choice UI is interactive
            _dialogueSystem.ShowDialogueWithChoices(
                line.Text,
                WrapEndChoices(_endChoices),
                portrait
            );
        }
        else
        {
            _dialogueSystem.ShowLine(line.Text, portrait);
        }
    }

    private void Finish()
    {
        _isPlaying = false;
        _dialogueSystem.SetLocked(false);
        _dialogueSystem.Hide();

        _onComplete?.Invoke();
        OnCutsceneFinished?.Invoke();
    }

    private DialogueSystem.Choice[] WrapEndChoices(DialogueSystem.Choice[] choices)
    {
        var wrapped = new DialogueSystem.Choice[choices.Length];
        for (int i = 0; i < choices.Length; i++)
        {
            DialogueSystem.Choice original = choices[i];
            wrapped[i] = new DialogueSystem.Choice(
                original.Text,
                () =>
                {
                    original.OnSelected?.Invoke();
                    OnCutsceneFinished?.Invoke();
                }
            );
        }
        return wrapped;
    }
}
