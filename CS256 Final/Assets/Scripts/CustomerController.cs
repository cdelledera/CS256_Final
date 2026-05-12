using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustomerController : MonoBehaviour
{
    public enum CustomerState { WalkingIn, Waiting, WalkingOut }
    public CustomerState currentState = CustomerState.WalkingIn;
    [HideInInspector] public int pendingGold = 0;

    [Header("Data Profile")]
    public CharacterData myProfile;
    public PotionBrewing brewingSystem;

    [HideInInspector] public bool hasGreeted = false;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public Transform doorLocation;
    public Transform counterLocation;

    // --- Companion Reference ---
    private GameObject spawnedCompanion;

    void Start()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("CustomerSpawns");
        // Check if this character should have a companion walk in with them
        if (myProfile.companionPrefab != null)
        {
            SpawnCompanion();
        }
    }

    void SpawnCompanion()
    {
        // Spawn Gilby (or any companion) at the door with Lily
        spawnedCompanion = Instantiate(myProfile.companionPrefab, transform.position, Quaternion.identity);

        // Give the companion a "Follower" component or handle it here
        // We'll assume the companion prefab has a script that looks for a 'leader'
        CompanionFollower follower = spawnedCompanion.GetComponent<CompanionFollower>();
        if (follower != null)
        {
            follower.leader = this.transform;
            follower.followSpeed = this.moveSpeed;
        }
    }

    void Update()
    {
        if (currentState == CustomerState.WalkingIn)
        {
            transform.position = Vector2.MoveTowards(transform.position, counterLocation.position, moveSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, counterLocation.position) < 0.1f)
            {
                currentState = CustomerState.Waiting;
            }
        }
        else if (currentState == CustomerState.WalkingOut)
        {
            transform.position = Vector2.MoveTowards(transform.position, doorLocation.position, moveSpeed * Time.deltaTime);

            // If we have a companion, tell them to walk out too!
            if (spawnedCompanion != null)
            {
                CompanionFollower follower = spawnedCompanion.GetComponent<CompanionFollower>();
                if (follower != null) follower.isWalkingOut = true;
            }

            if (Vector2.Distance(transform.position, doorLocation.position) < 0.1f)
            {
                // Clean up the companion before destroying the leader
                if (spawnedCompanion != null) Destroy(spawnedCompanion);

                FindFirstObjectByType<CustomerSpawner>().OnCustomerLeft();
                Destroy(gameObject);
            }
        }
    }

    public void FinishTransaction()
    {
        currentState = CustomerState.WalkingOut;
    }

    void OnMouseDown()
    {
        // --- THE UI SHIELD ---
        // If the mouse is hovering over ANY UI element (like your Close button), ignore the click!
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (currentState != CustomerState.Waiting || GameManager.Instance.isUIActive) return;

        if (!hasGreeted)
        {
            Topic validGreeting = null;

            // --- 1. Check the new Conditional Greetings list first! ---
            if (myProfile.conditionalGreetings != null && myProfile.conditionalGreetings.Count > 0)
            {
                foreach (Topic t in myProfile.conditionalGreetings)
                {
                    bool dayMatch = (t.requiredDay == 0 || t.requiredDay == GameManager.Instance.currentDay);
                    bool flagMatch = string.IsNullOrEmpty(t.requiredStoryFlag) || GameManager.Instance.storyFlags.Contains(t.requiredStoryFlag);
                    bool excludeMatch = string.IsNullOrEmpty(t.excludedStoryFlag) || !GameManager.Instance.storyFlags.Contains(t.excludedStoryFlag);

                    if (dayMatch && flagMatch && excludeMatch)
                    {
                        validGreeting = t; // We found a match!
                        break;
                    }
                }
            }

            // --- 2. If no conditionals matched, use the old fallback greeting ---
            if (validGreeting == null && myProfile.greetingTopic != null && myProfile.greetingTopic.lines != null && myProfile.greetingTopic.lines.Length > 0)
            {
                validGreeting = myProfile.greetingTopic;
            }

            // --- 3. Play the greeting, or open the menu if they have nothing to say ---
            if (validGreeting != null && validGreeting.lines != null && validGreeting.lines.Length > 0)
            {
                hasGreeted = true;
                DialogueManager.Instance.StartGreeting(this, validGreeting);
            }
            else
            {
                DialogueManager.Instance.OpenActionMenu(this);
            }
        }
        else
        {
            DialogueManager.Instance.OpenActionMenu(this);
        }
    }

    public void ReceivePotion()
    {
        if (brewingSystem.readyToServePotion == Potion.None)
        {
            DialogueManager.Instance.ShowInnerMonologue("(I haven't brewed a potion to give them yet...)");
            return;
        }

        Potion givenPotion = brewingSystem.readyToServePotion;
        MagicEffect givenEffect = brewingSystem.readyToServeEffect;

        brewingSystem.readyToServePotion = Potion.None;
        brewingSystem.readyToServeEffect = MagicEffect.None;

        if (BrewingVisuals.Instance != null)
        {
            BrewingVisuals.Instance.ClearGlassVisuals();
        }

        string drinkName = PotionBrewing.GetDisplayName(givenPotion, givenEffect);

        if (myProfile.characterName == "Rachel" && GameManager.Instance.currentDay == 2)
        {
            if (givenEffect == MagicEffect.Speed)
            {
                GameManager.Instance.AddStoryFlag("Rachel_Got_Speed");
            }
        }

        List<DrinkFlavor> givenFlavors = new List<DrinkFlavor>(brewingSystem.readyToServeFlavors);

        bool foundMatch = false;

        foreach (ReactionBranch branch in myProfile.conditionalReactions)
        {
            // --- Check story flags before checking the drink! ---
            bool flagMatch = string.IsNullOrEmpty(branch.requiredStoryFlag) || GameManager.Instance.storyFlags.Contains(branch.requiredStoryFlag);
            bool excludeMatch = string.IsNullOrEmpty(branch.excludedStoryFlag) || !GameManager.Instance.storyFlags.Contains(branch.excludedStoryFlag);

            // If the player doesn't have the right story flags, skip this reaction entirely!
            if (!flagMatch || !excludeMatch) continue;

            // ==========================================
            // --- THE FIX: CATCH-ALL WILDCARDS ---
            // ==========================================
            bool requiresSpecificDrink = branch.requiredPotions.Count > 0 ||
                                         branch.requiredEffects.Count > 0 ||
                                         branch.requiredFlavors.Count > 0;

            bool potionMatches = branch.requiredPotions.Contains(givenPotion);
            bool effectMatches = branch.requiredEffects.Contains(givenEffect);

            bool flavorMatches = false;
            foreach (DrinkFlavor f in givenFlavors)
            {
                if (branch.requiredFlavors.Contains(f))
                {
                    flavorMatches = true;
                    break;
                }
            }

            // If it doesn't require a specific drink, it automatically passes the drink check!
            if (!requiresSpecificDrink || potionMatches || effectMatches || flavorMatches)
            {
                // --- CHANGED: Now passes the branch.endInteraction boolean! ---
                DialogueManager.Instance.ShowReaction(branch.reactionTopic, branch.endInteraction);
                pendingGold = branch.goldReward;

                GameManager.Instance.RecordTransaction(myProfile.characterName, drinkName, branch.goldReward);

                foundMatch = true;
                break;
            }
        }

        if (!foundMatch)
        {
            // --- CHANGED: The default fallback always ends the interaction! ---
            DialogueManager.Instance.ShowReaction(myProfile.defaultFailReaction, true);
            pendingGold = myProfile.failureGoldReward;

            GameManager.Instance.RecordTransaction(myProfile.characterName, drinkName, myProfile.failureGoldReward);
        }
    }
}