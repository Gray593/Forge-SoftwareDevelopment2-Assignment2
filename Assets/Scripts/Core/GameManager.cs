using System;
using UnityEngine;

// Core singleton. Owns balance, the goal system, and the tick timer.

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    //Events
    public event Action<float> OnBalanceChanged;   // new balance value
    public event Action<float> OnGoalChanged;      // new goal target
    public event Action        OnGoalCompleted;    // fired once per completion

    //Inspector
    [Header("Tick Settings")]
    [SerializeField] private float tickInterval = 2f;   // seconds between ticks

    [Header("Goal Settings")]
    [SerializeField] private float startingGoal    = 50f;
    [SerializeField] private float goalMultiplier  = 2.5f;  // how much harder each goal gets

    //State
    public float Balance       { get; private set; }
    public float CurrentGoal   { get; private set; }
    public int   GoalsCompleted { get; private set; }

    private float _tickTimer;

    
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        CurrentGoal = startingGoal;
    }

    private void Update()
    {
        _tickTimer += Time.deltaTime;
        if (_tickTimer >= tickInterval)
        {
            _tickTimer = 0f;
            DoTick();
        }
    }

    //Tick
    private void DoTick()
    {
        float earned = GridManager.Instance.EvaluateAllChains();
        if (earned > 0f)
            AddBalance(earned);
    }

    //Balance
    public void AddBalance(float amount)
    {
        Balance += amount;
        OnBalanceChanged?.Invoke(Balance);
        CheckGoal();
    }

    public bool SpendBalance(float amount)
    {
        if (Balance < amount) return false;
        Balance -= amount;
        OnBalanceChanged?.Invoke(Balance);
        return true;
    }

    //Goals
    private void CheckGoal()
    {
        if (Balance >= CurrentGoal)
        {
            GoalsCompleted++;
            OnGoalCompleted?.Invoke();

            // Advance to next goal
            CurrentGoal *= goalMultiplier;
            OnGoalChanged?.Invoke(CurrentGoal);

            // Notify the shop that a new goal tier may have unlocked tiles
            ShopManager.Instance?.RefreshUnlocks(GoalsCompleted);
        }
    }

    //Helpers
    public float TickInterval => tickInterval;
}
