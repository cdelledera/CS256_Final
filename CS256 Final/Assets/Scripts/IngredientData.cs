using UnityEngine;

[CreateAssetMenu(fileName = "New Ingredient", menuName = "Tavern/Ingredient Data")]
public class IngredientData : ScriptableObject
{
    [Header("Basic Info")]
    public string ingredientName;
    [TextArea(2, 4)] public string description;
    public Sprite icon;

    [Header("Brewing Attributes")]
    [Tooltip("The exact item from your enum list (e.g., Apple, IceCherries).")]
    public Ingredient ingredientType;

    [Tooltip("The base flavor of this fruit.")]
    public DrinkFlavor flavor;

    [Tooltip("Leave as 'None' for regular fruit!")]
    public MagicEffect effect;
}