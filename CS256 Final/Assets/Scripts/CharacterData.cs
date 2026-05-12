using System.Collections.Generic;
using UnityEngine;

public enum ScreenEffect { None, Shake, Flash, ShakeAndFlash }

[System.Serializable]
public class DialogueLine
{
    public CharacterData speaker;
    [TextArea(2, 4)] public string text;

    // --- The effect dropdown for the Inspector! ---
    public ScreenEffect effect = ScreenEffect.None;

    // --- NEW: The mid-dialogue gold reward! ---
    [Tooltip("If greater than 0, the player gets this gold instantly when this line is spoken!")]
    public int bonusGold;
}

[System.Serializable]
public class ReactionBranch
{
    public string branchName = "New Reaction"; // Just to keep your Inspector organized!

    [Header("The Conditions (Leave lists empty to ignore)")]
    public List<Potion> requiredPotions = new List<Potion>();
    public List<MagicEffect> requiredEffects = new List<MagicEffect>();
    public List<DrinkFlavor> requiredFlavors = new List<DrinkFlavor>();

    // --- NEW: Story Flags for Reactions! ---
    [Header("Story Conditions")]
    [Tooltip("Leave empty if no flag is needed. Otherwise, type a flag like 'Rachel_Day2_Fail'")]
    public string requiredStoryFlag;

    [Tooltip("Type a flag here to skip this reaction if the player has the flag!")]
    public string excludedStoryFlag;

    [Header("The Outcome")]
    public int goldReward;
    public Topic reactionTopic;

    [Tooltip("If FALSE, the customer stays at the bar so you can serve them ANOTHER drink!")]
    public bool endInteraction = true;
}

[System.Serializable]
public class Topic
{
    public string topicName;

    // --- THE STORY LOCKS ---
    [Tooltip("Leave at 0 to show on ANY day. Set to 1 for Day 1, 5 for Day 5, etc.")]
    public int requiredDay = 0;

    [Tooltip("Leave empty if no flag is needed. Otherwise, type a flag like 'Joe_Got_Pink'")]
    public string requiredStoryFlag;

    [Tooltip("Type a flag here to HIDE this topic if the player has the flag!")]
    public string excludedStoryFlag;

    [Tooltip("Leave empty to show immediately. Type another topic's exact name here to lock this until that one is read!")]
    public string requiredPreviousTopic;

    [Tooltip("If you type a word here, the game will permanently remember it after reading this topic!")]
    public string flagToSet;

    public DialogueLine[] lines;
}

[CreateAssetMenu(fileName = "New Customer Data", menuName = "Tavern/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Customer Info")]
    public string characterName;

    public Sprite defaultPortrait;

    [Tooltip("If this is filled, another character will walk in with them!")]
    public CharacterData companion;

    [Header("Pre-Potion Dialogue")]
    [Tooltip("The fallback dialogue that plays automatically if no conditionals match.")]
    public Topic greetingTopic;

    // --- NEW: Conditional Greetings List ---
    [Tooltip("The game checks these top-to-bottom. It plays the first one with matching flags!")]
    public List<Topic> conditionalGreetings = new List<Topic>();

    [Header("Available Topics")]
    public Topic[] availableTopics;

    [Header("Post-Potion Reactions")]
    [Tooltip("The game checks these from top to bottom. The first one that matches wins!")]
    public List<ReactionBranch> conditionalReactions = new List<ReactionBranch>();

    [Header("Failure Outcomes")]
    public Topic defaultFailReaction;
    [Tooltip("How much gold they give even if you fail the recipe (Consolation prize).")]
    public int failureGoldReward = 0;

    [Header("Companions")]
    public GameObject companionPrefab;
}