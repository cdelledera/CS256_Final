public enum Ingredient
{
    None,

    // --- REGULAR FRUIT (Base Flavors) ---
    Lemon,
    Pepper,
    Pineapple,
    DragonFruit,
    Cherry,
    Apple,

    // --- SPECIAL FRUIT (Magic Effects) ---
    PurpleLemon,       // Endurance
    IceCherries,       // Speed
    GoldenApple,       // Healing
    DragonheadFruit,   // Strength
    Watermelon,        // Pondering
    StarFruit,         // Charisma

    // --- STORY ITEMS ---
    HeartLemon         // Mentioned in Joe's final anniversary ending
}

public enum Potion
{
    None,
    Slop,

    // --- SINGLE INGREDIENT DRINKS ---
    Lemonade,            // Lemon
    AppleJuice,          // Apple
    HotSauce,            // Pepper
    DragonJuice,         // Dragonfruit
    CherryJuice,         // Cherry
    PineappleJuice,      // Pineapple

    // --- TWO INGREDIENT MIXES ---
    PinkLemonade,        // Lemon + Dragonfruit
    HellInAGlass,        // Lemon + Pepper
    SweetAndSpicy,       // Apple + Pepper
    PPAP,                // Pineapple + Apple
    SourApple,           // Lemon + Apple
    SourCherry,      // Lemon + Cherry
    TropicalCitrus,      // Lemon + Pineapple
    DragonbiteCider,     // Apple + Dragonfruit
    RomanceSmoothie,     // Cherry + Dragonfruit
    FirecrackerMash,     // Cherry + Pepper
    SunsetJuice,         // Cherry + Pineapple
    DragonblazeSmoothie, // Dragonfruit + Pepper
    VolcanoJuice,        // Pineapple + Pepper

    // --- SECRET / STORY DRINKS ---
    AutumnChapple,       // Rachel asks for this if she only gets slop
    DragonKing           // Rachel mentions this on Day 2
}

// Tracks the physical flavor profiles
public enum DrinkFlavor
{
    None,
    Sour,
    Spicy,
    Tart,
    Subtle,
    Bitter,
    Sweet
}

// Tracks the magic effects passed to the final drink
public enum MagicEffect
{
    None,
    Endurance,
    Speed,
    Healing,
    Strength,
    Pondering,
    Charisma
}

// World state trackers
public enum TimeOfDay
{
    Morning,
    Afternoon,
    Evening
}