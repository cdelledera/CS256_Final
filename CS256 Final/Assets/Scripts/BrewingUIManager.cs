using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BrewingUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject confirmPromptPanel;
    public GameObject brewingMenuPanel;

    [Header("Main HUD")]
    public GameObject mainHUDPanel;

    [Header("Result Screen")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;

    [Header("The Visual Slots")]
    public TextMeshProUGUI slot1Text;
    public TextMeshProUGUI slot2Text;

    [Tooltip("This now acts as our Magic Effect scanner!")]
    public TextMeshProUGUI specialSlotText;

    public PotionBrewing brewingSystem;

    // NEW: A single list that holds a maximum of 2 ingredients!
    private List<IngredientData> currentIngredients = new List<IngredientData>();

    void Start()
    {
        confirmPromptPanel.SetActive(false);
        brewingMenuPanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(false);
    }

    public void ShowConfirmPrompt()
    {
        confirmPromptPanel.SetActive(true);
        GameManager.Instance.isUIActive = true;
    }

    public void CancelBrewing()
    {
        if (confirmPromptPanel != null) confirmPromptPanel.SetActive(false);
        if (brewingMenuPanel != null) brewingMenuPanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(false);
        if (mainHUDPanel != null) mainHUDPanel.SetActive(true);
        GameManager.Instance.isUIActive = false;

        
    }

    public void OpenBrewingMenu()
    {
        confirmPromptPanel.SetActive(false);
        brewingMenuPanel.SetActive(true);
        resultPanel.SetActive(false);
        ClearCauldron();

        GameManager.Instance.isUIActive = true;
        if (mainHUDPanel != null) mainHUDPanel.SetActive(false);
    }

    public void ClearCauldron()
    {
        currentIngredients.Clear();
        UpdateUI();
    }

    // Since IngredientButton.cs still calls these two functions, 
    // we keep them but point them both to the same logic!
    public void SelectBaseIngredient(IngredientData baseItem)
    {
        TryAddIngredient(baseItem);
    }

    public void SelectSpecialIngredient(IngredientData specialItem)
    {
        TryAddIngredient(specialItem);
    }

    private void TryAddIngredient(IngredientData item)
    {
        // Prevent adding if the pot is full (Max 2 ingredients)
        if (currentIngredients.Count >= 2) return;

        // Prevent adding the exact same fruit twice
        if (currentIngredients.Contains(item)) return;

        currentIngredients.Add(item);
        UpdateUI();
    }

    private void UpdateUI()
    {
        slot1Text.text = "Empty";
        slot2Text.text = "Empty";
        specialSlotText.text = "Effect: None";

        if (currentIngredients.Count > 0) slot1Text.text = currentIngredients[0].ingredientName;
        if (currentIngredients.Count > 1) slot2Text.text = currentIngredients[1].ingredientName;

        // --- THE MAGIC SCANNER ---
        MagicEffect activeEffect = MagicEffect.None;
        foreach (IngredientData item in currentIngredients)
        {
            if (item.effect != MagicEffect.None)
            {
                activeEffect = item.effect;
            }
        }

        // Update the 3rd slot to show the magic!
        if (activeEffect != MagicEffect.None)
        {
            specialSlotText.text = "Effect: " + activeEffect.ToString();
            specialSlotText.color = Color.cyan; // Gives it a magical glow!
        }
        else
        {
            specialSlotText.text = "Effect: None";
            specialSlotText.color = Color.white;
        }
    }

    public void ConfirmBrew()
    {
        if (currentIngredients.Count == 0) return;

        // Dump everything into the actual Cauldron script
        foreach (IngredientData item in currentIngredients)
        {
            brewingSystem.AddIngredient(item);
        }

        Potion craftedPotion = brewingSystem.Brew();
        resultPanel.SetActive(true);

        // Updated to perfectly match your new "Slop" naming convention!
        if (craftedPotion == Potion.Slop)
        {
            resultText.text = "Failed!\nYou made Slop!";
        }
        else
        {
            // If the drink has magic, we tell the player on the success screen!
            MagicEffect effect = brewingSystem.readyToServeEffect;
            string effectString = effect != MagicEffect.None ? $"\n(Magic: {effect})" : "";

            resultText.text = "Success!\nYou brewed:\n" + PotionBrewing.GetDisplayName(craftedPotion) + effectString;
        }
    }

    public void CollectPotionAndClose()
    {
        CancelBrewing();
    }
}