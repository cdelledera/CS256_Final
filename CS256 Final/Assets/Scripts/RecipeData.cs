using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Brewing/Recipe Data")]
public class RecipeData : ScriptableObject
{
    public string potionName;
    public Sprite potionSprite; // The picture of the crafted drink!

    [TextArea(3, 5)]
    public string recipeDescription; // The "Lemon + Dragon Fruit" text
}