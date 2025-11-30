using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    
    public int paper
    {
        get => _paper;
        set
        {
            _paper = value;
            UpdatePaper();
        }
    }
    private int _paper;
    [SerializeField] private TextMeshProUGUI paperText;

    public int ink
    {
        get => _ink;
        set
        {
            _ink = value;
            UpdateInk();
        }
    }
    private int _ink;
    [SerializeField] private TextMeshProUGUI inkText;

    public int leaflets
    {
        get => _leaflets;
        set
        {
            _leaflets = value;
            UpdateLeaflets();
        }
    }
    private int _leaflets;
    [SerializeField] private TextMeshProUGUI leafletsText;

    public int money
    {
        get => _money;
        set
        {
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

    // Daily Stats
    public int LeafletsHiddenToday { get; private set; }
    public int LeafletsSentToday { get; private set; }


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    private void Start()
    {
        UpdatePaper();
        UpdateInk();
        UpdateLeaflets();
        UpdateMoney();
        _paperAnimator = noPaperWarning.GetComponent<Animator>();
        _inkAnimator = noInkWarning.GetComponent<Animator>();
        _leafletsAnimator = noLeafletsWarning.GetComponent<Animator>();
        _moneyAnimator = noMoneyWarning.GetComponent<Animator>();
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
    }
    
    public void RecordHiddenLeaflet()
    {
        LeafletsHiddenToday++;
    }

    public void RecordSentLeaflet()
    {
        LeafletsSentToday++;
    }

    public void ResetDailyStats()
    {
        LeafletsHiddenToday = 0;
        LeafletsSentToday = 0;
        Debug.Log("Daily Stats Reset");
    }
}
