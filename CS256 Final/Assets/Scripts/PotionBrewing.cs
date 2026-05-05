using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public enum DrinkCategory
{
    None,
    Juice,
    Lemonade,
    Cider,
    Wine,
    Mash,
    Sludge
}

public class PotionBrewing : MonoBehaviour
{
    public List<IngredientData> currentIngredients = new List<IngredientData>();

    public Potion readyToServePotion = Potion.None;
    // NEW: We now track the effect directly in the cauldron based on the fruit used!
    public MagicEffect readyToServeEffect = MagicEffect.None;

    public void AddIngredient(IngredientData item) { currentIngredients.Add(item); }

    public Potion Brew()
    {
        if (currentIngredients.Count == 0) return readyToServePotion = Potion.None;

        // 1. EXTRACT THE MAGIC EFFECT
        // Look at every fruit in the pot. If one has magic, the final drink gets that magic!
        readyToServeEffect = MagicEffect.None;
        foreach (IngredientData item in currentIngredients)
        {
            if (item.effect != MagicEffect.None)
            {
                readyToServeEffect = item.effect;
            }
        }

        // 2. EXTRACT THE INGREDIENT NAMES 
        // This grabs just the name of the fruit (e.g., "Apple") so checking recipes is easier
        List<Ingredient> items = new List<Ingredient>();
        foreach (IngredientData data in currentIngredients)
        {
            items.Add(data.ingredientType); // Ensure your IngredientData script has a variable called ingredientType!
        }

        Potion brewed = Potion.Slop;

        // --- SINGLE INGREDIENT RECIPES ---
        if (items.Count == 1)
        {
            Ingredient i = items[0];
            if (i == Ingredient.Lemon || i == Ingredient.PurpleLemon || i == Ingredient.HeartLemon) brewed = Potion.Lemonade;
            else if (i == Ingredient.Apple || i == Ingredient.GoldenApple) brewed = Potion.AppleJuice;
            else if (i == Ingredient.Pepper) brewed = Potion.HotSauce;
            else if (i == Ingredient.DragonFruit || i == Ingredient.MagicDragonFruit) brewed = Potion.DragonJuice;
            else if (i == Ingredient.Cherry || i == Ingredient.IceCherries) brewed = Potion.CherryJuice;
            else if (i == Ingredient.Pineapple) brewed = Potion.PineappleJuice;
        }
        // --- TWO INGREDIENT MIXES ---
        else if (items.Count == 2)
        {
            // Grouping the regular fruit with their magical variants so either works for the recipe!
            bool lemon = HasGroup(items, Ingredient.Lemon, Ingredient.PurpleLemon, Ingredient.HeartLemon);
            bool dragon = HasGroup(items, Ingredient.DragonFruit, Ingredient.MagicDragonFruit, Ingredient.None);
            bool apple = HasGroup(items, Ingredient.Apple, Ingredient.GoldenApple, Ingredient.None);
            bool cherry = HasGroup(items, Ingredient.Cherry, Ingredient.IceCherries, Ingredient.None);

            bool pepper = items.Contains(Ingredient.Pepper);
            bool pineapple = items.Contains(Ingredient.Pineapple);

            // Checking the exact combos from your Document
            if (lemon && dragon) brewed = Potion.PinkLemonade;
            else if (lemon && pepper) brewed = Potion.HellInAGlass;
            else if (apple && pepper) brewed = Potion.SweetAndSpicy;
            else if (pineapple && apple) brewed = Potion.PPAP;

            // The two secret drinks from Rachel's dialogue (I assigned them logic so they work!)
            else if (apple && cherry) brewed = Potion.AutumnChapple;
            else if (dragon && pineapple) brewed = Potion.DragonKing;
        }

        // Empty the pot and serve!
        currentIngredients.Clear();
        readyToServePotion = brewed;
        return readyToServePotion;
    }

    // Helper function to check if the pot has either the normal OR magical version of a fruit
    private bool HasGroup(List<Ingredient> items, Ingredient normal, Ingredient special1, Ingredient special2)
    {
        return items.Contains(normal) || items.Contains(special1) || items.Contains(special2);
    }

    // --- DRINK CATEGORIES UPDATED ---
    public static DrinkCategory GetCategory(Potion potion)
    {
        switch (potion)
        {
            case Potion.Lemonade:
            case Potion.PinkLemonade:
            case Potion.HellInAGlass:
                return DrinkCategory.Lemonade;

            case Potion.AutumnChapple:
            case Potion.SweetAndSpicy:
                return DrinkCategory.Cider;

            case Potion.DragonKing:
                return DrinkCategory.Wine;

            case Potion.AppleJuice:
            case Potion.DragonJuice:
            case Potion.CherryJuice:
            case Potion.PineappleJuice:
                return DrinkCategory.Juice;

            case Potion.PPAP:
            case Potion.HotSauce:
                return DrinkCategory.Mash;

            default:
                return DrinkCategory.Sludge;
        }
    }

    public static string GetDisplayName(Potion potion)
    {
        if (potion == Potion.None) return "Nothing";
        return Regex.Replace(potion.ToString(), "([a-z])([A-Z])", "$1 $2");
    }

    // NOTE: GetPotionEffect() was deleted! We don't need it because the Cauldron tracks the effect now.
}