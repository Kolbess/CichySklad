using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PrintLeaflet : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler
{
    [Header("UI Elements")]
    public Slider cooldownSlider;
    public Text costText;

    [Header("Settings")]
    public float cooldownDuration = 5f;
    public int costInk = 1;
    public int costPaper = 2;

    private float cooldownTimer = 0f;
    private bool onCooldown = false;

    private void Start()
    {
        if (cooldownSlider != null)
        {
            cooldownSlider.maxValue = cooldownDuration;
            cooldownSlider.value = 0f;
        }

        if (costText != null)
            costText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (onCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            cooldownSlider.value = cooldownDuration - cooldownTimer;

            if (cooldownTimer <= 0f)
            {
                cooldownTimer = 0f;
                onCooldown = false;
            }
        }
    }

    private void OnMouseDown()
    {
        if (onCooldown) return; // still cooling down

        if (ResourceManager.Instance.TrySpend(costInk, costPaper))
        {
            ResourceManager.Instance.leaflets += 1;
            StartCooldown();
        }
    }

    private void StartCooldown()
    {
        onCooldown = true;
        cooldownTimer = cooldownDuration;
        if (cooldownSlider != null)
            cooldownSlider.value = 0f;
    }

    // Hover / Drag UI events
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (costText != null)
        {
            costText.text = $"Ink: {costInk}, Paper: {costPaper}";
            costText.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (costText != null)
            costText.gameObject.SetActive(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Show cost while dragging as well
        if (costText != null)
        {
            costText.text = $"Ink: {costInk}, Paper: {costPaper}";
            costText.gameObject.SetActive(true);
        }
    }
}
