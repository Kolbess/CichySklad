using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// A clickable printing station: spends ink and paper to produce a leaflet, then enters a
/// cooldown visualised on a slider. Hovering/dragging shows the cost. Talks to an injected
/// <see cref="ResourceManager"/>.
/// </summary>
public class PrintLeaflet : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler
{
    [Header("Dependencies")]
    [Tooltip("Resource source that pays for and receives printed leaflets. Required.")]
    [SerializeField]
    private ResourceManager _resourceManager;

    [Header("UI Elements")]
    [Tooltip("Slider filled over the cooldown after a successful print. Required.")]
    [FormerlySerializedAs("cooldownSlider")]
    [SerializeField]
    private Slider _cooldownSlider;

    [Tooltip("Text showing the ink/paper cost on hover and drag. Required.")]
    [FormerlySerializedAs("costText")]
    [SerializeField]
    private TextMeshProUGUI _costText;

    [Header("Settings")]
    [Tooltip("Seconds of cooldown after a print before another is allowed. Must be > 0.")]
    [FormerlySerializedAs("cooldownDuration")]
    [SerializeField]
    private float _cooldownDuration = 5f;

    [Tooltip("Ink consumed per printed leaflet.")]
    [FormerlySerializedAs("costInk")]
    [SerializeField]
    private int _costInk = 1;

    [Tooltip("Paper consumed per printed leaflet.")]
    [FormerlySerializedAs("costPaper")]
    [SerializeField]
    private int _costPaper = 2;

    private float _cooldownTimer;
    private bool _onCooldown;

    private void OnValidate()
    {
        if (_cooldownDuration <= 0f)
            _cooldownDuration = 0.01f;
        if (_costInk < 0)
            _costInk = 0;
        if (_costPaper < 0)
            _costPaper = 0;
    }

    private void Awake()
    {
        Assert.IsNotNull(
            _resourceManager,
            $"[{nameof(PrintLeaflet)}] ResourceManager unassigned on {name}!"
        );
        Assert.IsNotNull(
            _cooldownSlider,
            $"[{nameof(PrintLeaflet)}] Cooldown slider unassigned on {name}!"
        );
        Assert.IsNotNull(_costText, $"[{nameof(PrintLeaflet)}] Cost text unassigned on {name}!");
    }

    private void Start()
    {
        _cooldownSlider.maxValue = _cooldownDuration;
        _cooldownSlider.value = 0f;
        _costText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!_onCooldown)
            return;

        _cooldownTimer -= Time.deltaTime;
        _cooldownSlider.value = _cooldownDuration - _cooldownTimer;

        if (_cooldownTimer <= 0f)
        {
            _cooldownTimer = 0f;
            _onCooldown = false;
        }
    }

    private void OnMouseDown()
    {
        if (_onCooldown)
            return;

        if (_resourceManager.TrySpend(costPaper: _costPaper, costInk: _costInk))
        {
            _resourceManager.AddLeaflets(1);
            StartCooldown();
        }
    }

    private void StartCooldown()
    {
        _onCooldown = true;
        _cooldownTimer = _cooldownDuration;
        _cooldownSlider.value = 0f;
    }

    public void OnPointerEnter(PointerEventData eventData) => ShowCost();

    public void OnPointerExit(PointerEventData eventData) => _costText.gameObject.SetActive(false);

    public void OnDrag(PointerEventData eventData) => ShowCost();

    private void ShowCost()
    {
        _costText.text = $"Tusz: {_costInk}, Papier: {_costPaper}";
        _costText.gameObject.SetActive(true);
    }
}
