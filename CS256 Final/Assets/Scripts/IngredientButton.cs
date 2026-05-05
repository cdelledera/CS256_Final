using UnityEngine;
using UnityEngine.UI; // NEW: Needed to change the Image!
using TMPro;

public class IngredientButton : MonoBehaviour
{
    [Header("Ingredient Settings")]
    public IngredientData myIngredient;

    [Header("Manager Reference")]
    public BrewingUIManager uiManager;

    [Header("Auto-UI Setup")]
    public Image buttonIcon; // Drag the button's Image component here
    public TextMeshProUGUI buttonText; // Drag the button's Text here

    void Start()
    {
        // NEW: Automatically find the UI Manager in the scene!
        if (uiManager == null)
        {
            uiManager = FindFirstObjectByType<BrewingUIManager>();
        }

        // When the game starts, automatically set the picture and text based on the data file!
        if (myIngredient != null)
        {
            if (buttonIcon != null) buttonIcon.sprite = myIngredient.icon;
            if (buttonText != null) buttonText.text = myIngredient.ingredientName;
        }
    }

    public void SendIngredientToManager()
    {
        if (myIngredient == null) return;

        if (myIngredient.effect != MagicEffect.None)
        {
            uiManager.SelectSpecialIngredient(myIngredient);
        }
        else
        {
            uiManager.SelectBaseIngredient(myIngredient);
        }
    }
}