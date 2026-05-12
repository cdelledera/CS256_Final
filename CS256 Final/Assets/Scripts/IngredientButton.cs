using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngredientButton : MonoBehaviour
{
    [Header("Ingredient Settings")]
    public IngredientData myIngredient;

    [Header("Manager Reference")]
    public BrewingUIManager uiManager;

    [Header("Auto-UI Setup")]
    public Image buttonIcon;
    public TextMeshProUGUI buttonText;

    void Start()
    {
        if (uiManager == null)
        {
            uiManager = FindFirstObjectByType<BrewingUIManager>();
        }

        if (myIngredient != null)
        {
            if (buttonIcon != null) buttonIcon.sprite = myIngredient.icon;
            if (buttonText != null) buttonText.text = myIngredient.ingredientName;
        }
    }

    public void SendIngredientToManager()
    {
        if (myIngredient == null) return;

        // 1. Tell the Logic Manager to remember the ingredient
        if (myIngredient.effect != MagicEffect.None)
        {
            uiManager.SelectSpecialIngredient(myIngredient);
        }
        else
        {
            uiManager.SelectBaseIngredient(myIngredient);
        }

        if (BrewingVisuals.Instance != null)
        {
            // CHANGED: We now send the WHOLE myIngredient profile instead of just the icon!
            BrewingVisuals.Instance.DropIngredientIntoGlass(myIngredient);
        }
    }
}