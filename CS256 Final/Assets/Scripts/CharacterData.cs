using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Topic
{
    public string topicName;
    [TextArea(2, 4)] public string topicDialogue;
}

[CreateAssetMenu(fileName = "New Customer Data", menuName = "Tavern/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Customer Info")]
    public string characterName;

    [Header("Visuals & Animation")]
    public Sprite characterSprite;
    public Sprite dialoguePortrait;
    public RuntimeAnimatorController characterAnimator;

    // --- NEW: TIER 1 (MAX GOLD) ---
    [Tooltip("Specific drinks that give max gold.")]
    public List<Potion> perfectDrinks;

    [Tooltip("If set, ANY drink with this effect gives max gold!")]
    public MagicEffect perfectEffect;

    public int bounty;

    // --- NEW: TIER 2 (HALF GOLD) ---
    [Tooltip("Categories they will accept if they don't get a perfect match.")]
    public List<DrinkCategory> acceptableCategories;

    [Tooltip("A backup effect they will settle for (Half Gold).")]
    public MagicEffect acceptableEffect;

    [Header("Pre-Potion Dialogue")]
    public Topic[] availableTopics;

    [Header("Post-Potion Reactions")]
    [TextArea(2, 4)] public string reactionPerfect;
    [TextArea(2, 4)] public string reactionOkay;
    [TextArea(2, 4)] public string reactionFail;
}