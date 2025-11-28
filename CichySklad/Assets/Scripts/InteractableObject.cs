using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    private static readonly int OutlineEnabled = Shader.PropertyToID("_OutlineEnabled");
    private AudioSource _audioSource;
    private bool _isInteracted;
    [SerializeField] private float cooldown;
    private SpriteRenderer _spriteRenderer;
    private Material _material;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _material = _spriteRenderer.material;
    }

    private void PlaySound()
    {
        _audioSource.Play();
    }

    private void EnableOutline()
    {
        _material.SetInt(OutlineEnabled, 1);
    }
    
    private void DisableOutline()
    {
        _material.SetInt(OutlineEnabled, 0);
    }
    
    private void TakeAnimation()
    {
        transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
    }
    
    private void DropAnimation()
    {
        transform.localScale = new Vector3(1f, 1f, 1f);
    }

    private void OnMouseDown()
    {
        Debug.Log("OnMouseDown");
        TakeAnimation();
        PlaySound();
        
        // Increase risk when interacting (simulating noise/activity)
        if (RiskManager.Instance != null)
        {
            RiskManager.Instance.AddRisk(10f);
        }
    }

    private void OnMouseUp()
    {
        DropAnimation();
        PlaySound();
    }

    private void OnMouseDrag()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10;
        if (Camera.main != null) transform.position = Camera.main.ScreenToWorldPoint(mousePos);
        Debug.Log($"x: {Input.mousePosition.x}, y: {Input.mousePosition.y}, ox: {transform.position.x}, oy: {transform.position.y}");
    }

    private void OnMouseEnter()
    {
        EnableOutline();
    }
    
    private void OnMouseExit()
    {
        DisableOutline();
    }
}
