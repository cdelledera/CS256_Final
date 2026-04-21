using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public enum DrinkCategory
{
    Juice,
    Lemonade,
    Cider,
    Wine,
    Mash,
    Sludge
}

public class PotionBrewing : MonoBehaviour
{
    // 1. CHANGED: Now the pot holds the data files, not the old enum!
    public List<IngredientData> currentIngredients = new List<IngredientData>();
    public Potion readyToServePotion = Potion.None;

    public void AddIngredient(IngredientData item) { currentIngredients.Add(item); }

    public Potion Brew()
    {
        // 2. THE NEW MATH: We count the attributes instead of the names!
        int sweetBases = CountFlavor(Flavor.Sweet);
        int sourBases = CountFlavor(Flavor.Sour);
        int tartBases = CountFlavor(Flavor.Tart);

        int frostSpecials = CountEffect(MagicEffect.Frost);
        int enduranceSpecials = CountEffect(MagicEffect.Endurance);
        int strengthSpecials = CountEffect(MagicEffect.Strength);

        int totalBases = sweetBases + sourBases + tartBases;
        int totalSpecials = frostSpecials + enduranceSpecials + strengthSpecials;

        currentIngredients.Clear();

        if (totalBases == 0 && totalSpecials > 0) return readyToServePotion = Potion.ToxicSludge;

        if (totalSpecials == 0)
        {
            if (sweetBases > 0 && sourBases > 0) return readyToServePotion = Potion.AppleLemonade;
            if (sweetBases > 0 && tartBases > 0) return readyToServePotion = Potion.OrchardBlend;
            if (sourBases > 0 && tartBases > 0) return readyToServePotion = Potion.CitrusCrush;

            if (sweetBases > 0) return readyToServePotion = Potion.AppleJuice;
            if (sourBases > 0) return readyToServePotion = Potion.SourLemonade;
            if (tartBases > 0) return readyToServePotion = Potion.GrapeJuice;
        }
        else if (sweetBases > 0 && sourBases == 0 && tartBases == 0)
        {
            if (frostSpecials > 0) return readyToServePotion = Potion.ChilledCider;
            if (enduranceSpecials > 0) return readyToServePotion = Potion.SunsetCider;
            if (strengthSpecials > 0) return readyToServePotion = Potion.DragonbreathCider;
        }
        else if (sourBases > 0 && sweetBases == 0 && tartBases == 0)
        {
            if (frostSpecials > 0) return readyToServePotion = Potion.FrostbiteLemonade;
            if (enduranceSpecials > 0) return readyToServePotion = Potion.BloodLemonade;
            if (strengthSpecials > 0) return readyToServePotion = Potion.SpicyLemonade;
        }
        else if (tartBases > 0 && sweetBases == 0 && sourBases == 0)
        {
            if (frostSpecials > 0) return readyToServePotion = Potion.GlacierCrush;
            if (enduranceSpecials > 0) return readyToServePotion = Potion.CrimsonSangria;
            if (strengthSpecials > 0) return readyToServePotion = Potion.WyvernWine;
        }

        return readyToServePotion = Potion.WildTavernMash;
    }

    // 3. THE NEW HELPERS: These check the data files for their specific tags
    private int CountFlavor(Flavor flavorToFind)
    {
        int count = 0;
        foreach (IngredientData item in currentIngredients)
        {
            if (item.flavor == flavorToFind) count++;
        }
        return count;
    }

    private int CountEffect(MagicEffect effectToFind)
    {
        int count = 0;
        foreach (IngredientData item in currentIngredients)
        {
            if (item.effect == effectToFind) count++;
        }
        return count;
    }

    // --- REMAINS UNCHANGED ---
    public static DrinkCategory GetCategory(Potion potion)
    {
        switch (potion)
        {
            case Potion.SourLemonade:
            case Potion.AppleLemonade:
            case Potion.FrostbiteLemonade:
            case Potion.BloodLemonade:
            case Potion.SpicyLemonade:
                return DrinkCategory.Lemonade;

            case Potion.ChilledCider:
            case Potion.SunsetCider:
            case Potion.DragonbreathCider:
                return DrinkCategory.Cider;

            case Potion.GrapeJuice:
            case Potion.CrimsonSangria:
            case Potion.WyvernWine:
            case Potion.GlacierCrush:
                return DrinkCategory.Wine;

            case Potion.AppleJuice:
            case Potion.OrchardBlend:
            case Potion.CitrusCrush:
                return DrinkCategory.Juice;

            case Potion.WildTavernMash:
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

    // --- NEW: THE EFFECT CLASSIFIER ---
    // This tells the game what magic effect is inside the finished drink
    public static MagicEffect GetPotionEffect(Potion potion)
    {
        switch (potion)
        {
            case Potion.ChilledCider:
            case Potion.FrostbiteLemonade:
            case Potion.GlacierCrush:
                return MagicEffect.Frost;

            case Potion.SunsetCider:
            case Potion.BloodLemonade:
            case Potion.CrimsonSangria:
                return MagicEffect.Endurance;

            case Potion.DragonbreathCider:
            case Potion.SpicyLemonade:
            case Potion.WyvernWine:
                return MagicEffect.Strength;

            default:
                return MagicEffect.None; // Juices, pure bases, and sludge have no magic!
        }
    }
}