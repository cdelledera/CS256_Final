using UnityEngine;
using TMPro;
using UnityEngine.UI; 

public class CustomerController : MonoBehaviour
{
    public enum CustomerState { WalkingIn, Waiting, WalkingOut }
    public CustomerState currentState = CustomerState.WalkingIn;

    [Header("Data Profile")]
    public CharacterData myProfile;
    public PotionBrewing brewingSystem;

    [Header("Visuals & Animation")]
    public SpriteRenderer mySpriteRenderer;
    public Animator myAnimator; 

    [Header("Movement")]
    public float moveSpeed = 3f;
    public Transform doorLocation;
    public Transform counterLocation;

    [Header("UI References")]
    public Image dialoguePortraitUI; 
    public GameObject topicMenuPanel;
    public TextMeshProUGUI[] topicButtonTexts;
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueTextUI;

    private bool isReacting = false;

    void Start()
    {
        // 1. Swap the physical body
        if (myProfile.characterSprite != null && mySpriteRenderer != null)
            mySpriteRenderer.sprite = myProfile.characterSprite;

        // 2. Swap the Stardew Portrait
        if (myProfile.dialoguePortrait != null && dialoguePortraitUI != null)
            dialoguePortraitUI.sprite = myProfile.dialoguePortrait;

        // 3. Swap the Animation file (so Joe walks differently than Lily)
        if (myProfile.characterAnimator != null && myAnimator != null)
            myAnimator.runtimeAnimatorController = myProfile.characterAnimator;

        topicMenuPanel.SetActive(false);
        dialoguePanel.SetActive(false);

     
        for (int i = 0; i < topicButtonTexts.Length; i++)
        {
            if (i < myProfile.availableTopics.Length)
            {
                topicButtonTexts[i].text = myProfile.availableTopics[i].topicName;
                topicButtonTexts[i].transform.parent.gameObject.SetActive(true);
            }
            else
            {
                topicButtonTexts[i].transform.parent.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (currentState == CustomerState.WalkingIn)
        {
            // NEW: Tell the animator to start the walking loop!
            if (myAnimator != null) myAnimator.SetBool("isWalking", true);

            transform.position = Vector2.MoveTowards(transform.position, counterLocation.position, moveSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, counterLocation.position) < 0.1f)
            {
                currentState = CustomerState.Waiting;

                // NEW: Tell the animator to stop walking and stand idle!
                if (myAnimator != null) myAnimator.SetBool("isWalking", false);

                topicMenuPanel.SetActive(true);
            }
        }
        else if (currentState == CustomerState.WalkingOut)
        {
            // NEW: Start walking again!
            if (myAnimator != null) myAnimator.SetBool("isWalking", true);

            transform.position = Vector2.MoveTowards(transform.position, doorLocation.position, moveSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, doorLocation.position) < 0.1f)
            {
                GameManager.Instance.AdvanceTime();
                Destroy(gameObject);
            }
        }

        // ... (Keep the rest of Update and the other functions exactly the same!)
        if (dialoguePanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
            {
                AdvanceDialogue();
            }
        }
    }

    private void AdvanceDialogue()
    {
        if (!isReacting)
        {
            dialoguePanel.SetActive(false);
            topicMenuPanel.SetActive(true);
            GameManager.Instance.isUIActive = false;
        }
        else
        {
            GameManager.Instance.isUIActive = false;
            dialoguePanel.SetActive(false);
            currentState = CustomerState.WalkingOut;
        }
    }

    public void OnTopicClicked(int topicIndex)
    {
        if (currentState != CustomerState.Waiting) return;

        topicMenuPanel.SetActive(false);
        dialoguePanel.SetActive(true);
        dialogueTextUI.text = myProfile.availableTopics[topicIndex].topicDialogue;
        GameManager.Instance.isUIActive = true;
    }

    void OnMouseDown()
    {
        if (currentState != CustomerState.Waiting) return;

        if (topicMenuPanel.activeSelf && brewingSystem.readyToServePotion != Potion.None)
        {
            Potion givenPotion = brewingSystem.readyToServePotion;
            brewingSystem.readyToServePotion = Potion.None;

            DrinkCategory givenCategory = PotionBrewing.GetCategory(givenPotion);
            MagicEffect givenEffect = PotionBrewing.GetPotionEffect(givenPotion);

            isReacting = true;
            topicMenuPanel.SetActive(false);
            dialoguePanel.SetActive(true);
            GameManager.Instance.isUIActive = true;

            // TIER 1: PERFECT MATCH (Check the List OR check the Effect!)
            if (myProfile.perfectDrinks.Contains(givenPotion) ||
               (myProfile.perfectEffect != MagicEffect.None && givenEffect == myProfile.perfectEffect))
            {
                dialogueTextUI.text = myProfile.reactionPerfect;
                GameManager.Instance.LogQuestResult(myProfile.bounty); // Full Gold
            }
            // TIER 2: OKAY MATCH (Check Category OR Backup Effect)
            else if (myProfile.acceptableCategories.Contains(givenCategory) ||
                    (myProfile.acceptableEffect != MagicEffect.None && givenEffect == myProfile.acceptableEffect))
            {
                dialogueTextUI.text = myProfile.reactionOkay;
                GameManager.Instance.LogQuestResult(myProfile.bounty / 2); // Half Gold
            }
            // TIER 3: FAIL
            else
            {
                dialogueTextUI.text = myProfile.reactionFail;
                GameManager.Instance.LogQuestResult(0); // 0 Gold
            }
        }
    }
}
