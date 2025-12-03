using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class DayCycle : MonoBehaviour
{
    private float timer = 0f;
    private float dayTime = 30; //5 minut
    [SerializeField] private List<Sprite> daySprites;
    [SerializeField] private Image _spriteRenderer;
    [SerializeField] private DialogueSystem dialogueSystem;
    private float lastDayRisk;
    [SerializeField] private GameObject dayScreen;
    [SerializeField] private TextMeshProUGUI dayText;
    private int currentDay = 0;
    private bool dayDisplay = false;
    private bool eventA = false;
    private bool eventB = false;

    private void Start()
    {
        timer = dayTime;
    }
    
    private void Update()
    {
        timer += Time.deltaTime;
        if (dayDisplay && timer > 3f)
        {
            EndDayDisplay();
        }
        if (timer / (dayTime/6) < 1f)
        {
            _spriteRenderer.sprite = daySprites[0];
        } else if (timer / (dayTime/6) < 2f)
        {
            if (!eventA)
            {
                GetRandomEvent();
            }
            _spriteRenderer.sprite = daySprites[1];
        } else if (timer / (dayTime/6) < 3f)
        {
            if (!eventB)
            {
                eventA = false;
                GetRandomEventB();
            }
            _spriteRenderer.sprite = daySprites[2];
        } else if (timer / (dayTime/6) < 4f)
        {
            if (eventB)
            {
                eventB = false;
            }
            _spriteRenderer.sprite = daySprites[3];
        } else if (timer / (dayTime/6) < 5f)
        {
            _spriteRenderer.sprite = daySprites[4];
        } else if (timer / (dayTime/6) < 6f)
        {
            _spriteRenderer.sprite = daySprites[5];
        }

        if (timer >= dayTime)
        {
            timer = 0f;
            EndDay();
        }
    }

    private void EndDay()
    {
        if (currentDay == 7 && ResourceManager.Instance.trust == 100)
        {
            SceneManager.LoadScene("EndGame");
        }
        lastDayRisk = RiskManager.Instance.CurrentRisk;
        RiskManager.Instance.SetRisk(0);
        StartDay();
    }

    private void GetRandomEvent()
    {
        eventA = true;
        var randomInt = Random.Range(1, 3);
        switch (randomInt)
        {
            case 1:
                EventSystem.CourierInjured();
                break;
            case 2:
                EventSystem.KnockAtDoor();
                break;
            case 3:
                EventSystem.NeighborPeeking();
                break;
        }
    }
    
    private void GetRandomEventB()
    {
        eventB = true;
        var randomInt = Random.Range(1, 5);
        switch (randomInt)
        {
            case 1:
                ResourceManager.Instance.money += 3;
                break;
            case 2:
                ResourceManager.Instance.paper += 3;
                break;
            case 3:
                ResourceManager.Instance.ink += 3;
                break;
            case 4:
                ResourceManager.Instance.leaflets += 3;
                break;
            case 5:
                ResourceManager.Instance.trust += 10;
                break;
        }
    }

    private void StartDay()
    {
        currentDay += 1;
        timer = 0f;
        DisplayDayScreen();
        var randomMultiplier = Random.Range(0, 10);
        RiskManager.Instance.SetRisk((lastDayRisk/10)*randomMultiplier);
        if (RiskManager.Instance.CurrentRisk > 50)
        {
            dialogueSystem.ShowDialogue($"Uważaj Ochrana dzisiaj czuwa.", portrait: dialogueSystem.GetPortraitSprite("maria"));
        }
        else
        {
            dialogueSystem.ShowDialogue($"Dzisiejszy dzień powinien być spokojny.", portrait: dialogueSystem.GetPortraitSprite("maria"));
        }
    }

    private void DisplayDayScreen()
    {
        dayScreen.SetActive(true);
        dayText.text = $"{3 + currentDay} Listopad 1945";
        dayDisplay = true;
    }

    private void EndDayDisplay()
    {
        dayScreen.SetActive(false);
        dayDisplay = false;
        timer = 0f;
    }
}
