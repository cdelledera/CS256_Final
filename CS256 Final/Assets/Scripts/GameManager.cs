using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public bool isUIActive = false;
    public TimeOfDay currentTime = TimeOfDay.Morning;
    public int currentGold = 0;
    public int currentDay = 1;

    [Header("Win Conditions")]
    public int maxDays = 7;
    public int goalGold = 10000;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // UPDATED: Now it cleanly accepts just the exact amount of gold earned!
    public void LogQuestResult(int goldEarned)
    {
        if (goldEarned > 0)
        {
            currentGold += goldEarned;
            Debug.Log($"LEDGER: SUCCESS! Earned {goldEarned}g. | Total Gold: {currentGold}");
        }
        else
        {
            Debug.Log($"LEDGER: FAILED. 0g earned. | Total Gold: {currentGold}");
        }
    }

    public void AdvanceTime()
    {
        switch (currentTime)
        {
            case TimeOfDay.Morning:
                currentTime = TimeOfDay.Afternoon;
                Debug.Log("Time shifted to Afternoon.");
                Object.FindFirstObjectByType<CustomerSpawner>().SpawnNextCustomer();
                break;

            case TimeOfDay.Afternoon:
                currentTime = TimeOfDay.Evening;
                Debug.Log("Time shifted to Evening.");
                Object.FindFirstObjectByType<CustomerSpawner>().SpawnNextCustomer();
                break;

            case TimeOfDay.Evening:
                Debug.Log("The tavern is closing for the night...");
                EndTheDay();
                break;
        }
    }

    private void EndTheDay()
    {
        Debug.Log($"--- END OF DAY {currentDay} ---");

        if (currentDay >= maxDays)
        {
            CheckWinCondition();
            return;
        }

        currentDay++;
        currentTime = TimeOfDay.Morning;

        Debug.Log($"Starting Day {currentDay}! Current Bank: {currentGold}g");

        Object.FindFirstObjectByType<CustomerSpawner>().SpawnNextCustomer();
    }

    private void CheckWinCondition()
    {
        if (currentGold >= goalGold)
        {
            Debug.Log($"VICTORY! You survived the week and paid off the Guild with {currentGold}g!");
        }
        else
        {
            Debug.Log($"GAME OVER. You only made {currentGold}g out of {goalGold}g. The Guild shuts you down.");
        }
    }
}