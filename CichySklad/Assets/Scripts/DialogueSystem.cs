using System;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Presents narrative text with an optional portrait, and optionally a row of choice buttons.
/// Choiceless dialogue auto-hides after a delay; choices hide it on selection. Portrait lookup is
/// keyed by a short character name.
/// </summary>
public class DialogueSystem : MonoBehaviour
{
    [Header("Dialogue Box")]
    [Tooltip("Root object toggled to show/hide the dialogue. Required.")]
    [FormerlySerializedAs("dialogueBox")]
    [SerializeField]
    private GameObject _dialogueBox;

    [Tooltip("Text element that displays the dialogue body. Required.")]
    [FormerlySerializedAs("dialogueText")]
    [SerializeField]
    private TextMeshProUGUI _dialogueText;

    [Tooltip("Image element that displays the speaker portrait. Required.")]
    [FormerlySerializedAs("dialogueImage")]
    [SerializeField]
    private Image _dialogueImage;

    [Header("Choices")]
    [Tooltip("Prefab with a Button + child text used for each choice. Required.")]
    [FormerlySerializedAs("choiceButtonPrefab")]
    [SerializeField]
    private GameObject _choiceButtonPrefab;

    [Tooltip("Parent transform the choice buttons are spawned under. Required.")]
    [FormerlySerializedAs("choicesContainer")]
    [SerializeField]
    private Transform _choicesContainer;

    [Header("Timing")]
    [Tooltip("Seconds a choiceless dialogue stays on screen before auto-hiding.")]
    [SerializeField]
    private float _autoHideSeconds = 4f;

    [Header("Portraits")]
    [Tooltip("Portrait used when a character name is unknown or none is supplied.")]
    [FormerlySerializedAs("defaultPortrait")]
    [SerializeField]
    private Sprite _defaultPortrait;

    [Tooltip("Portrait for the 'ochrana' speaker.")]
    [FormerlySerializedAs("ochranaSprite")]
    [SerializeField]
    private Sprite _ochranaSprite;

    [Tooltip("Portrait for the 'maria' speaker.")]
    [FormerlySerializedAs("mariaSprite")]
    [SerializeField]
    private Sprite _mariaSprite;

    [Tooltip("Portrait for the 'kowal' speaker.")]
    [FormerlySerializedAs("kowalSprite")]
    [SerializeField]
    private Sprite _kowalSprite;

    [Tooltip("Portrait for the 'neighbour' speaker.")]
    [FormerlySerializedAs("neighbourSprite")]
    [SerializeField]
    private Sprite _neighbourSprite;

    // While locked, a CutscenePlayer owns the box; external ShowDialogue* calls are ignored so events
    // cannot overwrite or hide a cutscene mid-playback.
    private bool _locked;

    /// <summary>Whether the box is currently reserved by a cutscene and ignoring external calls.</summary>
    public bool IsLocked => _locked;

    private void Awake()
    {
        Assert.IsNotNull(
            _dialogueBox,
            $"[{nameof(DialogueSystem)}] Dialogue box unassigned on {name}!"
        );
        Assert.IsNotNull(
            _dialogueText,
            $"[{nameof(DialogueSystem)}] Dialogue text unassigned on {name}!"
        );
        Assert.IsNotNull(
            _dialogueImage,
            $"[{nameof(DialogueSystem)}] Dialogue image unassigned on {name}!"
        );
        Assert.IsNotNull(
            _choiceButtonPrefab,
            $"[{nameof(DialogueSystem)}] Choice button prefab unassigned on {name}!"
        );
        Assert.IsNotNull(
            _choicesContainer,
            $"[{nameof(DialogueSystem)}] Choices container unassigned on {name}!"
        );
    }

    private void Start()
    {
        _dialogueBox.SetActive(false);
    }

    /// <summary>Shows a plain dialogue line that auto-hides after <see cref="_autoHideSeconds"/>.
    /// Ignored while a cutscene holds the box (see <see cref="IsLocked"/>).</summary>
    public void ShowDialogue(string text, Sprite portrait = null)
    {
        if (_locked)
            return;

        OpenWith(text, portrait);
        ClearChoices();
        CancelInvoke(nameof(HideDialogue));
        Invoke(nameof(HideDialogue), _autoHideSeconds);
    }

    /// <summary>Shows a dialogue with a row of choice buttons; hides when a choice is picked.
    /// Ignored while a cutscene holds the box (see <see cref="IsLocked"/>).</summary>
    public void ShowDialogueWithChoices(string text, Choice[] choices, Sprite portrait = null)
    {
        if (_locked)
            return;

        OpenWith(text, portrait);
        ClearChoices();

        foreach (Choice choice in choices)
        {
            GameObject buttonObject = Instantiate(_choiceButtonPrefab, _choicesContainer);
            Button button = buttonObject.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObject.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = choice.Text;

            Action onSelected = choice.OnSelected;
            button.onClick.AddListener(() =>
            {
                onSelected?.Invoke();
                HideDialogue();
            });
        }
    }

    /// <summary>
    /// Presents a single cutscene line: opens the box with a portrait, clears any choices, and does
    /// NOT auto-hide — a <see cref="CutscenePlayer"/> drives advancement. Bypasses the lock so the
    /// owning cutscene can keep updating the box.
    /// </summary>
    public void ShowLine(string text, Sprite portrait = null)
    {
        OpenWith(text, portrait);
        ClearChoices();
        CancelInvoke(nameof(HideDialogue));
    }

    /// <summary>Hides the dialogue box immediately (used by a cutscene when it ends without choices).</summary>
    public void Hide() => HideDialogue();

    /// <summary>Reserves (or releases) the box for a cutscene so external calls are ignored.</summary>
    public void SetLocked(bool locked) => _locked = locked;

    public Sprite GetPortraitSprite(string characterName)
    {
        switch (characterName)
        {
            case "ochrana":
                return _ochranaSprite;
            case "maria":
                return _mariaSprite;
            case "kowal":
                return _kowalSprite;
            case "neighbour":
                return _neighbourSprite;
            default:
                return _defaultPortrait;
        }
    }

    private void OpenWith(string text, Sprite portrait)
    {
        _dialogueBox.SetActive(true);
        _dialogueText.text = text;
        _dialogueImage.sprite = portrait ? portrait : _defaultPortrait;
        _dialogueImage.gameObject.SetActive(true);
    }

    private void ClearChoices()
    {
        foreach (Transform child in _choicesContainer)
            Destroy(child.gameObject);
    }

    private void HideDialogue()
    {
        _dialogueBox.SetActive(false);
        ClearChoices();
    }

    /// <summary>A single dialogue choice: its button label and the action taken when selected.</summary>
    public readonly struct Choice
    {
        public string Text { get; }
        public Action OnSelected { get; }

        public Choice(string text, Action onSelected)
        {
            Text = text;
            OnSelected = onSelected;
        }
    }
}
