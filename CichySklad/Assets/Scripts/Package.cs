using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

/// <summary>
/// A bundle of hidden resource items. Hovering outlines it; clicking pops one item out at the drop
/// point, and clicking an already-empty package removes it from the scene.
/// <see cref="ResourceManager"/> fills packages when resources are gained and drains them when
/// resources are lost.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Package : MonoBehaviour
{
    [Tooltip("Where a popped item is placed when the package is clicked. Required.")]
    [FormerlySerializedAs("dropPoint")]
    [SerializeField]
    private Transform _dropPoint;

    private readonly List<GameObject> _items = new List<GameObject>();
    private SpriteRenderer _spriteRenderer;
    private Material _material;

    /// <summary>The live item list. Callers may drain it as items are consumed/destroyed.</summary>
    public List<GameObject> Items => _items;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _material = _spriteRenderer.material;
        Assert.IsNotNull(_dropPoint, $"[{nameof(Package)}] Drop point unassigned on {name}!");
    }

    public void AddItem(GameObject item)
    {
        item.SetActive(false); // ukrywamy item w paczce
        _items.Add(item);
    }

    private void OnMouseEnter() => Utils.EnableOutline(_material);

    private void OnMouseExit() => Utils.DisableOutline(_material);

    private void OnMouseDown()
    {
        // A spend may have destroyed a bundled item straight out of the list; drop the stale nulls.
        _items.RemoveAll(item => item == null);

        // An empty package has nothing left to give — remove it from the scene.
        if (_items.Count == 0)
        {
            Destroy(gameObject);
            return;
        }

        GameObject item = _items[0];
        _items.RemoveAt(0);

        item.transform.position = _dropPoint.position;
        item.SetActive(true);

        if (item.TryGetComponent(out InteractableObject interactable))
            interactable.EnableObject();
    }
}
