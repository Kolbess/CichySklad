using System.Collections.Generic;
using UnityEngine;

public class Package : MonoBehaviour
{
    [SerializeField] private Transform dropPoint;
    private List<GameObject> _items = new List<GameObject>();
    public List<GameObject> GetItems() => _items;
    private SpriteRenderer _spriteRenderer;
    private Material _material;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _material = _spriteRenderer.material;
    }
    
    public void AddItem(GameObject g)
    {
        g.SetActive(false);          // ukrywamy item w paczce
        _items.Add(g);
    }

    private void OnMouseEnter()
    {
        Utils.EnableOutline(_material);
    }
    
    private void OnMouseExit()
    {
        Utils.DisableOutline(_material);
    }

    private void OnMouseDown()
    {
        Debug.Log(_items.ToString()+" "+_items.Count);
        if (_items.Count == 0)
        {
            Destroy(this.gameObject);
            return;
        }

        GameObject item = _items[0];
        _items.RemoveAt(0);

        item.transform.position = dropPoint.position;
        item.SetActive(true);

        // jeśli item ma InteractableObject
        var io = item.GetComponent<InteractableObject>();
        if (io != null)
        {
            io.EnableObject();   // włącza sprite + collider
        }
    }
}