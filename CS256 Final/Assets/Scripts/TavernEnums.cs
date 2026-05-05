public enum Ingredient
{
    None,

    // --- REGULAR FRUIT (Flavors) ---
    Lemon,
    Pepper,
    Pineapple,
    DragonFruit,
    Cherry,
    Apple,

    // --- SPECIAL FRUIT (Magic Effects) ---
    PurpleLemon,
    IceCherries,
    GoldenApple,
    MagicDragonFruit, // Added "Magic" so it doesn't conflict with regular Dragon Fruit
    Watermelon,
    StarFruit,

    // --- STORY ITEMS ---
    HeartLemon // Mentioned in Joe's final anniversary ending
}

public enum Potion
{
    None,
    Slop, 

    // --- SINGLE INGREDIENT DRINKS ---
    Lemonade,       // Lemon
    AppleJuice,     // Apple
    HotSauce,       // Pepper
    DragonJuice,    // Dragonfruit
    CherryJuice,    // Cherry
    PineappleJuice, // Pineapple

    // --- MIXED DRINKS ---
    PinkLemonade,   // Lemon + Dragonfruit
    HellInAGlass,   // Lemon + Pepper
    SweetAndSpicy,  // Apple + Pepper
    PPAP,           // Pineapple + Apple

    // --- SECRET / STORY DRINKS ---
    AutumnChapple,  // Rachel asks for this if she only gets slop
    DragonKing      // Rachel mentions this on Day 2
}

// NEW: To track the flavors of the fruit
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

// NEW: To track the magic effects of the special fruit
public enum MagicEffect
{
    None, Endurance, Speed, Healing, Strength, Pondering, Charisma
}

public enum TimeOfDay
{
    Morning,
    Afternoon,
    Evening
}

public enum LilyState
{
    Any, Sober, DrunkWin, DrunkLose
}