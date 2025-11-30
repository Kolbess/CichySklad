using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    private AudioSource _audioSource;
    private bool _isInteracted;
    private SpriteRenderer _spriteRenderer;
    private Material _material;
    private Collider2D[] _colliders;
    private Rigidbody2D _rb;
    public HidingSpot hidingSpot;

    public bool isHidden
    {
        get => _isHidden;

        set
        {
            if (value)
            {
                if (DisableObject()) _isHidden = true;
                transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                Debug.Log($"{name}, isHidden: {isHidden}, Hidden Setter");
            }
            else
            {
                if (EnableObject()) _isHidden = false;
            }
        }
    }
    private bool _isHidden;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _material = _spriteRenderer.material;
        _colliders = GetComponents<Collider2D>();
        Utils.DisableOutline(_material);
    }

    public SpriteRenderer GetSpriteRenderer()
    {
        return _spriteRenderer;
    }

    public bool DisableObject()
    {
        if (!_spriteRenderer || _colliders.Length <= 0) return false;
        Utils.DisableOutline(_material);
        _spriteRenderer.enabled = false;
        foreach (var col in _colliders) col.enabled = false;
        return true;
    }

    public bool EnableObject()
    {
        if (!_spriteRenderer || _colliders.Length <= 0) return false;
        _spriteRenderer.enabled = true;
        foreach (var col in _colliders) col.enabled = true;
        return true;
    }

    private void PlaySound()
    {
        _audioSource.Play();
    }
    
    private void TakeAnimation()
    {
        transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
    }
    
    private void DropAnimation()
    {
        transform.localScale = new Vector3(1f, 1f, 1f);
    }

    private void TakeObject()
    {
        // Debug.Log($"{name}, isHidden: {isHidden}");
        if (isHidden)
        {
            hidingSpot.TakeOutObject(this);
        }
        TakeAnimation();
        PlaySound();
        
        // Increase risk when interacting (simulating noise/activity)
        if (RiskManager.Instance != null)
        {
            RiskManager.Instance.AddRisk(7.5f);
        }
    }
    
    private void DropObject()
    {
        DropAnimation();
        PlaySound();
    }

    private void OnMouseDown()
    {
        TakeObject();
    }

    private void OnMouseUp()
    {
        DropObject();
    }

    private void OnMouseDrag()
    {
        if (Camera.main) transform.position = Utils.MouseToWorldPosition(Camera.main);
    }

    private void OnMouseEnter()
    {
        Utils.EnableOutline(_material);
    }
    
    private void OnMouseExit()
    {
        Utils.DisableOutline(_material);
    }
}
