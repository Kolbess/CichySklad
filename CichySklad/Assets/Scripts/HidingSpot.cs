using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

/// <summary>
/// A container the player drags contraband into. Shows remaining capacity on hover, opens a
/// content panel on click, and lays hidden objects out onto content slots. Owns the list of
/// stashed <see cref="InteractableObject"/>s; objects notify it when destroyed.
/// </summary>
[RequireComponent(typeof(SpriteRenderer), typeof(AudioSource))]
public class HidingSpot : MonoBehaviour
{
    [Header("Capacity")]
    [Tooltip("Maximum number of objects this spot can hold. Must be >= 1.")]
    [FormerlySerializedAs("maxCapacity")]
    [SerializeField]
    private int _maxCapacity = 3;

    [Tooltip("Root object of the 'space left' readout, shown on hover. Required.")]
    [FormerlySerializedAs("capacityUI")]
    [SerializeField]
    private GameObject _capacityUI;

    [Tooltip("Text that displays the remaining capacity. Required.")]
    [FormerlySerializedAs("capacityText")]
    [SerializeField]
    private TextMeshProUGUI _capacityText;

    [Header("Content Panel")]
    [Tooltip("Panel (with tagged 'ContentSlot' children) that lays out stashed objects. Required.")]
    [FormerlySerializedAs("contentUI")]
    [SerializeField]
    private GameObject _contentUI;

    [Header("Sprites")]
    [Tooltip("Sprite shown while the spot is open.")]
    [FormerlySerializedAs("openSprite")]
    [SerializeField]
    private Sprite _openSprite;

    [Tooltip("Sprite shown while the spot is closed.")]
    [FormerlySerializedAs("closedSprite")]
    [SerializeField]
    private Sprite _closedSprite;

    private readonly List<InteractableObject> _objects = new List<InteractableObject>();
    private int _capacity;
    private AudioSource _audioSource;
    private SpriteRenderer _spriteRenderer;
    private Material _material;

    public IReadOnlyList<InteractableObject> Objects => _objects;
    public int MaxCapacity => _maxCapacity;

    private void OnValidate()
    {
        if (_maxCapacity < 1)
            _maxCapacity = 1;
    }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _audioSource = GetComponent<AudioSource>();
        _material = _spriteRenderer.material;

        Assert.IsNotNull(_capacityUI, $"[{nameof(HidingSpot)}] Capacity UI unassigned on {name}!");
        Assert.IsNotNull(
            _capacityText,
            $"[{nameof(HidingSpot)}] Capacity text unassigned on {name}!"
        );
        Assert.IsNotNull(_contentUI, $"[{nameof(HidingSpot)}] Content UI unassigned on {name}!");
    }

    private void Start()
    {
        Utils.DisableOutline(_material);
        _capacity = _objects.Count;
        _contentUI.SetActive(false);
        _capacityUI.SetActive(false);
    }

    /// <summary>Removes an object as the player pulls it back out, re-laying-out the remainder.</summary>
    public void TakeOutObject(InteractableObject interactableObject)
    {
        _objects.Remove(interactableObject);
        interactableObject.IsHidden = false;
        interactableObject.ClearHidingSpot();
        UpdateCapacity();

        if (_capacity <= 0)
            HideContent();
        else
            GenerateContent();
    }

    /// <summary>Called by an object when it is destroyed so the spot drops its stale reference.</summary>
    public void RemoveObject(InteractableObject interactableObject)
    {
        if (_objects.Remove(interactableObject))
            UpdateCapacity();
    }

    /// <summary>Permanently enlarges the stash (a progression unlock — "better hiding spot"), and
    /// refreshes the capacity readout. Non-positive amounts are ignored.</summary>
    public void IncreaseCapacity(int extra)
    {
        if (extra <= 0)
            return;

        _maxCapacity += extra;
        UpdateCapacity();
    }

    private void OnMouseEnter()
    {
        Utils.EnableOutline(_material);
        ShowCapacity();
    }

    private void OnMouseExit()
    {
        Utils.DisableOutline(_material);
        HideCapacity();
    }

    private void OnMouseDown()
    {
        if (_contentUI.activeSelf)
            HideContent();
        else
            ShowContent();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("InteractableObject"))
            return;

        ShowCapacity();
        Utils.EnableOutline(_material);
        ChangeSprite(true);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("InteractableObject"))
            return;

        if (!Input.GetMouseButton(0))
            HideObject(other.gameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("InteractableObject"))
            return;

        HideCapacity();
        Utils.DisableOutline(_material);
        ChangeSprite(false);
    }

    private void ShowCapacity()
    {
        UpdateCapacity();
        _capacityUI.SetActive(true);
    }

    private void HideCapacity() => _capacityUI.SetActive(false);

    private void UpdateCapacity()
    {
        _capacity = _objects.Count;
        _capacityText.text = $"Wolne miejsce: {_maxCapacity - _capacity}";
    }

    private void ShowContent()
    {
        _contentUI.SetActive(true);
        GenerateContent();
        PlaySound();
        ChangeSprite(true);
    }

    private void HideContent()
    {
        _contentUI.SetActive(false);
        PlaySound();
        foreach (InteractableObject stashed in _objects)
            stashed.DisableObject();
        ChangeSprite(false);
    }

    private void GenerateContent()
    {
        GameObject[] contentSlots = _contentUI
            .GetComponentsInChildren<Transform>(true)
            .Select(t => t.gameObject)
            .Where(obj => obj.CompareTag("ContentSlot"))
            .ToArray();

        for (int i = 0; i < _capacity && i < contentSlots.Length; i++)
        {
            InteractableObject objectToSpawn = _objects[i];
            objectToSpawn.EnableObject();
            objectToSpawn.transform.position = contentSlots[i].transform.position;
            objectToSpawn.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        }
    }

    private void HideObject(GameObject obj)
    {
        if (!obj.TryGetComponent(out InteractableObject interactable))
            return;

        if (_objects.Count >= _maxCapacity || _objects.Contains(interactable))
            return;

        _objects.Add(interactable);
        interactable.IsHidden = true;
        interactable.AssignHidingSpot(this);
        UpdateCapacity();

        if (_contentUI.activeSelf)
            GenerateContent();
        PlaySound();
    }

    private void ChangeSprite(bool open) =>
        _spriteRenderer.sprite = open ? _openSprite : _closedSprite;

    private void PlaySound(AudioClip clip = null)
    {
        if (clip)
            _audioSource.PlayOneShot(clip);
        _audioSource.Play();
    }
}
