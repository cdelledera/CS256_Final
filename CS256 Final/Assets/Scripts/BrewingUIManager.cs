using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

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
    public TextMeshProUGUI specialSlotText;

    // --- NEW: The dedicated text for the Effect button! ---
    public TextMeshProUGUI effectText;

    public PotionBrewing brewingSystem;

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
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("BrewMenuCancel");
        if (confirmPromptPanel != null) confirmPromptPanel.SetActive(false);

        if (MenuTransitionManager.Instance != null)
        {
            MenuTransitionManager.Instance.CloseBrewingMenu();
        }
        else
        {
            if (brewingMenuPanel != null) brewingMenuPanel.SetActive(false);
            if (mainHUDPanel != null) mainHUDPanel.SetActive(true);
        }

        if (resultPanel != null) resultPanel.SetActive(false);

        ClearCauldron();
        GameManager.Instance.isUIActive = false;
    }

    public void OpenBrewingMenu()
    {
        confirmPromptPanel.SetActive(false);

        if (MenuTransitionManager.Instance != null)
        {
            MenuTransitionManager.Instance.OpenBrewingMenu();
        }
        else
        {
            brewingMenuPanel.SetActive(true);
            if (mainHUDPanel != null) mainHUDPanel.SetActive(false);
        }

        resultPanel.SetActive(false);
        ClearCauldron();
        GameManager.Instance.isUIActive = true;
    }

    public void ClearCauldron()
    {
        if (brewingSystem != null)
        {
            brewingSystem.currentIngredients.Clear();
        }

        UpdateUI();

        if (BrewingVisuals.Instance != null)
        {
            BrewingVisuals.Instance.ClearGlassVisuals();
        }

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

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
        if (brewingSystem.TryAddIngredient(item))
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("FruitSelect");
            UpdateUI();

            
            if (BrewingVisuals.Instance != null)
            {
                BrewingVisuals.Instance.DropIngredientIntoGlass(item);
            }
        }
    }

    private void UpdateUI()
    {
        // 1. Reset all text to empty
        slot1Text.text = "Empty";
        slot2Text.text = "Empty";
        specialSlotText.text = "Empty";

        if (effectText != null)
        {
            effectText.text = "Effect: None";
            effectText.color = Color.white;
        }

        int baseCount = 0;

        // 2. Sort ingredients into the correct slots
        foreach (IngredientData item in brewingSystem.currentIngredients)
        {
            if (item.effect != MagicEffect.None)
            {
                // Slot 3 just gets the name now!
                specialSlotText.text = item.ingredientName;

                // The Effect button gets the actual effect text and color
                if (effectText != null)
                {
                    effectText.text = "Effect: " + item.effect.ToString();
                    effectText.color = GetEffectColor(item.effect);
                }
            }
            else
            {
                if (baseCount == 0)
                {
                    slot1Text.text = item.ingredientName;
                    baseCount++;
                }
                else if (baseCount == 1)
                {
                    slot2Text.text = item.ingredientName;
                    baseCount++;
                }
            }
        }
    }

    // --- NEW: Helper function to assign colors to effects! ---
    private Color GetEffectColor(MagicEffect effect)
    {
        switch (effect)
        {
            case MagicEffect.Strength:
                return Color.red;

            case MagicEffect.Speed:
                return Color.cyan;

            case MagicEffect.Healing:
                return Color.yellow;

            case MagicEffect.Endurance:
                return new Color(1f, 0.5f, 0f);    // Orange

            case MagicEffect.Pondering:
                return new Color(0.6f, 0.2f, 0.8f);  // Purple

            case MagicEffect.Charisma:
                return new Color(1f, 0.4f, 0.7f);    // Pink

            default:
                return Color.white; // A safe fallback if an effect has no color assigned
        }
    }

    public void ConfirmBrew()
    {
        if (brewingSystem.currentIngredients.Count == 0) return;

        Potion craftedPotion = brewingSystem.Brew();

        resultPanel.SetActive(true);

        if (MenuTransitionManager.Instance != null)
        {
            MenuTransitionManager.Instance.CloseBrewingMenu();

            if (MenuTransitionManager.Instance.mainHUD != null)
            {
                MenuTransitionManager.Instance.mainHUD.SetActive(false);
            }
        }

        if (craftedPotion == Potion.Slop)
        {
            resultText.text = "Failed!\nYou made Slop!";
        }
        else
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("DrinkMade");
            resultText.text = "Success!\nYou brewed:\n" + PotionBrewing.GetDisplayName(craftedPotion, brewingSystem.readyToServeEffect);
        }
    }

    public void CollectPotionAndClose()
    {
        if (resultPanel != null) resultPanel.SetActive(false);

        if (MenuTransitionManager.Instance != null && MenuTransitionManager.Instance.mainHUD != null)
        {
            MenuTransitionManager.Instance.mainHUD.SetActive(true);
        }
        else if (mainHUDPanel != null)
        {
            mainHUDPanel.SetActive(true);
        }

        ClearCauldron();
        GameManager.Instance.isUIActive = false;
    }
}