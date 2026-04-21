public enum Ingredient
{
    None,
    Apple,
    Lemon,
    Grapes,
  
    Iceberries,
    BloodOrange,
    Dragonfruit
}

public enum Potion
{
    None,
    ToxicSludge,      
    WildTavernMash,  

    // 1. The Pure Bases
    AppleJuice,
    SourLemonade,
    GrapeJuice,

    // 2. The Mixed Bases
    AppleLemonade,
    OrchardBlend,     // Apple + Grapes
    CitrusCrush,      // Lemon + Grapes

    // 3. Apple + Specials
    ChilledCider,         // Apple + Ice
    SunsetCider,          // Apple + Blood Orange
    DragonbreathCider,    // Apple + Dragonfruit

    // 4. Lemon + Specials
    FrostbiteLemonade,    // Lemon + Ice
    BloodLemonade,        // Lemon + Blood Orange
    SpicyLemonade,        // Lemon + Dragonfruit

    // 5. Grapes + Specials
    GlacierCrush,         // Grapes + Ice
    CrimsonSangria,       // Grapes + Blood Orange
    WyvernWine            // Grapes + Dragonfruit
}

public enum TimeOfDay
{
    Morning,
    Afternoon,
    Evening
}