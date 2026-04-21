using UnityEngine;

// 1. The Classifications
public enum Flavor { None, Sweet, Sour, Tart, Spicy }
public enum ItemType { BaseFruit, SpecialModifier }
public enum MagicEffect { None, Frost, Endurance, Strength }


[CreateAssetMenu(fileName = "New Ingredient", menuName = "Tavern/Ingredient")]
public class IngredientData : ScriptableObject
{
    [Header("Basic Info")]
    public string ingredientName;
    [TextArea(2, 4)] public string description;
    public Sprite icon; 
    [Header("Attributes")]
    public ItemType type;
    public Flavor flavor;
    public MagicEffect effect;
}