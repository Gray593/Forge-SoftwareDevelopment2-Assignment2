using System;
using UnityEngine;

// The game manager class exists to control the in game balance, goal management and tick system 

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // static is used to create one shared copy across all instances and private set prevents external overwriting  

    // Events notify other scripts when something happens, in the case of the first two a float is passed to the other scripts
    public event Action<float> OnBalanceChanged; 
    public event Action<float> OnGoalChanged;     
    public event Action        OnGoalCompleted;   

    // The below values appear in the inspector to control the time between ticks and the starting goal and the goal multiplier 
    [Header("Tick Settings")]
    [SerializeField] private float tickInterval = 2f;   
    [Header("Goal Settings")]
    [SerializeField] private float startingGoal    = 50f;
    [SerializeField] private float goalMultiplier  = 5f; 

    public float Balance       { get; private set; }
    public float CurrentGoal   { get; private set; }
    public int   GoalsCompleted { get; private set; }
    private float _tickTimer;

    // Runs when the object is created to destroy any duplicate instances then sets the current goal to the previously assigned starting goal
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CurrentGoal = startingGoal;
    }
    // The update function runs every frame and is used to control the tick timer and then running the DoTick function when the condition is met 
    private void Update()
    {
        _tickTimer += Time.deltaTime;
        if (_tickTimer >= tickInterval)
        {
            _tickTimer = 0f;
            DoTick();
        }
    }

    // is called by the previous function to evaluate all tile chains and then update the balance
    private void DoTick()
    {
        float earned = GridManager.Instance.EvaluateAllChains();
        if (earned > 0f)
            AddBalance(earned);
    }

    // increases the balance, notifies the subscribed scripts and then checks the goal
    public void AddBalance(float amount)
    {
        Balance += amount;
        OnBalanceChanged?.Invoke(Balance);
        CheckGoal();
    }

    // if the user has enough balance deducts the amount from the balance and then returns true
    public bool SpendBalance(float amount)
    {
        if (Balance < amount) return false;
        Balance -= amount;
        OnBalanceChanged?.Invoke(Balance);
        return true;
    }

    // Evaluates the balance against the goal, completes the goal then notifies subscribed scripts then updates the goal,  
    // notifies subscribed scripts and refreshes the shop for new tile unlocks
    private void CheckGoal()
    {
        if (Balance >= CurrentGoal)
        {
            GoalsCompleted++;
            OnGoalCompleted?.Invoke();
            CurrentGoal *= goalMultiplier;
            OnGoalChanged?.Invoke(CurrentGoal);
            ShopManager.Instance?.RefreshUnlocks(GoalsCompleted);
        }
    }
    public float TickInterval => tickInterval;
}
