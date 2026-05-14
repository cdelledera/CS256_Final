using UnityEngine;
using System.Collections.Generic;

// NEW: We wrap your lists and time variables into a neat "Day" package!
[System.Serializable]
public class DailySchedule
{
    public string dayName = "Day 1";

    [Header("Daily Characters")]
    public List<CharacterData> customersToSpawn;

    [Header("Time Management For This Day")]
    public TimeOfDay startingTimeOfDay = TimeOfDay.Morning;

    [Tooltip("How many customers must leave before it shifts to Afternoon?")]
    public int customersBeforeAfternoon = 1;

    [Tooltip("How many customers must leave before it shifts to Evening?")]
    public int customersBeforeEvening = 2;
}

public class CustomerSpawner : MonoBehaviour
{
    [Header("The Master Schedule")]
    public List<DailySchedule> weekSchedule; // Holds Day 1, Day 2, Day 3, etc.

    private int currentCustomerIndex = 0;
    private int customersFinished = 0;
    private DailySchedule today; // Remembers which day we are currently on

    [Header("Scene References")]
    public GameObject customerPrefab;
    public Transform doorLocation;
    public Transform counterLocation;
    public PotionBrewing cauldron;

    private bool dayEnded = false;

    void Start()
    {
        // Start the very first day automatically when the game loads!
        StartDay();
    }

    // Call this function from your End of Day UI when you click "Next Day"!
    public void StartDay()
    {
        // Arrays start at 0. So Day 1 is index 0. Day 2 is index 1.
        int dayIndex = GameManager.Instance.currentDay - 1;

        if (dayIndex >= 0 && dayIndex < weekSchedule.Count)
        {
            today = weekSchedule[dayIndex]; // Load today's specific schedule!

            currentCustomerIndex = 0;
            customersFinished = 0;
            dayEnded = false;

            // 1. Set the clock to whatever you chose for THIS specific day
            GameManager.Instance.currentTime = today.startingTimeOfDay;

            // 2. Safely tell the UI to refresh its text
            GameManager.Instance.UpdateTimeUI();

            // 3. Open the doors! (UNLESS IT IS DAY 1!)
            if (GameManager.Instance.currentDay == 1)
            {
                // Do nothing! Let the TutorialManager spawn the first customer after the lore!
            }
            else
            {
                SpawnNextCustomer();
            }
        }
        else
        {
            Debug.LogWarning("You tried to start Day " + GameManager.Instance.currentDay + ", but it isn't in the schedule!");
        }
    }

    public void SpawnNextCustomer()
    {
        if (currentCustomerIndex < today.customersToSpawn.Count)
        {
            GameObject newCustomer = Instantiate(customerPrefab, doorLocation.position, Quaternion.identity);
            CustomerController controller = newCustomer.GetComponent<CustomerController>();

            // Inject the data from TODAY's list
            controller.myProfile = today.customersToSpawn[currentCustomerIndex];
            controller.brewingSystem = cauldron;
            controller.doorLocation = doorLocation;
            controller.counterLocation = counterLocation;

            // =========================================================
            // --- NEW: INJECT THE SPRITE DIRECTLY IN THE SPAWNER ---
            // =========================================================
            SpriteRenderer spriteRenderer = newCustomer.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && controller.myProfile.worldSprite != null)
            {
                spriteRenderer.sprite = controller.myProfile.worldSprite;
            }

            currentCustomerIndex++;
        }
    }

    public void OnCustomerLeft()
    {
        customersFinished++;

        // 1. Tick the clock based on TODAY'S specific limits!
        if (customersFinished == today.customersBeforeAfternoon)
        {
            GameManager.Instance.AdvanceTime();
        }
        else if (customersFinished == today.customersBeforeEvening)
        {
            GameManager.Instance.AdvanceTime();
        }

        // 2. Are there still people in line? Send them in!
        if (currentCustomerIndex < today.customersToSpawn.Count)
        {
            // --- FIXED: The spawner directly tells the Tutorial that Joe left! ---
            if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive)
            {
                TutorialManager.Instance.TriggerGoldTutorial();
                return; // Stop here! The tutorial will trigger the next spawn when it is ready.
            }

            // Use Invoke to wait 2 seconds before sending the next person in
            Invoke("SpawnNextCustomer", 2f);
        }
        // 3. Is the line empty? And have we NOT ended the day yet?
        else if (!dayEnded)
        {
            dayEnded = true;

            GameManager.Instance.currentTime = TimeOfDay.Evening;
            GameManager.Instance.AdvanceTime(); // Triggers your Night/End of day logic
        }
    }
}