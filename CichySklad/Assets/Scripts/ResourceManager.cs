using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;
    
    private static readonly int NoResource = Animator.StringToHash("NoResource");
    [SerializeField] private GameObject noPaperWarning;
    [SerializeField] private GameObject noInkWarning;
    [SerializeField] private GameObject noLeafletsWarning;
    [SerializeField] private GameObject noMoneyWarning;
    private Animator _paperAnimator;
    private Animator _inkAnimator;
    private Animator _leafletsAnimator;
    private Animator _moneyAnimator;
    [SerializeField] private GameObject paperPrefab;
    [SerializeField] private GameObject inkPrefab;
    [SerializeField] private GameObject leafletsPrefab;
    [SerializeField] private GameObject moneyPrefab;
    [SerializeField] private Transform resourceSpawnPoint;
    private List<GameObject> _resources = new List<GameObject>(); 
    
// ---------------- PAPER ----------------
    public int paper
    {
        get => _paper;
        set
        {
            if (value < 0) value = 0;

            if (value > _paper)
                SpawnResource(paperPrefab, value - _paper);
            else if (value < _paper)
                DestroyResource(paperPrefab, _paper - value);

            _paper = value;
            UpdatePaper();
        }
    }
    private int _paper;
    [SerializeField] private TextMeshProUGUI paperText;


// ---------------- INK ----------------

    public int ink
    {
        get => _ink;
        set
        {
            if (value < 0) value = 0;

            if (value > _ink)
                SpawnResource(inkPrefab, value - _ink);
            else if (value < _ink)
                DestroyResource(inkPrefab, _ink - value);

            _ink = value;
            UpdateInk();
        }
    }
    private int _ink;
    [SerializeField] private TextMeshProUGUI inkText;


// ---------------- LEAFLETS ----------------

    public int leaflets
    {
        get => _leaflets;
        set
        {
            if (value < 0) value = 0;

            if (value > _leaflets)
                SpawnResource(leafletsPrefab, value - _leaflets);
            else if (value < _leaflets)
                DestroyResource(leafletsPrefab, _leaflets - value);

            _leaflets = value;
            UpdateLeaflets();
        }
    }
    private int _leaflets;
    [SerializeField] private TextMeshProUGUI leafletsText;


// ---------------- MONEY ----------------

    public int money
    {
        get => _money;
        set
        {
            if (value < 0) value = 0;

            if (value > _money)
                SpawnResource(moneyPrefab, value - _money);
            else if (value < _money)
                DestroyResource(moneyPrefab, _money - value);

            _money = value;
            UpdateMoney();
        }
    }
    private int _money;
    [SerializeField] private TextMeshProUGUI moneyText;

    public int trust
    {
        get => _trust;
        set
        {
            _trust = value;
            UpdateTrust();
        }
    }
    private int _trust;
    [SerializeField] private Slider trustSlider;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    private void Start()
    {
        paper += 2;
        money += 5;
        ink += 2;
        UpdatePaper();
        UpdateInk();
        UpdateLeaflets();
        UpdateMoney();
        _paperAnimator = noPaperWarning.GetComponent<Animator>();
        _inkAnimator = noInkWarning.GetComponent<Animator>();
        _leafletsAnimator = noLeafletsWarning.GetComponent<Animator>();
        _moneyAnimator = noMoneyWarning.GetComponent<Animator>();
    }

    private void SpawnResource(GameObject prefab, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-0.2f, 0.2f),
                Random.Range(-0.2f, 0.2f),
                0f
            );
            
            _resources.Add(Instantiate(prefab, resourceSpawnPoint.position + offset, Quaternion.identity));
        }
    }

    private void DestroyResource(GameObject prefab, int amount)
    {
        for (int i = _resources.Count - 1; i >= 0 && amount > 0; i--)
        {
            GameObject res = _resources[i];
            if (res)
            {
                if (res.name.Contains(prefab.name))
                {
                    Destroy(res);
                    _resources.RemoveAt(i);
                    amount--;
                }
            }
        }
    }


    private void UpdatePaper()
    {
        paperText.text = $"{_paper}";
    }
    
    private void UpdateInk()
    {
        inkText.text = $"{_ink}";
    }
    
    private void UpdateLeaflets()
    {
        leafletsText.text = $"{_leaflets}";
    }
    
    private void UpdateMoney()
    {
        moneyText.text = $"{_money}";
    }

    private void UpdateTrust()
    {
        trustSlider.value = _trust / 100f;
    }

    public bool TrySpend(int costPaper = 0, int costInk = 0, int costLeaflets = 0, int costMoney = 0)
    {
        if (paper < costPaper)
        {
            DisplayNoResourceWarning("paper");
        }
        if (ink < costInk)
        {
            DisplayNoResourceWarning("ink");
        }
        if (leaflets < costLeaflets)
        {
            DisplayNoResourceWarning("leaflets");
        }
        if (money < costMoney)
        {
            DisplayNoResourceWarning("money");
        }
        if (paper < costPaper || ink < costInk || leaflets < costLeaflets || money < costMoney) return false;
        paper -= costPaper;
        ink -= costInk;
        leaflets -= costLeaflets;
        money -= costMoney;
        return true;
    }

    private void DisplayNoResourceWarning(string resource)
    {
        switch (resource)
        {
            case "paper":
                NoPaperWarning();
                break;
            case "ink":
                NoInkWarning();
                break;
            case "leaflets":
                NoLeafletsWarning();
                break;
            case "money":
                NoMoneyWarning();
                break;
        }
    }

    private void NoPaperWarning()
    {
        _paperAnimator.SetTrigger(NoResource);
    }

    private void NoInkWarning()
    {
        _inkAnimator.SetTrigger(NoResource);
    }
    
    private void NoLeafletsWarning()
    {
        _leafletsAnimator.SetTrigger(NoResource);
    }
    
    private void NoMoneyWarning()
    {
        _moneyAnimator.SetTrigger(NoResource);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            EventSystem.RumorsSpread();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    
}
