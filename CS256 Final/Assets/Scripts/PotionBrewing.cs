using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class PotionBrewing : MonoBehaviour
{
    public List<IngredientData> currentIngredients = new List<IngredientData>();

    public Potion readyToServePotion = Potion.None;
    public MagicEffect readyToServeEffect = MagicEffect.None;
    public List<DrinkFlavor> readyToServeFlavors = new List<DrinkFlavor>();

    public bool TryAddIngredient(IngredientData item)
    {
        // 1. RESTRICTION: No repeats allowed!
        if (currentIngredients.Contains(item))
        {
            Debug.Log("Ingredient is already in the pot!");
            return false;
        }

        bool isMagic = item.effect != MagicEffect.None;

        if (isMagic)
        {
            // 2. RESTRICTION: Check if we already have a magic item (Slot 3 full)
            int magicCount = 0;
            foreach (var ing in currentIngredients)
            {
                if (ing.effect != MagicEffect.None) magicCount++;
            }

            if (magicCount >= 1)
            {
                Debug.Log("Magic slot is already full!");
                return false;
            }
        }
        else
        {
            // 3. RESTRICTION: Check if base slots are full (Slots 1 & 2 full)
            int baseCount = 0;
            foreach (var ing in currentIngredients)
            {
                if (ing.effect == MagicEffect.None) baseCount++;
            }

            if (baseCount >= 2)
            {
                Debug.Log("Base ingredient slots are already full!");
                return false;
            }
        }

        // If it passed all tests, add it!
        currentIngredients.Add(item);
        return true;
    }

    public Potion Brew()
    {
        if (currentIngredients.Count == 0) return readyToServePotion = Potion.None;

        if (BrewingVisuals.Instance != null) BrewingVisuals.Instance.ClearGlassVisuals();

        int magicCount = 0;
        readyToServeEffect = MagicEffect.None;
        readyToServeFlavors.Clear();

        // --- We separate the base ingredients from the magic one! ---
        List<Ingredient> recipeBaseItems = new List<Ingredient>();

        foreach (IngredientData item in currentIngredients)
        {
            // Extract Magic & Flavors
            if (item.effect != MagicEffect.None)
            {
                magicCount++;
                readyToServeEffect = item.effect;
            }
            else
            {
                // Only pure base ingredients make it into this list!
                recipeBaseItems.Add(item.ingredientType);
            }

            if (item.flavor != DrinkFlavor.None && !readyToServeFlavors.Contains(item.flavor))
            {
                readyToServeFlavors.Add(item.flavor);
            }
        }

        // Failsafe 
        if (magicCount > 1)
        {
            currentIngredients.Clear();
            readyToServeEffect = MagicEffect.None;
            readyToServeFlavors.Clear();
            return readyToServePotion = Potion.Slop;
        }

        Potion brewed = Potion.Slop;

        // =============================================================
        // --- CLEANED UP RECIPE LOGIC (No magic items allowed here) ---
        // =============================================================

        // --- SINGLE INGREDIENT RECIPES ---
        if (recipeBaseItems.Count == 1)
        {
            Ingredient i = recipeBaseItems[0];

            // Just check the pure base items now!
            if (i == Ingredient.Lemon) brewed = Potion.Lemonade;
            else if (i == Ingredient.Apple) brewed = Potion.AppleJuice;
            else if (i == Ingredient.Pepper) brewed = Potion.HotSauce;
            else if (i == Ingredient.DragonFruit) brewed = Potion.DragonJuice;
            else if (i == Ingredient.Cherry) brewed = Potion.CherryJuice;
            else if (i == Ingredient.Pineapple) brewed = Potion.PineappleJuice;
        }
        // --- TWO INGREDIENT MIXES ---
        else if (recipeBaseItems.Count == 2)
        {
            // No more "HasGroup"! We just check if the pure list contains the pure base item.
            bool lemon = recipeBaseItems.Contains(Ingredient.Lemon);
            bool dragon = recipeBaseItems.Contains(Ingredient.DragonFruit);
            bool apple = recipeBaseItems.Contains(Ingredient.Apple);
            bool cherry = recipeBaseItems.Contains(Ingredient.Cherry);
            bool pepper = recipeBaseItems.Contains(Ingredient.Pepper);
            bool pineapple = recipeBaseItems.Contains(Ingredient.Pineapple);
            bool watermelon = recipeBaseItems.Contains(Ingredient.Watermelon);
            bool starfruit = recipeBaseItems.Contains(Ingredient.StarFruit);

            int uniqueFlavors = (lemon ? 1 : 0) + (dragon ? 1 : 0) + (apple ? 1 : 0) +
                                (cherry ? 1 : 0) + (pepper ? 1 : 0) + (pineapple ? 1 : 0) +
                                (watermelon ? 1 : 0) + (starfruit ? 1 : 0);

            if (uniqueFlavors < 2)
            {
                currentIngredients.Clear();
                readyToServeEffect = MagicEffect.None;
                readyToServeFlavors.Clear();
                return readyToServePotion = Potion.Slop;
            }

            // --- THE PURE MODIFIERS (Watermelon & Starfruit) ---
            if (watermelon || starfruit)
            {
                if (lemon) brewed = Potion.Lemonade;
                else if (apple) brewed = Potion.AppleJuice;
                else if (pepper) brewed = Potion.HotSauce;
                else if (dragon) brewed = Potion.DragonJuice;
                else if (cherry) brewed = Potion.CherryJuice;
                else if (pineapple) brewed = Potion.PineappleJuice;
            }
            // --- THE STANDARD MIXES ---
            else
            {
                if (lemon && dragon) brewed = Potion.PinkLemonade;
                else if (lemon && pepper) brewed = Potion.HellInAGlass;
                else if (apple && pepper) brewed = Potion.SweetAndSpicy;
                else if (pineapple && apple) brewed = Potion.PPAP;
                else if (apple && cherry) brewed = Potion.AutumnChapple;
                else if (dragon && pineapple) brewed = Potion.DragonKing;
                else if (lemon && apple) brewed = Potion.SourApple;
                else if (lemon && cherry) brewed = Potion.CherryLemonade;
                else if (lemon && pineapple) brewed = Potion.TropicalCitrus;
                else if (apple && dragon) brewed = Potion.DragonbiteCider;
                else if (cherry && dragon) brewed = Potion.CrimsonWyrm;
                else if (cherry && pepper) brewed = Potion.FirecrackerMash;
                else if (cherry && pineapple) brewed = Potion.SunsetJuice;
                else if (dragon && pepper) brewed = Potion.DragonblazeSmoothie;
                else if (pineapple && pepper) brewed = Potion.VolcanoJuice;
            }
        }

        currentIngredients.Clear();
        readyToServePotion = brewed;

        // --- NEW: Also wipe the magic effect if they made Slop! ---
        if (readyToServePotion == Potion.Slop)
        {
            readyToServeFlavors.Clear();
            readyToServeEffect = MagicEffect.None;
        }

        return readyToServePotion;
    }

    public static string GetDisplayName(Potion potion, MagicEffect effect)
    {
        if (potion == Potion.None) return "Nothing";
        if (potion == Potion.Slop) return "Slop";

        string baseName = Regex.Replace(potion.ToString(), "([a-z])([A-Z])", "$1 $2");

        if (effect != MagicEffect.None)
        {
            return baseName + " (" + effect.ToString() + ")";
        }

        return baseName;
    }
}