using TMPro;
using UnityEngine;
using UnityEngine.UI;


// Hooks into GameManager events and keeps the HUD in sync.
// Assign references in the Inspector.

public class UIManager : MonoBehaviour
{
    [Header("Balance")]
    [SerializeField] private TMP_Text balanceText;

    [Header("Goal")]
    [SerializeField] private TMP_Text goalText;
    [SerializeField] private Slider   goalProgressBar;

    [Header("Tick Progress")]
    [SerializeField] private Slider   tickProgressBar;   // optional visual tick timer

    
    private void Start()
    {
        GameManager gm = GameManager.Instance;
        gm.OnBalanceChanged += UpdateBalance;
        gm.OnGoalChanged    += UpdateGoal;
        gm.OnGoalCompleted  += OnGoalCompleted;

        // Initialise displays
        UpdateBalance(gm.Balance);
        UpdateGoal(gm.CurrentGoal);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnBalanceChanged -= UpdateBalance;
        GameManager.Instance.OnGoalChanged    -= UpdateGoal;
        GameManager.Instance.OnGoalCompleted  -= OnGoalCompleted;
    }

    //Callbacks
    private void UpdateBalance(float balance)
    {
        if (balanceText)
            balanceText.text = $"£ {FormatNumber(balance)}";

        float goal = GameManager.Instance.CurrentGoal;
        if (goalProgressBar)
            goalProgressBar.value = Mathf.Clamp01(balance / goal);
    }

    private void UpdateGoal(float goal)
    {
        if (goalText)
            goalText.text = $"Goal: £ {FormatNumber(goal)}";

        // Reset progress bar to current fraction
        if (goalProgressBar)
            goalProgressBar.value = Mathf.Clamp01(GameManager.Instance.Balance / goal);
    }

    private void OnGoalCompleted()
    {
        NotificationManager.Instance?.ShowNotification("Goal Reached!");
        AudioManager.Instance?.PlayGoalComplete();
    }

    //Tick Progress
    private void Update()
    {
        // Animate the optional tick bar so players can see when the next tick is
        if (tickProgressBar && GameManager.Instance != null)
        {
            float interval   = GameManager.Instance.TickInterval;
            float timeInCycle = Time.time % interval;
            tickProgressBar.value = timeInCycle / interval;
        }
    }

    //Number Formatter
    private string FormatNumber(float n)
    {
        if (n >= 1_000_000_000) return $"{n / 1_000_000_000f:0.##}B";
        if (n >= 1_000_000)     return $"{n / 1_000_000f:0.##}M";
        if (n >= 1_000)         return $"{n / 1_000f:0.##}K";
        return $"{n:0.##}";
    }
}
