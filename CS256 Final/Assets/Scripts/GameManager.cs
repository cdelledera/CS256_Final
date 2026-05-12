using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct OrderRecord
{
    public string customerName;
    public string drinkGiven;
    public int goldEarned;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // --- NEW: DEBUG & TESTING OVERRIDES ---
    [Header("Debug & Testing")]
    [Tooltip("WARNING: Turn this OFF before building the final game!")]
    public bool enableDebugMode = false;
    public int testStartingDay = 1;
    public List<string> testStartingFlags = new List<string>();

    [Header("UI References")]
    public TextMeshProUGUI goldTextUI;
    public TextMeshProUGUI timeTextUI;

    [Header("Game State")]
    public bool isUIActive = false;
    public TimeOfDay currentTime = TimeOfDay.Morning;
    public int currentGold = 0;
    public int currentDay = 1;

    [Header("End of Day UI")]
    public GameObject endOfDayPanel;
    public TextMeshProUGUI dailySummaryText;
    public TextMeshProUGUI totalGoalText;
    public int dailyGoldTracker = 0;

    public List<OrderRecord> todaysOrders = new List<OrderRecord>();

    [Header("Story Memory")]
    public List<string> storyFlags = new List<string>();

    public void AddStoryFlag(string flag)
    {
        if (!storyFlags.Contains(flag)) storyFlags.Add(flag);
    }

    public void RecordTransaction(string cName, string dName, int gold)
    {
        todaysOrders.Add(new OrderRecord { customerName = cName, drinkGiven = dName, goldEarned = gold });
    }

    [Header("Win Conditions")]
    // --- CHANGED: Scope lowered to 6 days! ---
    public int maxDays = 6;
    public int goalGold = 5000;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // --- APPLY DEBUG DATA IF ACTIVE ---
        if (enableDebugMode)
        {
            currentDay = testStartingDay;

            // Inject the test flags so the game thinks we made past choices!
            foreach (string flag in testStartingFlags)
            {
                if (!storyFlags.Contains(flag))
                {
                    storyFlags.Add(flag);
                }
            }

            Debug.Log($"<color=yellow>DEBUG MODE ACTIVE: Starting on Day {currentDay} with {storyFlags.Count} flags.</color>");
        }

        endOfDayPanel.SetActive(false);
        UpdateGoldUI();
        UpdateTimeUI();

        // --- AUDIO: START THE AFTERNOON MUSIC! ---
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic("Afternoon_Theme");
        }
    }

    public void LogQuestResult(int goldEarned)
    {
        if (goldEarned > 0)
        {
            currentGold += goldEarned;
            dailyGoldTracker += goldEarned;
        }
        UpdateGoldUI();
    }

    private void UpdateGoldUI()
    {
        if (goldTextUI != null) goldTextUI.text = "Gold: " + currentGold + "g";
    }

    // --- JUICY GOLD ANIMATION ---
    public void AddGoldWithAnimation(int goldEarned, System.Action onComplete)
    {
        StartCoroutine(GoldAnimationRoutine(goldEarned, onComplete));
    }

    private IEnumerator GoldAnimationRoutine(int goldEarned, System.Action onComplete)
    {
        RectTransform textRect = goldTextUI.GetComponent<RectTransform>();
        Canvas canvas = textRect.GetComponentInParent<Canvas>(); // Find the master Canvas

        // 1. Use World Position so we completely ignore where the text is anchored!
        Vector3 originalPos = textRect.position;
        Vector3 originalScale = textRect.localScale;

        // 2. The Canvas's position is ALWAYS the exact dead-center of the screen
        Vector3 centerPos = canvas.transform.position;
        centerPos.z = originalPos.z; // Keep the same depth so it doesn't clip!

        Vector3 targetScale = originalScale * 2.5f;

        int startGold = currentGold;
        int targetGold = currentGold + goldEarned;

        float duration = 0.4f;
        float elapsed = 0f;

        // FLY TO CENTER
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = Mathf.SmoothStep(0, 1, t);

            // Move the Position, not the AnchoredPosition!
            textRect.position = Vector3.Lerp(originalPos, centerPos, smoothT);
            textRect.localScale = Vector3.Lerp(originalScale, targetScale, smoothT);
            yield return null;
        }

        // NUMBER TICKING PHASE
        float countDuration = 0.6f;
        elapsed = 0f;
        while (elapsed < countDuration)
        {
            elapsed += Time.deltaTime;
            int displayGold = Mathf.RoundToInt(Mathf.Lerp(startGold, targetGold, elapsed / countDuration));
            goldTextUI.text = "Gold: " + displayGold + "g";
            yield return null;
        }

        LogQuestResult(goldEarned);
        yield return new WaitForSeconds(0.3f);

        // FLY BACK PHASE
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = Mathf.SmoothStep(0, 1, t);

            textRect.position = Vector3.Lerp(centerPos, originalPos, smoothT);
            textRect.localScale = Vector3.Lerp(targetScale, originalScale, smoothT);
            yield return null;
        }

        // Lock it perfectly back into place
        textRect.position = originalPos;
        textRect.localScale = originalScale;
        onComplete?.Invoke();
    }

    // ==========================================
    // --- TIME LOGIC ---
    // ==========================================
    public void AdvanceTime()
    {
        if (currentTime == TimeOfDay.Morning)
        {
            currentTime = TimeOfDay.Afternoon;
        }
        else if (currentTime == TimeOfDay.Afternoon)
        {
            currentTime = TimeOfDay.Evening;

            // --- AUDIO: FADE INTO THE EVENING THEME! ---
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.CrossfadeMusic("Evening_Theme", 2.0f);
            }
        }
        else if (currentTime == TimeOfDay.Evening)
        {
            EndTheDay();
            return;
        }

        UpdateTimeUI();
    }

    private void EndTheDay()
    {
        StartCoroutine(EndOfDaySequence());
    }

    private IEnumerator EndOfDaySequence()
    {
        isUIActive = true;

        // --- AUDIO: FADE INTO THE NIGHT THEME! ---
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.CrossfadeMusic("Night_Theme", 2.0f);
        }

        // --- JUICE: SMOOTH FADE IN ---
        CanvasGroup cg = endOfDayPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = endOfDayPanel.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        endOfDayPanel.SetActive(true);

        float fadeElapsed = 0f;
        float fadeDuration = 0.8f;
        while (fadeElapsed < fadeDuration)
        {
            fadeElapsed += Time.deltaTime;
            cg.alpha = Mathf.SmoothStep(0f, 1f, fadeElapsed / fadeDuration);
            yield return null;
        }
        cg.alpha = 1f;

        // --- JUICE: THE CASH REGISTER RECEIPT ---
        totalGoalText.text = ""; // Hide the goal text for a second
        dailySummaryText.text = $"DAILY EARNINGS: {dailyGoldTracker}g\n\n--- TODAY'S RECEIPT ---\n";

        // Print each customer one by one!
        foreach (OrderRecord order in todaysOrders)
        {
            yield return new WaitForSeconds(0.2f); // The typewriter delay

            // --- AUDIO: Optional receipt ticking sound ---
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("ReceiptTick");

            dailySummaryText.text += $"{order.customerName} : {order.drinkGiven} ... +{order.goldEarned}g\n";
        }

        yield return new WaitForSeconds(0.5f);

        // Add the basic stat
        dailySummaryText.text += $"\nCustomers Served: {todaysOrders.Count}\n";

        yield return new WaitForSeconds(1.0f);

        // --- JUICE: VISUAL TICK FOR THE GOAL ---
        int startTotal = currentGold - dailyGoldTracker;
        float elapsed = 0f;
        float duration = 1.5f;

        // --- AUDIO: Optional counting sound ---
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("GoldCountUp");

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            int displayTotal = Mathf.RoundToInt(Mathf.Lerp(startTotal, currentGold, elapsed / duration));
            totalGoalText.text = $"TOTAL: {displayTotal} / {goalGold}g to Goal";
            yield return null;
        }
        totalGoalText.text = $"TOTAL: {currentGold} / {goalGold}g to Goal";

        // Give the player time to read their stats
        yield return new WaitForSeconds(4.0f);

        // Advance Day Variables
        currentDay++;
        currentTime = TimeOfDay.Morning; // Resets back to Morning for the new day
        dailyGoldTracker = 0;
        todaysOrders.Clear(); // Shred the receipt for tomorrow!

        if (timeTextUI != null)
        {
            timeTextUI.text = "Next Day...";
            yield return new WaitForSeconds(1.0f);
            UpdateTimeUI();
        }

        if (currentDay > maxDays)
        {
            dailySummaryText.text = currentGold >= goalGold ? "TAVERN SAVED!" : "TAVERN LOST...";
            yield break;
        }

        // --- AUDIO: FADE BACK INTO THE AFTERNOON THEME! ---
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.CrossfadeMusic("Afternoon_Theme", 2.0f);
        }

        // --- JUICE: SMOOTH FADE OUT ---
        fadeElapsed = 0f;
        while (fadeElapsed < fadeDuration)
        {
            fadeElapsed += Time.deltaTime;
            cg.alpha = Mathf.SmoothStep(1f, 0f, fadeElapsed / fadeDuration);
            yield return null;
        }

        endOfDayPanel.SetActive(false);
        isUIActive = false;

        // --- THE FIX: WAKE UP THE SPAWNER ---
        CustomerSpawner spawner = FindFirstObjectByType<CustomerSpawner>();
        if (spawner != null)
        {
            spawner.StartDay();
        }
    }

    public void UpdateTimeUI()
    {
        if (timeTextUI != null)
        {
            string dayOfWeek = GetDayOfWeekName(currentDay);
            timeTextUI.text = $"({dayOfWeek}) - {currentTime}";
        }
    }

    // --- CHANGED: Dynamic countdown logic! ---
    private string GetDayOfWeekName(int dayNumber)
    {
        int daysLeft = maxDays - dayNumber;

        if (daysLeft > 1)
        {
            return $"{daysLeft} Days Left";
        }
        else if (daysLeft == 1)
        {
            return "1 Day Left"; // Proper grammar for the penultimate day!
        }
        else if (daysLeft == 0)
        {
            return "Final Day";
        }
        else
        {
            return "Overtime"; // Just a safe fallback
        }
    }
}