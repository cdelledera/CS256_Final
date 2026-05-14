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

    private GameObject spawnedCompanion;

    void Start()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("CustomerSpawns");

        SpriteRenderer myRenderer = GetComponent<SpriteRenderer>();
        if (myRenderer != null && myProfile != null && myProfile.worldSprite != null)
        {
            myRenderer.sprite = myProfile.worldSprite;
        }

        if (myProfile.companionPrefab != null) SpawnCompanion();
    }

    void SpawnCompanion()
    {
        spawnedCompanion = Instantiate(myProfile.companionPrefab, transform.position, Quaternion.identity);
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
            if (Vector2.Distance(transform.position, counterLocation.position) < 0.1f) currentState = CustomerState.Waiting;
        }
        else if (currentState == CustomerState.WalkingOut)
        {
            transform.position = Vector2.MoveTowards(transform.position, doorLocation.position, moveSpeed * Time.deltaTime);

            if (spawnedCompanion != null)
            {
                CompanionFollower follower = spawnedCompanion.GetComponent<CompanionFollower>();
                if (follower != null) follower.isWalkingOut = true;
            }

            if (Vector2.Distance(transform.position, doorLocation.position) < 0.1f)
            {
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
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (currentState != CustomerState.Waiting || GameManager.Instance.isUIActive) return;

        if (!hasGreeted)
        {
            Topic validGreeting = null;

            if (myProfile.conditionalGreetings != null && myProfile.conditionalGreetings.Count > 0)
            {
                foreach (Topic t in myProfile.conditionalGreetings)
                {
                    bool dayMatch = (t.requiredDay == 0 || t.requiredDay == GameManager.Instance.currentDay);

                    // --- CHANGED: List Logic ---
                    bool flagMatch = true;
                    foreach (string req in t.requiredStoryFlags) { if (!string.IsNullOrEmpty(req) && !GameManager.Instance.storyFlags.Contains(req)) flagMatch = false; }

                    bool excludeMatch = true;
                    foreach (string exc in t.excludedStoryFlags) { if (!string.IsNullOrEmpty(exc) && GameManager.Instance.storyFlags.Contains(exc)) excludeMatch = false; }

                    if (dayMatch && flagMatch && excludeMatch)
                    {
                        validGreeting = t;
                        break;
                    }
                }
            }

            if (validGreeting == null && myProfile.greetingTopic != null && myProfile.greetingTopic.lines != null && myProfile.greetingTopic.lines.Length > 0)
            {
                validGreeting = myProfile.greetingTopic;
            }

            if (validGreeting != null && validGreeting.lines != null && validGreeting.lines.Length > 0)
            {
                hasGreeted = true;
                DialogueManager.Instance.StartGreeting(this, validGreeting);
            }
            else DialogueManager.Instance.OpenActionMenu(this);
        }
        else DialogueManager.Instance.OpenActionMenu(this);
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

        if (BrewingVisuals.Instance != null) BrewingVisuals.Instance.ClearGlassVisuals();

        string drinkName = PotionBrewing.GetDisplayName(givenPotion, givenEffect);

        // =======================================================
        // --- HARDCODED STORY MILESTONES ---
        // =======================================================
        if (myProfile.characterName == "Rachel" && GameManager.Instance.currentDay == 2)
        {
            // Speed check
            if (givenEffect == MagicEffect.Speed) GameManager.Instance.AddStoryFlag("Rachel_Got_Speed");

            // Drink accuracy check
            if (givenPotion == Potion.DragonKing)
            {
                GameManager.Instance.AddStoryFlag("Rachel_Day2_Correct");
                Debug.Log("System: Rachel got Day 2 correct!");
            }
        }

        if (myProfile.characterName == "Rachel" && GameManager.Instance.currentDay == 3)
        {
            // Charisma Check
            if (givenEffect == MagicEffect.Charisma)
            {
                //  Only give the Charisma flag IF she is already on the Speed route
                if (GameManager.Instance.storyFlags.Contains("Rachel_Got_Speed"))
                {
                    GameManager.Instance.AddStoryFlag("Rachel_Charisma");
                    Debug.Log("System: Rachel is on the Speed Route and drank Charisma!");
                }
            }

        }

        if (myProfile.characterName == "Joe" && GameManager.Instance.currentDay == 5)
        {
            if (givenEffect == MagicEffect.Speed)
            {
                GameManager.Instance.AddStoryFlag("Joe_Got_Speed");
                Debug.Log("System: Joe drank Speed! Flag set automatically.");
            }
        }
        // =======================================================

        List<DrinkFlavor> givenFlavors = new List<DrinkFlavor>(brewingSystem.readyToServeFlavors);
        bool foundMatch = false;

        foreach (ReactionBranch branch in myProfile.conditionalReactions)
        {
            // --- CHANGED: List Logic ---
            bool flagMatch = true;
            foreach (string req in branch.requiredStoryFlags) { if (!string.IsNullOrEmpty(req) && !GameManager.Instance.storyFlags.Contains(req)) flagMatch = false; }

            bool excludeMatch = true;
            foreach (string exc in branch.excludedStoryFlags) { if (!string.IsNullOrEmpty(exc) && GameManager.Instance.storyFlags.Contains(exc)) excludeMatch = false; }

            if (!flagMatch || !excludeMatch) continue;

            bool requiresSpecificDrink = branch.requiredPotions.Count > 0 || branch.requiredEffects.Count > 0 || branch.requiredFlavors.Count > 0;
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

            if (!requiresSpecificDrink || potionMatches || effectMatches || flavorMatches)
            {
                DialogueManager.Instance.ShowReaction(branch.reactionTopic, branch.endInteraction);
                pendingGold = branch.goldReward;
                GameManager.Instance.RecordTransaction(myProfile.characterName, drinkName, branch.goldReward);
                foundMatch = true;
                break;
            }
        }

        if (!foundMatch)
        {
            DialogueManager.Instance.ShowReaction(myProfile.defaultFailReaction, true);
            pendingGold = myProfile.failureGoldReward;
            GameManager.Instance.RecordTransaction(myProfile.characterName, drinkName, myProfile.failureGoldReward);
        }
    }
}