using UnityEngine;

/// <summary>
/// A draggable contraband/resource object the player can pick up, drop, and stash inside a
/// <see cref="HidingSpot"/>. Picking it up raises risk via the <see cref="GameEvents"/> channel
/// (this object may be a runtime-spawned prefab, so it cannot hold a scene RiskManager reference).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class InteractableObject : MonoBehaviour
{
    [Tooltip("Risk added each time this object is picked up (noise/activity).")]
    [SerializeField]
    private float _pickupRisk = 10f;

    private AudioSource _audioSource;
    private SpriteRenderer _spriteRenderer;
    private Material _material;
    private Collider2D[] _colliders;
    private HidingSpot _hidingSpot;
    private bool _isHidden;

    public HidingSpot CurrentHidingSpot => _hidingSpot;

    public bool IsHidden
    {
        get => _isHidden;
        set
        {
            if (value)
            {
                if (DisableObject())
                    _isHidden = true;
                transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            }
            else if (EnableObject())
            {
                _isHidden = false;
            }
        }
    }

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _material = _spriteRenderer.material;
        _colliders = GetComponents<Collider2D>();
        Utils.DisableOutline(_material);
    }

    private void OnDestroy()
    {
        if (_hidingSpot)
            _hidingSpot.RemoveObject(this);
    }

    public void AssignHidingSpot(HidingSpot spot) => _hidingSpot = spot;

    public void ClearHidingSpot() => _hidingSpot = null;

    public bool DisableObject()
    {
        if (!_spriteRenderer || _colliders.Length <= 0)
            return false;

        Utils.DisableOutline(_material);
        _spriteRenderer.enabled = false;
        foreach (var col in _colliders)
            col.enabled = false;
        return true;
    }

    public bool EnableObject()
    {
        if (!_spriteRenderer || _colliders.Length <= 0)
            return false;

        _spriteRenderer.enabled = true;
        foreach (var col in _colliders)
            col.enabled = true;
        return true;
    }

    private void OnMouseDown() => TakeObject();

    private void OnMouseUp() => DropObject();

    private void OnMouseDrag()
    {
        if (Camera.main)
            transform.position = Utils.MouseToWorldPosition(Camera.main);
    }

    private void OnMouseEnter() => Utils.EnableOutline(_material);

    private void OnMouseExit() => Utils.DisableOutline(_material);

    private void TakeObject()
    {
        if (_isHidden && _hidingSpot)
            _hidingSpot.TakeOutObject(this);

        transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        PlaySound();
        GameEvents.RaiseRisk(_pickupRisk);
    }

    private void DropObject()
    {
        transform.localScale = Vector3.one;
        PlaySound();
    }

    private void PlaySound()
    {
        if (_audioSource)
            _audioSource.Play();
    }
}
