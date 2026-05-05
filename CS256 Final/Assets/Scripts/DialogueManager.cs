using UnityEngine;
using TMPro;
using System.Collections.Generic; // Required for the memory list!

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI Panels")]
    public GameObject actionMenuPanel;
    public GameObject topicMenuPanel;
    public GameObject dialoguePanel;

    [Header("Text References")]
    public TextMeshProUGUI speakerNameTextUI;
    public TextMeshProUGUI dialogueTextUI;
    public TextMeshProUGUI[] topicButtonTexts;

    private CustomerController currentCustomer;
    private Topic activeTopic;
    private int currentLineIndex = 0;

    private enum DialogueState { Normal, InnerMonologue, ReactingAndLeaving, Greeting }
    private DialogueState currentDialogueState;

    // --- SHORT TERM MEMORY ---
    private List<string> discussedTopics = new List<string>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        actionMenuPanel.SetActive(false);
        topicMenuPanel.SetActive(false);
        dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (dialoguePanel.activeSelf && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E)))
        {
            AdvanceDialogue();
        }
    }

    // --- ACE ATTORNEY FLOW ---
    public void OpenActionMenu(CustomerController customer)
    {
        // If a brand new customer walks up, wipe the memory clean!
        if (currentCustomer != customer)
        {
            discussedTopics.Clear();
            currentCustomer = customer;
        }

        GameManager.Instance.isUIActive = true;

        actionMenuPanel.SetActive(true);
        topicMenuPanel.SetActive(false);
        dialoguePanel.SetActive(false);
    }

    // Button: Leave
    public void CloseAllMenus()
    {
        actionMenuPanel.SetActive(false);
        topicMenuPanel.SetActive(false);
        dialoguePanel.SetActive(false);
        GameManager.Instance.isUIActive = false;
    }

    // Button: Talk
    public void OpenTopicMenu()
    {
        actionMenuPanel.SetActive(false);
        topicMenuPanel.SetActive(true);

        for (int i = 0; i < topicButtonTexts.Length; i++)
        {
            if (i < currentCustomer.myProfile.availableTopics.Length)
            {
                Topic topic = currentCustomer.myProfile.availableTopics[i];

                // 1. Is Lily in the right state?
                bool stateMatch = (topic.requiredLilyState == LilyState.Any || topic.requiredLilyState == GameManager.Instance.lilyYesterday);

                // 2. Is it the right day? (0 means it works any day)
                bool dayMatch = (topic.requiredDay == 0 || topic.requiredDay == GameManager.Instance.currentDay);

                // 3. Does the player have the required story flag?
                bool flagMatch = string.IsNullOrEmpty(topic.requiredStoryFlag) || GameManager.Instance.storyFlags.Contains(topic.requiredStoryFlag);

                bool excludeMatch = string.IsNullOrEmpty(topic.excludedStoryFlag) || !GameManager.Instance.storyFlags.Contains(topic.excludedStoryFlag);
                
                // 4. Has the prerequisite topic been discussed? (Or is it empty?)
                bool prereqMet = string.IsNullOrEmpty(topic.requiredPreviousTopic) || discussedTopics.Contains(topic.requiredPreviousTopic);

                // If ALL conditions are true, show the button!
                if (stateMatch && dayMatch && flagMatch && prereqMet)
                {
                    topicButtonTexts[i].text = topic.topicName;
                    topicButtonTexts[i].transform.parent.gameObject.SetActive(true);
                }
                else
                {
                    topicButtonTexts[i].transform.parent.gameObject.SetActive(false);
                }
            }
            else
            {
                topicButtonTexts[i].transform.parent.gameObject.SetActive(false);
            }
        }
    }

    // Button: Back (From Topics to Action Menu)
    public void BackToActionMenu()
    {
        topicMenuPanel.SetActive(false);
        actionMenuPanel.SetActive(true);
    }

    // Button: Present
    public void PresentPotion()
    {
        actionMenuPanel.SetActive(false);
        currentCustomer.ReceivePotion(); // Tells the customer to check the cauldron!
    }

    // --- DIALOGUE LOGIC ---
    public void OnTopicClicked(int topicIndex)
    {
        currentDialogueState = DialogueState.Normal;
        activeTopic = currentCustomer.myProfile.availableTopics[topicIndex];
        currentLineIndex = 0;

        // Write this topic down in our memory!
        if (!discussedTopics.Contains(activeTopic.topicName))
        {
            discussedTopics.Add(activeTopic.topicName);
        }

        // NEW: Write the flag down in our PERMANENT memory!
        if (!string.IsNullOrEmpty(activeTopic.flagToSet))
        {
            GameManager.Instance.AddStoryFlag(activeTopic.flagToSet);
        }

        topicMenuPanel.SetActive(false);
        dialoguePanel.SetActive(true);
        DisplayCurrentLine();
    }

    private void DisplayCurrentLine()
    {
        DialogueLine line = activeTopic.lines[currentLineIndex];
        speakerNameTextUI.text = line.speaker != null ? line.speaker.characterName : "???";
        dialogueTextUI.text = line.text;
    }

    private void AdvanceDialogue()
    {
        if (currentDialogueState == DialogueState.Normal)
        {
            currentLineIndex++;
            if (currentLineIndex < activeTopic.lines.Length)
            {
                DisplayCurrentLine();
            }
            else
            {
                // Convo over! Go back to Topic selection
                dialoguePanel.SetActive(false);
                OpenTopicMenu(); // Refreshes the menu so newly unlocked topics appear immediately!
            }
        }

        else if (currentDialogueState == DialogueState.Greeting)
        {
            currentLineIndex++;
            if (currentLineIndex < activeTopic.lines.Length)
            {
                DisplayCurrentLine();
            }
            else
            {
                // The intro is over! Close dialogue and open the Action Menu
                dialoguePanel.SetActive(false);
                OpenActionMenu(currentCustomer);
            }
        }
        else if (currentDialogueState == DialogueState.InnerMonologue)
        {
            // Just thinking to ourselves. Go back to the Action Menu!
            dialoguePanel.SetActive(false);
            actionMenuPanel.SetActive(true);
        }
        else if (currentDialogueState == DialogueState.ReactingAndLeaving)
        {
            // --- NEW REACTION LOGIC ---
            // Move to the next line of the reaction conversation!
            currentLineIndex++;

            if (currentLineIndex < activeTopic.lines.Length)
            {
                DisplayCurrentLine();
            }
            else
            {
                // The reaction conversation is completely over! Customer leaves.
                GameManager.Instance.isUIActive = false;
                dialoguePanel.SetActive(false);
                currentCustomer.FinishTransaction();
            }
        }
    }

    // Called if you try to present with empty hands
    public void ShowInnerMonologue(string text)
    {
        currentDialogueState = DialogueState.InnerMonologue;
        dialoguePanel.SetActive(true);
        speakerNameTextUI.text = "Player";
        dialogueTextUI.text = text;
    }

    // --- NEW: AUTO-GREETING LOGIC ---
    public void StartGreeting(CustomerController customer, Topic greetingTopic)
    {
        // Set the active customer and wipe the short term memory if they are new
        if (currentCustomer != customer)
        {
            discussedTopics.Clear();
            currentCustomer = customer;
        }

        GameManager.Instance.isUIActive = true;
        currentDialogueState = DialogueState.Greeting;

        // Load the greeting
        activeTopic = greetingTopic;
        currentLineIndex = 0;

        actionMenuPanel.SetActive(false);
        topicMenuPanel.SetActive(false);
        dialoguePanel.SetActive(true);

        // Still save flags if you want the greeting to trigger a story memory!
        if (!string.IsNullOrEmpty(activeTopic.flagToSet))
        {
            GameManager.Instance.AddStoryFlag(activeTopic.flagToSet);
        }

        DisplayCurrentLine();
    }

    // --- NEW REACTION LOADER ---
    // Called when the customer drinks the potion
    public void ShowReaction(Topic reactionTopic)
    {
        // SAFETY NET: Check if the topic actually has dialogue lines written!
        if (reactionTopic == null || reactionTopic.lines == null || reactionTopic.lines.Length == 0)
        {
            Debug.LogWarning("Wait! You forgot to write dialogue lines for this reaction in the Inspector!");

            // Even if the dialogue is empty, we still want to save the flag if you set one!
            if (reactionTopic != null && !string.IsNullOrEmpty(reactionTopic.flagToSet))
            {
                GameManager.Instance.AddStoryFlag(reactionTopic.flagToSet);
            }

            // Just end the transaction immediately so the game doesn't crash
            GameManager.Instance.isUIActive = false;
            currentCustomer.FinishTransaction();
            return;
        }

        currentDialogueState = DialogueState.ReactingAndLeaving;

        // Load the reaction exactly like a normal topic!
        activeTopic = reactionTopic;
        currentLineIndex = 0;

        dialoguePanel.SetActive(true);
        DisplayCurrentLine();

        if (!string.IsNullOrEmpty(activeTopic.flagToSet))
        {
            GameManager.Instance.AddStoryFlag(activeTopic.flagToSet);
        }
    }
}