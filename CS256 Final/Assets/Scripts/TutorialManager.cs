using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Settings")]
    [Tooltip("Check this to completely skip the tutorial on Day 1.")]
    public bool skipTutorial = false;

    [HideInInspector] public bool isTutorialActive = false;
    public int tutorialStep = 0;

    private Image blackScreen;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (GameManager.Instance.currentDay == 1)
        {
            // --- FIXED: Check if we are testing OR skipping before forcing the tutorial on! ---
            if (skipTutorial || GameManager.Instance.enableDebugMode)
            {
                isTutorialActive = false;

                // --- NEW: Play the music immediately since we skipped the intro! ---
                if (AudioManager.Instance != null) AudioManager.Instance.PlayMusic("Afternoon_Theme");

                // The Spawner is programmed to wait for the tutorial on Day 1. 
                // Since we skipped it, we have to manually tell the spawner to send the first guy in!
                FindFirstObjectByType<CustomerSpawner>().Invoke("SpawnNextCustomer", 1.0f);
            }
            else
            {
                isTutorialActive = true;
                CreateBlackScreen();
                StartCoroutine(IntroSequence());
            }
        }
    }

    void CreateBlackScreen()
    {
        GameObject canvasObj = new GameObject("TutorialFadeCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        GameObject panelObj = new GameObject("BlackPanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        blackScreen = panelObj.AddComponent<Image>();
        blackScreen.color = Color.black;

        RectTransform rt = panelObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }

    IEnumerator IntroSequence()
    {
        GameManager.Instance.isUIActive = true; // --- FREEZES PLAYER DURING FADE! ---

        yield return new WaitForSeconds(0.5f);

        float elapsed = 0f;
        float duration = 3.0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            blackScreen.color = new Color(0, 0, 0, Mathf.Lerp(1f, 0f, elapsed / duration));
            yield return null;
        }
        Destroy(blackScreen.canvas.gameObject);

        // --- NEW: CUE THE MUSIC THE EXACT SECOND THE TAVERN IS REVEALED! ---
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMusic("Afternoon_Theme");

        tutorialStep = 1;
        DialogueManager.Instance.ShowInnerMonologueSequence(new string[]
        {
            "(...This is it. The birds have officially taken over.)",
            "(And with it, my potion shop was forcibly shut down...)",
            "(But I can't just abandon the people who rely on me.)",
            "(I'm going to run this tavern, and sell my potions under the counter.)",
            "(It's risky... but it's the only way to help.)"
        });
    }

    void Update()
    {
        if (!isTutorialActive) return;

        // Step 1 -> 2: Lore finished, spawn the customer!
        if (tutorialStep == 1 && !DialogueManager.Instance.innerMonologuePanel.activeSelf)
        {
            tutorialStep = 2;
            FindFirstObjectByType<CustomerSpawner>().SpawnNextCustomer();
        }

        // Step 2 -> 3: Wait for Joe to reach the counter
        if (tutorialStep == 2)
        {
            CustomerController firstCust = FindFirstObjectByType<CustomerController>();
            if (firstCust != null && firstCust.currentState == CustomerController.CustomerState.Waiting)
            {
                tutorialStep = 3;
                DialogueManager.Instance.ShowInnerMonologueSequence(new string[] {
                    "(Looks like my first customer is here.)",
                    "(I should use WASD or the Arrow Keys to walk up to the counter and click on him.)"
                });
            }
        }

        // Step 3 -> 4: Wait for the Action Menu to open
        if (tutorialStep == 3 && DialogueManager.Instance.actionMenuPanel.activeSelf)
        {
            tutorialStep = 4;
            DialogueManager.Instance.ShowInnerMonologueSequence(new string[] {
                "(From here, I have three choices...)",
                "([Talk] allows me to learn about their day and gather information...)",
                "([Serve] allows me to give them a drink I've brewed...)",
                "(And [Leave] lets me step away from the counter to prepare.)"
            });
        }

        // Step 4 -> 5: Wait for the player to click Leave and close the menu
        if (tutorialStep == 4 && !GameManager.Instance.isUIActive && !DialogueManager.Instance.innerMonologuePanel.activeSelf)
        {
            tutorialStep = 5;
            DialogueManager.Instance.ShowInnerMonologueSequence(new string[] {
                "(I need to prep an order before I can serve.)",
                "(I should walk over to my Cauldron and click on it to start brewing.)"
            });
        }

        // Step 7 -> 8: Wait for the player to finish reading the gold text
        if (tutorialStep == 7 && !DialogueManager.Instance.innerMonologuePanel.activeSelf)
        {
            isTutorialActive = false; // NOW the tutorial is officially over!

            // Tell the Spawner it is officially okay to send Lily in!
            FindFirstObjectByType<CustomerSpawner>().Invoke("SpawnNextCustomer", 1.5f);
        }
    }

    public void OnCauldronClicked()
    {
        if (isTutorialActive && tutorialStep == 5)
        {
            tutorialStep = 6;

            DialogueManager.Instance.ShowInnerMonologueSequence(new string[] {
                "(Perfect. Here I can mix ingredients to match what the customers need.)",
                "(Time to get to work!)"
            });
        }
    }

    public void TriggerGoldTutorial()
    {
        StartCoroutine(WaitAndShowGold());
    }

    private IEnumerator WaitAndShowGold()
    {
        while (DialogueManager.Instance.innerMonologuePanel.activeSelf)
        {
            yield return null;
        }

        tutorialStep = 7;
        int targetGold = GameManager.Instance.goalGold;

        DialogueManager.Instance.ShowInnerMonologueSequence(new string[] {
            "(Phew... first customer down. That wasn't so bad.)",
            $"(But I can't relax yet. I need to make at least {targetGold} gold by the deadline.)",
            "(If I don't hit that target...)",
            "(Well... let's just say I'll lose a lot more than just the tavern.)",
            "(I need to keep brewing, serving, and saving every coin I can.)"
        });
    }
}