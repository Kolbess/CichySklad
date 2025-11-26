using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class HidingSpot : MonoBehaviour
{
    private int _capacity;
    [SerializeField] private int maxCapacity;
    [SerializeField] private GameObject capacityUI;
    [SerializeField] private TextMeshProUGUI capacityText;
    
    [SerializeField] private float risk;
    [SerializeField] private Image riskImage;
    private int _timesUsed;

    [SerializeField] private GameObject contentUI;
    
    private AudioSource _audioSource;
    private Material _material;
    private List<InteractableObject> _objects = new List<InteractableObject>();

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _material = GetComponent<SpriteRenderer>().material;
        Utils.DisableOutline(_material);
        _capacity = _objects.Count;
        contentUI.SetActive(false);
        capacityUI.SetActive(false);
    }

    private void ShowCapacity()
    {
        UpdateCapacity();
        capacityUI.SetActive(true);
    }
    
    private void UpdateCapacity()
    {
        _capacity = _objects.Count;
        capacityText.text = $"Space Left: {maxCapacity - _capacity}";
    }
    
    private void HideCapacity()
    {
        capacityUI.SetActive(false);
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
        ChangeContentState();
    }

    private void ChangeContentState()
    {
        if (contentUI.activeSelf) HideContent();
        else ShowContent();
    }
    
    private void ShowContent()
    {
        contentUI.SetActive(true);
        GenerateContent();
        PlaySound();
    }

    private void HideContent()
    {
        contentUI.SetActive(false);
        PlaySound();
        for (int i = 0; i < _capacity; i++)
        {
            var objectToSpawn = _objects[i];
            objectToSpawn.DisableObject();
        }
    }

    private void GenerateContent()
    {
        var allChildObjects = contentUI.GetComponentsInChildren<Transform>(true).Select(t => t.gameObject).ToArray();
        var contentSlots = allChildObjects.Where(obj => obj.CompareTag("ContentSlot")).ToArray();

        for (int i = 0; i < _capacity; i++)
        {
            var objectToSpawn = _objects[i];
            objectToSpawn.EnableObject();
            objectToSpawn.transform.position = contentSlots[i].transform.position;
            objectToSpawn.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        }
        Debug.Log("GeneratedContent");
    }


    private void PlaySound(AudioClip clip=null)
    {
        if (clip) _audioSource.PlayOneShot(clip);
        _audioSource.Play();
    }

    private void HideObject(GameObject obj)
    {
        var objInteractableScript = obj.GetComponent<InteractableObject>();
        if (_objects.Count >= maxCapacity || _objects.Contains(objInteractableScript)) return;
        _objects.Add(objInteractableScript);
        objInteractableScript.isHidden = true;
        objInteractableScript.hidingSpot = this;
        UpdateCapacity();
        if (contentUI.activeSelf) GenerateContent();
        PlaySound();
        Debug.Log($"{string.Join(", ", _objects.Select(o => o.name))}, HideObject");
    }

    public void TakeOutObject(InteractableObject interactableObject)
    {
        _objects.Remove(interactableObject);
        interactableObject.isHidden = false;
        interactableObject.hidingSpot = null;
        UpdateCapacity();
        if (_capacity <= 0)
        {
            HideContent();
        }
        else
        {
            GenerateContent();
        }
        Debug.Log($"{string.Join(", ", _objects.Select(o => o.name))}, TakeOutObject");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("InteractableObject")) return;
        ShowCapacity();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("InteractableObject")) return;
        if (!Input.GetMouseButton(0))
        {
            HideObject(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("InteractableObject")) return;
        HideCapacity();
    }
    
}
