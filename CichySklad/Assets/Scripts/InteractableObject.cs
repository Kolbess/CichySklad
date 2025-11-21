using System;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    private Collider2D _collider;
    private bool _isInteracted;
    
    private void Start()
    {
        _collider = GetComponent<Collider2D>();
    }

    public void DisableCollider()
    {
        _collider.enabled = false;
    }
    
    public void EnableCollider()
    {
        _collider.enabled = true;
    }

    public void Interact()
    {
        EnableCollider();
        _isInteracted = true;
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
    }

    private void OnMouseUp()
    {
        DropAnimation();
    }

    private void OnMouseDrag()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10;
        if (Camera.main != null) transform.position = Camera.main.ScreenToWorldPoint(mousePos);
        Debug.Log($"x: {Input.mousePosition.x}, y: {Input.mousePosition.y}, ox: {transform.position.x}, oy: {transform.position.y}");
    }
}
