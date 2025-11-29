using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DialogueSystem : MonoBehaviour
{
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image dialogueImage;
    [SerializeField] private Sprite defaultPortrait;
    [SerializeField] private GameObject choiceButtonPrefab; // prefab with Button + Text
    [SerializeField] private Transform choicesContainer;   // parent for buttons

    private Action _onChoiceMade;

    private void Start()
    {
        dialogueBox.SetActive(false);
    }

    // Show a simple dialogue with optional portrait
    public void ShowDialogue(string text, Sprite portrait = null)
    {
        dialogueBox.SetActive(true);
        dialogueText.text = text;

        dialogueImage.sprite = portrait ? portrait : defaultPortrait;

        dialogueImage.gameObject.SetActive(true);

        ClearChoices();
        CancelInvoke(nameof(HideDialogue));
        Invoke(nameof(HideDialogue), 4f); // auto-hide if no choice
    }

    // Show dialogue with choices
    public void ShowDialogueWithChoices(string text, Choice[] choices, Sprite portrait = null)
    {
        dialogueBox.SetActive(true);
        dialogueText.text = text;

        dialogueImage.sprite = portrait ? portrait : defaultPortrait;
        dialogueImage.gameObject.SetActive(true);

        ClearChoices();

        foreach (var choice in choices)
        {
            GameObject btnObj = Instantiate(choiceButtonPrefab, choicesContainer);
            Button btn = btnObj.GetComponent<Button>();
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = choice.text;
            btn.onClick.AddListener(() =>
            {
                choice.onSelected.Invoke();
                HideDialogue();
            });
        }
    }

    private void ClearChoices()
    {
        foreach (Transform child in choicesContainer)
            Destroy(child.gameObject);
    }

    private void HideDialogue()
    {
        dialogueBox.SetActive(false);
        ClearChoices();
    }

    [Serializable]
    public struct Choice
    {
        public string text;
        public Action onSelected;

        public Choice(string text, Action onSelected)
        {
            this.text = text;
            this.onSelected = onSelected;
        }
    }
}
