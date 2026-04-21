using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BrewingUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject confirmPromptPanel;
    public GameObject brewingMenuPanel;

    [Header("Result Screen")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;

    [Header("The Visual Slots")]
    public TextMeshProUGUI slot1Text;
    public TextMeshProUGUI slot2Text;
    public TextMeshProUGUI specialSlotText;

    public PotionBrewing brewingSystem;

   
    private List<IngredientData> selectedBaseIngredients = new List<IngredientData>();
    private IngredientData selectedSpecialIngredient = null;

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

        GameManager.Instance.isUIActive = false;
    }

    public void OpenBrewingMenu()
    {
        confirmPromptPanel.SetActive(false);
        brewingMenuPanel.SetActive(true);
        resultPanel.SetActive(false);
        ClearCauldron();

        GameManager.Instance.isUIActive = true;
    }

    public void ClearCauldron()
    {
        selectedBaseIngredients.Clear();
        selectedSpecialIngredient = null; 
        UpdateUI();
    }

    
    public void SelectBaseIngredient(IngredientData baseItem)
    {
        
        if (selectedBaseIngredients.Count >= 2) return;
 
        if (selectedBaseIngredients.Contains(baseItem)) return;

        selectedBaseIngredients.Add(baseItem);
        UpdateUI();
    }

    
    public void SelectSpecialIngredient(IngredientData specialItem)
    {
        selectedSpecialIngredient = specialItem;
        UpdateUI();
    }

    private void UpdateUI()
    {
        slot1Text.text = "Empty";
        slot2Text.text = "Empty";
        specialSlotText.text = "Empty";

        
        if (selectedBaseIngredients.Count > 0) slot1Text.text = selectedBaseIngredients[0].ingredientName;
        if (selectedBaseIngredients.Count > 1) slot2Text.text = selectedBaseIngredients[1].ingredientName;
        if (selectedSpecialIngredient != null) specialSlotText.text = selectedSpecialIngredient.ingredientName;
    }

    public void ConfirmBrew()
    {
        
        if (selectedBaseIngredients.Count == 0 && selectedSpecialIngredient == null) return;

        
        foreach (IngredientData item in selectedBaseIngredients)
        {
            brewingSystem.AddIngredient(item);
        }

        if (selectedSpecialIngredient != null)
        {
            brewingSystem.AddIngredient(selectedSpecialIngredient);
        }

        Potion craftedPotion = brewingSystem.Brew();

        resultPanel.SetActive(true);

        if (craftedPotion == Potion.ToxicSludge)
        {
            resultText.text = "Failed!\nYou made Toxic Sludge!";
        }
        else
        {
            resultText.text = "Success!\nYou brewed:\n" + PotionBrewing.GetDisplayName(craftedPotion);
        }
    }

    public void CollectPotionAndClose()
    {
        CancelBrewing();
    }
}