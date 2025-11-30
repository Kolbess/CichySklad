using System;
using UnityEngine;

public enum DayPhase
{
    Morning,
    Work,
    Evening,
    Night
}

public class DayCycleManager : MonoBehaviour
{
    public static DayCycleManager Instance { get; private set; }

    [Header("Day Settings")]
    [SerializeField] private int startDay = 1;
    [SerializeField] private float dayDurationSeconds = 300f; // 5 minutes work day? Or event based?
    // User request implies event based "End Day" action, but maybe a timer too? 
    // "Czas działa na naszą korzyść" (Time works in our favor) implies maybe survival time.
    // For now, we'll stick to manual phase changes triggered by player actions or UI buttons for prototype.

    public int CurrentDay { get; private set; }
    public DayPhase CurrentPhase { get; private set; }

    public event Action<int> OnDayStarted;
    public event Action<DayPhase> OnPhaseChanged;
    public event Action OnDayEnded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Keep across scenes if needed
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        CurrentDay = startDay;
        // Don't start immediately, wait for game init? 
        // For now, let's start day 1 automatically or wait for a "Start Game" trigger.
        // Let's assume we start in Morning phase.
        StartDay(); 
    }

    // Daily Targets
    public int TargetLeafletsToHide { get; private set; }
    public int TargetLeafletsToSend { get; private set; }

    public void StartDay()
    {
        CurrentPhase = DayPhase.Morning;
        
        // Generate Daily Targets
        TargetLeafletsToHide = UnityEngine.Random.Range(3, 8);
        TargetLeafletsToSend = UnityEngine.Random.Range(1, 5);

        OnDayStarted?.Invoke(CurrentDay);
        OnPhaseChanged?.Invoke(CurrentPhase);
        
        // Reset Daily Risk
        if (RiskManager.Instance != null)
        {
            RiskManager.Instance.ResetRisk();
        }

        // Reset Daily Resources Stats
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.ResetDailyStats();
        }

        Debug.Log($"Day {CurrentDay} Started. Phase: {CurrentPhase}. Targets: Hide {TargetLeafletsToHide}, Send {TargetLeafletsToSend}");
    }

    public void StartWork()
    {
        if (CurrentPhase != DayPhase.Morning) return;

        CurrentPhase = DayPhase.Work;
        OnPhaseChanged?.Invoke(CurrentPhase);
        Debug.Log($"Phase Changed: {CurrentPhase}");
    }

    public void EndWork()
    {
        if (CurrentPhase != DayPhase.Work) return;

        CurrentPhase = DayPhase.Evening;
        OnPhaseChanged?.Invoke(CurrentPhase);
        Debug.Log($"Phase Changed: {CurrentPhase}");
        
        // Trigger End of Day Events here (Patrol, etc)
        TriggerEndOfDayEvents();
    }

    private void TriggerEndOfDayEvents()
    {
        // 1. Warehouse Closing (Visuals) - Handled by UI/GameManager listening to OnPhaseChanged
        
        // 2. Calculate Penalties
        if (ResourceManager.Instance != null && RiskManager.Instance != null)
        {
            int missingSent = Mathf.Max(0, TargetLeafletsToSend - ResourceManager.Instance.LeafletsSentToday);
            if (missingSent > 0)
            {
                float penalty = missingSent * 10f; // 10% risk per missing package
                RiskManager.Instance.AddRisk(penalty);
                Debug.Log($"Penalty applied: {penalty}% risk for {missingSent} missing packages.");
            }
        }

        // 3. Patrol Ochrana
        Debug.Log("Event: Ochrana Patrol Passing...");
        // Increase risk slightly for the patrol
        if (RiskManager.Instance != null)
        {
            RiskManager.Instance.AddRisk(5f); // Arbitrary small amount
        }
    }

    public void CompleteDay()
    {
        if (CurrentPhase != DayPhase.Evening) return;

        CurrentPhase = DayPhase.Night;
        OnPhaseChanged?.Invoke(CurrentPhase);
        OnDayEnded?.Invoke();

        Debug.Log($"Day {CurrentDay} Completed.");

        // Prepare for next day
        CurrentDay++;
        // Invoke StartDay() after a delay or UI interaction? 
        // For now, let's leave it to be called manually or by a "Next Day" button.
    }
}
