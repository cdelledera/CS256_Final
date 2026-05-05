using UnityEngine;

public class CustomerController : MonoBehaviour
{
    public enum CustomerState { WalkingIn, Waiting, WalkingOut }
    public CustomerState currentState = CustomerState.WalkingIn;

    [Header("Data Profile")]
    public CharacterData myProfile;
    public PotionBrewing brewingSystem;

    // NEW: Tracks if the customer has said their opening line yet
    [HideInInspector] public bool hasGreeted = false;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public Transform doorLocation;
    public Transform counterLocation;

    void Update()
    {
        if (currentState == CustomerState.WalkingIn)
        {
            transform.position = Vector2.MoveTowards(transform.position, counterLocation.position, moveSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, counterLocation.position) < 0.1f)
            {
                currentState = CustomerState.Waiting;
                // CHANGED: They now just stand there and wait for you to click them!
            }
        }
        else if (currentState == CustomerState.WalkingOut)
        {
            transform.position = Vector2.MoveTowards(transform.position, doorLocation.position, moveSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, doorLocation.position) < 0.1f)
            {
                GameManager.Instance.AdvanceTime();
                Destroy(gameObject);
            }
        }
    }

    public void FinishTransaction()
    {
        currentState = CustomerState.WalkingOut;
    }

    // UPDATED: Now checks for a greeting before opening the Action Menu!
    void OnMouseDown()
    {
        if (currentState != CustomerState.Waiting || GameManager.Instance.isUIActive) return;

        // If they have a greeting written AND they haven't said it yet...
        if (!hasGreeted && myProfile.greetingTopic != null && myProfile.greetingTopic.lines != null && myProfile.greetingTopic.lines.Length > 0)
        {
            hasGreeted = true;
            DialogueManager.Instance.StartGreeting(this, myProfile.greetingTopic);
        }
        else
        {
            // Otherwise, just open the regular Action Menu
            DialogueManager.Instance.OpenActionMenu(this);
        }
    }

    // NEW: The manager calls this when you specifically click "Present"
    public void ReceivePotion()
    {
        // Ace Attorney Check: Do we actually have an item to present?
        if (brewingSystem.readyToServePotion == Potion.None)
        {
            DialogueManager.Instance.ShowInnerMonologue("(I haven't brewed a potion to give them yet...)");
            return;
        }

        // 1. Grab the drink and the effect directly from the cauldron's memory!
        Potion givenPotion = brewingSystem.readyToServePotion;
        MagicEffect givenEffect = brewingSystem.readyToServeEffect;

        // 2. Empty the cauldron!
        brewingSystem.readyToServePotion = Potion.None;
        brewingSystem.readyToServeEffect = MagicEffect.None;

        // 3. Get the category (like Juice or Lemonade)
        DrinkCategory givenCategory = PotionBrewing.GetCategory(givenPotion);

        // --- NEW EVALUATION LOGIC ---
        bool foundMatch = false;

        // Check the list of reactions from top to bottom
        foreach (ReactionBranch branch in myProfile.conditionalReactions)
        {
            // NEW LOGIC: Check if the drink they brewed exists inside ANY of the lists for this branch!
            bool potionMatches = branch.requiredPotions.Contains(givenPotion);
            bool effectMatches = branch.requiredEffects.Contains(givenEffect);
            bool categoryMatches = branch.requiredCategories.Contains(givenCategory);

            // If ANY of those conditions are met, trigger this branch!
            if (potionMatches || effectMatches || categoryMatches)
            {
                DialogueManager.Instance.ShowReaction(branch.reactionTopic);
                GameManager.Instance.LogQuestResult(branch.goldReward);
                foundMatch = true;
                break; // Stop looking! We found the right reaction.
            }
        }

        // If we checked the whole list and nothing matched, play the fail state
        if (!foundMatch)
        {
            DialogueManager.Instance.ShowReaction(myProfile.defaultFailReaction);
            GameManager.Instance.LogQuestResult(0); // 0 Gold for failing
        }
    }
}