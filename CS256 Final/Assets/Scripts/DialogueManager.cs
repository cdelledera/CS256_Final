using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI Panels")]
    public CanvasGroup interactionMasterGroup;
    public GameObject actionMenuPanel;
    public GameObject topicMenuPanel;
    public GameObject dialoguePanel;

    public GameObject innerMonologuePanel;
    public TextMeshProUGUI innerMonologueTextUI;

    [Header("Monologue Positions")]
    public Vector2 monologuePosDefault = new Vector2(0, -350);
    public Vector2 monologuePosWithMenu = new Vector2(0, 0);
    private RectTransform innerMonologueRect;

    [Header("Text & UI References")]
    public TextMeshProUGUI speakerNameTextUI;
    public TextMeshProUGUI dialogueTextUI;
    public TextMeshProUGUI[] topicButtonTexts;
    public Image portraitImageUI;

    [Header("Typewriter Settings")]
    public float baseTypeSpeed = 0.04f;
    public AudioSource voiceAudioSource;
    public AudioClip voiceBlip;
    [Range(1, 5)] public int blipFrequency = 2;

    [Header("Ace Attorney Effects")]
    public Camera mainCamera;
    public CanvasGroup flashPanel;
    public AudioClip slamSound;

    private Coroutine typingCoroutine;
    private string currentFullLine;
    [HideInInspector] public bool isTyping = false;
    private bool isTransitioning = false;

    private CustomerController currentCustomer;
    private Topic activeTopic;
    private int currentLineIndex = 0;
    private bool preventImmediateAdvance = false;

    private enum DialogueState { Normal, InnerMonologue, ReactingAndLeaving, ReactingAndContinuing, Greeting }
    private DialogueState currentDialogueState;

    private List<string> discussedTopics = new List<string>();
    private List<Topic> visibleTopics = new List<Topic>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (interactionMasterGroup != null)
        {
            interactionMasterGroup.gameObject.SetActive(false);
            interactionMasterGroup.alpha = 0f;
        }

        actionMenuPanel.SetActive(false);
        topicMenuPanel.SetActive(false);
        dialoguePanel.SetActive(false);
        if (innerMonologuePanel != null) innerMonologuePanel.SetActive(false);

        if (innerMonologuePanel != null) innerMonologueRect = innerMonologuePanel.GetComponent<RectTransform>();
    }

    void Update()
    {
        if (innerMonologueRect != null && innerMonologuePanel.activeSelf)
        {
            Vector2 targetPos = (actionMenuPanel.activeSelf || topicMenuPanel.activeSelf) ? monologuePosWithMenu : monologuePosDefault;
            innerMonologueRect.anchoredPosition = Vector2.Lerp(innerMonologueRect.anchoredPosition, targetPos, Time.deltaTime * 10f);
        }

        if (preventImmediateAdvance)
        {
            preventImmediateAdvance = false;
            return;
        }

        if (isTransitioning) return;

        if ((dialoguePanel.activeSelf || (innerMonologuePanel != null && innerMonologuePanel.activeSelf)) && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E)))
        {
            if (isTyping) SkipTyping();
            else AdvanceDialogue();
        }
    }

    private void EnsureMasterIsActiveAndFadedIn(System.Action onComplete = null)
    {
        if (!interactionMasterGroup.gameObject.activeSelf || interactionMasterGroup.alpha < 1f)
        {
            StartCoroutine(FadeInMaster(onComplete));
        }
        else onComplete?.Invoke();
    }

    private System.Collections.IEnumerator FadeInMaster(System.Action onComplete)
    {
        isTransitioning = true;
        preventImmediateAdvance = true;
        interactionMasterGroup.gameObject.SetActive(true);
        interactionMasterGroup.alpha = 0f;

        RectTransform rect = interactionMasterGroup.GetComponent<RectTransform>();
        Vector3 originalScale = Vector3.one;

        float duration = 0.35f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = t * t * (3f - 2f * t);
            interactionMasterGroup.alpha = Mathf.Lerp(0f, 1f, smoothT);
            rect.localScale = Vector3.Lerp(new Vector3(0.95f, 0.95f, 1f), originalScale, smoothT);
            yield return null;
        }

        interactionMasterGroup.alpha = 1f;
        rect.localScale = originalScale;
        yield return new WaitForSeconds(0.15f);
        isTransitioning = false;
        onComplete?.Invoke();
    }

    private System.Collections.IEnumerator FadeOutMaster(System.Action onComplete)
    {
        isTransitioning = true;
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            interactionMasterGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        interactionMasterGroup.alpha = 0f;
        interactionMasterGroup.gameObject.SetActive(false);
        isTransitioning = false;
        onComplete?.Invoke();
    }

    public void OpenActionMenu(CustomerController customer)
    {
        if (currentCustomer != customer)
        {
            discussedTopics.Clear();
            currentCustomer = customer;
        }

        GameManager.Instance.isUIActive = true;
        actionMenuPanel.SetActive(true);
        topicMenuPanel.SetActive(false);
        dialoguePanel.SetActive(false);

        EnsureMasterIsActiveAndFadedIn();
    }

    public void CloseAllMenus()
    {
        if (interactionMasterGroup.gameObject.activeSelf)
        {
            StartCoroutine(FadeOutMaster(() =>
            {
                actionMenuPanel.SetActive(false);
                topicMenuPanel.SetActive(false);
                dialoguePanel.SetActive(false);
                if (innerMonologuePanel != null) innerMonologuePanel.SetActive(false);
                GameManager.Instance.isUIActive = false;
            }));
        }
        else GameManager.Instance.isUIActive = false;
    }

    public void OpenTopicMenu()
    {
        actionMenuPanel.SetActive(false);
        topicMenuPanel.SetActive(true);
        visibleTopics.Clear();

        for (int i = 0; i < currentCustomer.myProfile.availableTopics.Length; i++)
        {
            Topic topic = currentCustomer.myProfile.availableTopics[i];
            bool dayMatch = (topic.requiredDay == 0 || topic.requiredDay == GameManager.Instance.currentDay);

            bool flagMatch = true;
            foreach (string req in topic.requiredStoryFlags) { if (!string.IsNullOrEmpty(req) && !GameManager.Instance.storyFlags.Contains(req)) flagMatch = false; }

            bool excludeMatch = true;
            foreach (string exc in topic.excludedStoryFlags) { if (!string.IsNullOrEmpty(exc) && GameManager.Instance.storyFlags.Contains(exc)) excludeMatch = false; }

            bool prereqMet = string.IsNullOrEmpty(topic.requiredPreviousTopic) || discussedTopics.Contains(topic.requiredPreviousTopic);

            if (dayMatch && flagMatch && excludeMatch && prereqMet) visibleTopics.Add(topic);
        }

        for (int i = 0; i < topicButtonTexts.Length; i++)
        {
            if (i < visibleTopics.Count)
            {
                topicButtonTexts[i].text = visibleTopics[i].topicName;
                topicButtonTexts[i].transform.parent.gameObject.SetActive(true);
            }
            else topicButtonTexts[i].transform.parent.gameObject.SetActive(false);
        }
    }

    public void BackToActionMenu()
    {
        topicMenuPanel.SetActive(false);
        actionMenuPanel.SetActive(true);
    }

    public void PresentPotion()
    {
        actionMenuPanel.SetActive(false);
        currentCustomer.ReceivePotion();
    }

    public void OnTopicClicked(int topicIndex)
    {
        if (topicIndex >= visibleTopics.Count) return;

        preventImmediateAdvance = true;
        currentDialogueState = DialogueState.Normal;
        activeTopic = visibleTopics[topicIndex];
        currentLineIndex = 0;

        if (!discussedTopics.Contains(activeTopic.topicName)) discussedTopics.Add(activeTopic.topicName);

        if (activeTopic.flagsToSet != null)
        {
            foreach (string f in activeTopic.flagsToSet) { if (!string.IsNullOrEmpty(f)) GameManager.Instance.AddStoryFlag(f); }
        }

        topicMenuPanel.SetActive(false);
        dialoguePanel.SetActive(true);

        EnsureMasterIsActiveAndFadedIn(DisplayCurrentLine);
    }

    public void ShowInnerMonologue(string text)
    {
        GameManager.Instance.isUIActive = true;
        if (interactionMasterGroup != null) interactionMasterGroup.interactable = false; // --- FREEZE THE MENU! ---

        preventImmediateAdvance = true;
        currentDialogueState = DialogueState.InnerMonologue;

        activeTopic = new Topic();
        activeTopic.lines = new DialogueLine[] { new DialogueLine { text = text } };
        currentLineIndex = 0;

        innerMonologuePanel.SetActive(true);
        DisplayCurrentLine();
    }

    public void ShowInnerMonologueSequence(string[] texts)
    {
        GameManager.Instance.isUIActive = true;
        if (interactionMasterGroup != null) interactionMasterGroup.interactable = false; // --- FREEZE THE MENU! ---

        preventImmediateAdvance = true;
        currentDialogueState = DialogueState.InnerMonologue;

        activeTopic = new Topic();
        activeTopic.lines = new DialogueLine[texts.Length];

        for (int i = 0; i < texts.Length; i++)
        {
            activeTopic.lines[i] = new DialogueLine { text = texts[i] };
        }
        currentLineIndex = 0;

        innerMonologuePanel.SetActive(true);
        DisplayCurrentLine();
    }

    public void StartGreeting(CustomerController customer, Topic greetingTopic)
    {
        preventImmediateAdvance = true;

        if (currentCustomer != customer)
        {
            discussedTopics.Clear();
            currentCustomer = customer;
        }

        GameManager.Instance.isUIActive = true;
        currentDialogueState = DialogueState.Greeting;
        activeTopic = greetingTopic;
        currentLineIndex = 0;

        actionMenuPanel.SetActive(false);
        topicMenuPanel.SetActive(false);
        dialoguePanel.SetActive(true);

        if (activeTopic.flagsToSet != null)
        {
            foreach (string f in activeTopic.flagsToSet) { if (!string.IsNullOrEmpty(f)) GameManager.Instance.AddStoryFlag(f); }
        }

        EnsureMasterIsActiveAndFadedIn(DisplayCurrentLine);
    }

    public void ShowReaction(Topic reactionTopic, bool endInteraction = true)
    {
        if (reactionTopic == null || reactionTopic.lines == null || reactionTopic.lines.Length == 0)
        {
            if (reactionTopic != null && reactionTopic.flagsToSet != null)
            {
                foreach (string f in reactionTopic.flagsToSet) { if (!string.IsNullOrEmpty(f)) GameManager.Instance.AddStoryFlag(f); }
            }
            GameManager.Instance.isUIActive = false;
            currentCustomer.FinishTransaction();
            return;
        }

        currentDialogueState = endInteraction ? DialogueState.ReactingAndLeaving : DialogueState.ReactingAndContinuing;
        preventImmediateAdvance = true;
        activeTopic = reactionTopic;
        currentLineIndex = 0;

        actionMenuPanel.SetActive(false);
        topicMenuPanel.SetActive(false);
        dialoguePanel.SetActive(true);

        if (activeTopic.flagsToSet != null)
        {
            foreach (string f in activeTopic.flagsToSet) { if (!string.IsNullOrEmpty(f)) GameManager.Instance.AddStoryFlag(f); }
        }

        EnsureMasterIsActiveAndFadedIn(DisplayCurrentLine);
    }

    private void AdvanceDialogue()
    {
        DialogueLine currentLine = activeTopic.lines[currentLineIndex];

        if (currentLine.bonusGold > 0)
        {
            isTransitioning = true;
            GameManager.Instance.AddGoldWithAnimation(currentLine.bonusGold, () =>
            {
                string charName = currentLine.speaker != null ? currentLine.speaker.characterName : "Bonus";
                GameManager.Instance.RecordTransaction(charName, "Mid-Convo Bonus", currentLine.bonusGold);

                isTransitioning = false;
                ProceedToNextLine();
            });
        }
        else ProceedToNextLine();
    }

    private void ProceedToNextLine()
    {
        if (currentDialogueState == DialogueState.Normal)
        {
            currentLineIndex++;
            if (currentLineIndex < activeTopic.lines.Length) DisplayCurrentLine();
            else
            {
                dialoguePanel.SetActive(false);
                OpenTopicMenu();
            }
        }
        else if (currentDialogueState == DialogueState.Greeting)
        {
            currentLineIndex++;
            if (currentLineIndex < activeTopic.lines.Length) DisplayCurrentLine();
            else
            {
                dialoguePanel.SetActive(false);
                OpenActionMenu(currentCustomer);
            }
        }
        else if (currentDialogueState == DialogueState.InnerMonologue)
        {
            currentLineIndex++;
            if (currentLineIndex < activeTopic.lines.Length) DisplayCurrentLine();
            else
            {
                innerMonologuePanel.SetActive(false);
                if (interactionMasterGroup != null) interactionMasterGroup.interactable = true; // --- UNFREEZE MENU! ---

                if (!actionMenuPanel.activeSelf && !topicMenuPanel.activeSelf && !dialoguePanel.activeSelf)
                {
                    GameManager.Instance.isUIActive = false;
                    CloseAllMenus();
                }
            }
        }
        else if (currentDialogueState == DialogueState.ReactingAndContinuing)
        {
            currentLineIndex++;
            if (currentLineIndex < activeTopic.lines.Length) DisplayCurrentLine();
            else
            {
                dialoguePanel.SetActive(false);
                OpenActionMenu(currentCustomer);
            }
        }
        else if (currentDialogueState == DialogueState.ReactingAndLeaving)
        {
            currentLineIndex++;
            if (currentLineIndex < activeTopic.lines.Length) DisplayCurrentLine();
            else
            {
                dialoguePanel.SetActive(false);
                StartCoroutine(FadeOutMaster(() =>
                {
                    if (currentCustomer.pendingGold > 0)
                    {
                        GameManager.Instance.AddGoldWithAnimation(currentCustomer.pendingGold, () =>
                        {
                            GameManager.Instance.isUIActive = false;
                            currentCustomer.FinishTransaction();
                        });
                    }
                    else
                    {
                        GameManager.Instance.isUIActive = false;
                        currentCustomer.FinishTransaction();
                    }
                }));
            }
        }
    }

    private void DisplayCurrentLine()
    {
        DialogueLine line = activeTopic.lines[currentLineIndex];

        if (currentDialogueState == DialogueState.InnerMonologue)
        {
            StartTypingLine(line.text, innerMonologueTextUI);
        }
        else
        {
            speakerNameTextUI.text = line.speaker != null ? line.speaker.characterName : "???";
            if (portraitImageUI != null)
            {
                if (line.speaker != null && line.speaker.defaultPortrait != null)
                {
                    portraitImageUI.sprite = line.speaker.defaultPortrait;
                    portraitImageUI.color = Color.white;
                }
                else portraitImageUI.color = Color.clear;
            }
            StartTypingLine(line.text, dialogueTextUI);
        }

        if (line.effect == ScreenEffect.Shake || line.effect == ScreenEffect.ShakeAndFlash) StartCoroutine(ShakeCamera(0.2f, 0.3f));
        if (line.effect == ScreenEffect.Flash || line.effect == ScreenEffect.ShakeAndFlash) StartCoroutine(ScreenFlash());
    }

    private void StartTypingLine(string text, TextMeshProUGUI targetTextUI)
    {
        currentFullLine = text;
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(text, targetTextUI));
    }

    private System.Collections.IEnumerator TypeText(string line, TextMeshProUGUI targetTextUI)
    {
        isTyping = true;
        targetTextUI.text = "";
        int visibleCharacterCount = 0;

        foreach (char letter in line.ToCharArray())
        {
            targetTextUI.text += letter;
            if (char.IsLetterOrDigit(letter))
            {
                visibleCharacterCount++;
                if (voiceAudioSource != null && voiceBlip != null && visibleCharacterCount % blipFrequency == 0)
                {
                    voiceAudioSource.pitch = Random.Range(0.95f, 1.05f);
                    voiceAudioSource.Stop();
                    voiceAudioSource.PlayOneShot(voiceBlip);
                }
            }

            if (letter == '.' || letter == '?' || letter == '!') yield return new WaitForSeconds(baseTypeSpeed * 8f);
            else if (letter == ',') yield return new WaitForSeconds(baseTypeSpeed * 4f);
            else yield return new WaitForSeconds(baseTypeSpeed);
        }
        isTyping = false;
    }

    private void SkipTyping()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        if (currentDialogueState == DialogueState.InnerMonologue) innerMonologueTextUI.text = currentFullLine;
        else dialogueTextUI.text = currentFullLine;

        isTyping = false;
    }

    private System.Collections.IEnumerator ShakeCamera(float duration, float magnitude)
    {
        Vector3 originalCamPos = mainCamera != null ? mainCamera.transform.localPosition : Vector3.zero;
        RectTransform uiRect = null;
        Vector3 originalUIPos = Vector3.zero;

        if (interactionMasterGroup != null)
        {
            uiRect = interactionMasterGroup.GetComponent<RectTransform>();
            originalUIPos = uiRect.anchoredPosition;
        }

        if (voiceAudioSource != null && slamSound != null) voiceAudioSource.PlayOneShot(slamSound);

        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            if (mainCamera != null) mainCamera.transform.localPosition = new Vector3(originalCamPos.x + x, originalCamPos.y + y, originalCamPos.z);
            if (uiRect != null) uiRect.anchoredPosition = new Vector2(originalUIPos.x + (x * 75f), originalUIPos.y + (y * 75f));

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (mainCamera != null) mainCamera.transform.localPosition = originalCamPos;
        if (uiRect != null) uiRect.anchoredPosition = originalUIPos;
    }

    private System.Collections.IEnumerator ScreenFlash()
    {
        if (flashPanel == null) yield break;
        flashPanel.alpha = 1f;
        float fadeSpeed = 4f;
        while (flashPanel.alpha > 0)
        {
            flashPanel.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
        flashPanel.alpha = 0f;
    }
}