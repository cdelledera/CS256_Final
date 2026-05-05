using UnityEngine;
using TMPro;




public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI References")]
    public TextMeshProUGUI goldTextUI;
    public TextMeshProUGUI timeTextUI; // NEW: The Clock and Calendar text

    [Header("Game State")]
    public bool isUIActive = false;
    public TimeOfDay currentTime = TimeOfDay.Morning;
    public int currentGold = 0;
    public int currentDay = 1;

    [Header("Story Memory")]
    public System.Collections.Generic.List<string> storyFlags = new System.Collections.Generic.List<string>();

    // Call this to permanently remember a choice!
    public void AddStoryFlag(string flag)
    {
        if (!storyFlags.Contains(flag)) storyFlags.Add(flag);
    }

    public LilyState lilyYesterday = LilyState.Any;

    [Header("Win Conditions")]
    public int maxDays = 7;
    public int goalGold = 10000;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateGoldUI();
        UpdateTimeUI(); // NEW: Make sure the clock is right when the game starts!
    }

    public void LogQuestResult(int goldEarned)
    {
        if (goldEarned > 0) currentGold += goldEarned;
        UpdateGoldUI();
    }

    private void UpdateGoldUI()
    {
        if (goldTextUI != null) goldTextUI.text = "Gold: " + currentGold + "g";
    }

    // --- NEW: CLOCK AND CALENDAR LOGIC ---

    public void AdvanceTime()
    {
        // 1. Cycle through the times of day
        if (currentTime == TimeOfDay.Morning)
        {
            currentTime = TimeOfDay.Afternoon;
        }
        else if (currentTime == TimeOfDay.Afternoon)
        {
            currentTime = TimeOfDay.Evening;
        }
        else if (currentTime == TimeOfDay.Evening)
        {
            // 2. If the Evening is over, end the day!
            EndTheDay();
        }

        // 3. Update the screen to show the new time!
        UpdateTimeUI();
    }

    private void EndTheDay()
    {
        currentTime = TimeOfDay.Morning; // Reset the clock
        currentDay++; // Move to tomorrow

        if (currentDay > maxDays)
        {
            // The deadline has arrived!
            Debug.Log("THE WEEK IS OVER! Time to check if the player made enough gold...");
            // (You can trigger your ending cutscene or win/lose screen here later!)
        }
    }

    private void UpdateTimeUI()
    {
        if (timeTextUI != null)
        {
            string dayOfWeek = GetDayOfWeekName(currentDay);

            // This formats it to look like: "Day 1 (Sunday) - Morning"
            timeTextUI.text = $"({dayOfWeek}) - {currentTime}";
        }
    }

    // This converts the integer into a readable word
    private string GetDayOfWeekName(int dayNumber)
    {
        switch (dayNumber)
        {
            case 1: return "Sunday";
            case 2: return "Monday";
            case 3: return "Tuesday";
            case 4: return "Wednesday";
            case 5: return "Thursday";
            case 6: return "Friday";
            case 7: return "Saturday";
            default: return "Unknown Day";
        }
    }
}