using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// Drives the in-game day: advances a timer through six time-of-day sprites, fires a random
/// narrative event and a random resource windfall each day, and shows a day-transition screen.
/// A run is won on day 7 with full trust. Talks to injected risk/resource/dialogue systems.
/// </summary>
public class DayCycle : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Risk source reset and seeded at each day boundary. Required.")]
    [SerializeField]
    private RiskManager _riskManager;

    [Tooltip("Resource source granted the daily windfall. Required.")]
    [SerializeField]
    private ResourceManager _resourceManager;

    [Tooltip("Dialogue presenter for Maria's daily warning. Required.")]
    [FormerlySerializedAs("dialogueSystem")]
    [SerializeField]
    private DialogueSystem _dialogueSystem;

    [Header("Timing")]
    [Tooltip("Real seconds a full in-game day lasts. Must be > 0.")]
    [FormerlySerializedAs("dayTime")]
    [SerializeField]
    private float _dayDuration = 30f;

    [Header("Time-of-day Visuals")]
    [Tooltip("Six sprites cycled through across a day (index 0..5). Required.")]
    [FormerlySerializedAs("daySprites")]
    [SerializeField]
    private List<Sprite> _daySprites;

    [Tooltip("Image whose sprite is swapped to show the current time of day. Required.")]
    [FormerlySerializedAs("_spriteRenderer")]
    [SerializeField]
    private Image _dayImage;

    [Header("Day Transition Screen")]
    [Tooltip("Overlay shown briefly at the start of each new day. Required.")]
    [FormerlySerializedAs("dayScreen")]
    [SerializeField]
    private GameObject _dayScreen;

    [Tooltip("Text showing the current in-game date. Required.")]
    [FormerlySerializedAs("dayText")]
    [SerializeField]
    private TextMeshProUGUI _dayText;

    [Header("Win Condition")]
    [Tooltip("Day number on which full trust wins the run.")]
    [SerializeField]
    private int _winDay = 7;

    [Tooltip("Scene loaded when the run is won.")]
    [SerializeField]
    private string _endGameSceneName = "EndGame";

    private const int TimeSegments = 6;

    private float _timer;
    private float _lastDayRisk;
    private int _currentDay;
    private bool _dayDisplay;
    private bool _eventA;
    private bool _eventB;

    private void OnValidate()
    {
        if (_dayDuration <= 0f)
            _dayDuration = 1f;
    }

    private void Awake()
    {
        Assert.IsNotNull(_riskManager, $"[{nameof(DayCycle)}] RiskManager unassigned on {name}!");
        Assert.IsNotNull(
            _resourceManager,
            $"[{nameof(DayCycle)}] ResourceManager unassigned on {name}!"
        );
        Assert.IsNotNull(
            _dialogueSystem,
            $"[{nameof(DayCycle)}] DialogueSystem unassigned on {name}!"
        );
        Assert.IsNotNull(_dayImage, $"[{nameof(DayCycle)}] Day image unassigned on {name}!");
        Assert.IsNotNull(_dayScreen, $"[{nameof(DayCycle)}] Day screen unassigned on {name}!");
        Assert.IsNotNull(_dayText, $"[{nameof(DayCycle)}] Day text unassigned on {name}!");
    }

    private void Start()
    {
        _timer = _dayDuration;
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_dayDisplay && _timer > 3f)
            EndDayDisplay();

        UpdateTimeOfDay();

        if (_timer >= _dayDuration)
        {
            _timer = 0f;
            EndDay();
        }
    }

    private void UpdateTimeOfDay()
    {
        float segment = _timer / (_dayDuration / TimeSegments);

        if (segment < 1f)
        {
            _dayImage.sprite = _daySprites[0];
        }
        else if (segment < 2f)
        {
            if (!_eventA)
                GetRandomEvent();
            _dayImage.sprite = _daySprites[1];
        }
        else if (segment < 3f)
        {
            if (!_eventB)
            {
                _eventA = false;
                GetRandomEventB();
            }
            _dayImage.sprite = _daySprites[2];
        }
        else if (segment < 4f)
        {
            if (_eventB)
                _eventB = false;
            _dayImage.sprite = _daySprites[3];
        }
        else if (segment < 5f)
        {
            _dayImage.sprite = _daySprites[4];
        }
        else if (segment < 6f)
        {
            _dayImage.sprite = _daySprites[5];
        }
    }

    private void EndDay()
    {
        if (_currentDay == _winDay && _resourceManager.Trust == 100)
            SceneManager.LoadScene(_endGameSceneName);

        _lastDayRisk = _riskManager.CurrentRisk;
        _riskManager.SetRisk(0);
        StartDay();
    }

    private void GetRandomEvent()
    {
        _eventA = true;
        switch (Random.Range(1, 3))
        {
            case 1:
                GameEvents.CourierInjured();
                break;
            case 2:
                GameEvents.KnockAtDoor();
                break;
            case 3:
                GameEvents.NeighborPeeking();
                break;
        }
    }

    private void GetRandomEventB()
    {
        _eventB = true;
        switch (Random.Range(1, 5))
        {
            case 1:
                _resourceManager.AddMoney(3);
                break;
            case 2:
                _resourceManager.AddPaper(3);
                break;
            case 3:
                _resourceManager.AddInk(3);
                break;
            case 4:
                _resourceManager.AddLeaflets(3);
                break;
            case 5:
                _resourceManager.AddTrust(10);
                break;
        }
    }

    private void StartDay()
    {
        _currentDay += 1;
        _timer = 0f;
        DisplayDayScreen();

        int randomMultiplier = Random.Range(0, 10);
        _riskManager.SetRisk((_lastDayRisk / 10) * randomMultiplier);

        string warning =
            _riskManager.CurrentRisk > 50
                ? "Uważaj Ochrana dzisiaj czuwa."
                : "Dzisiejszy dzień powinien być spokojny.";
        _dialogueSystem.ShowDialogue(warning, _dialogueSystem.GetPortraitSprite("maria"));
    }

    private void DisplayDayScreen()
    {
        _dayScreen.SetActive(true);
        _dayText.text = $"{3 + _currentDay} Listopad 1945";
        _dayDisplay = true;
    }

    private void EndDayDisplay()
    {
        _dayScreen.SetActive(false);
        _dayDisplay = false;
        _timer = 0f;
    }
}
