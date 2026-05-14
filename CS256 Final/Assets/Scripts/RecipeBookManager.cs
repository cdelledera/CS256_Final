using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class RecipeBookManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject recipeBookPanel;
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipText;

    [Header("The Slots (Exactly 4)")]
    public List<RecipeSlotUI> slots = new List<RecipeSlotUI>();

    [Header("Your Scriptable Object Recipes")]
    // Drag all your new RecipeData assets into this list in the Inspector!
    public List<RecipeData> allRecipes = new List<RecipeData>();

    private int currentPage = 0;
    private int itemsPerPage = 4;

    void Start()
    {
        recipeBookPanel.SetActive(false);
        tooltipPanel.SetActive(false);
    }

    public void OpenBook()
    {
        recipeBookPanel.SetActive(true);
        currentPage = 0;
        UpdatePageDisplay();
    }

    public void CloseBook()
    {
        recipeBookPanel.SetActive(false);
        HideTooltip(); // Make sure tooltip doesn't get stuck!
    }

    // --- NEW: LOOPING PAGE LOGIC ---
    public void NextPage()
    {
        if (allRecipes.Count == 0) return; // Safety check in case you have no recipes yet!

        int maxPage = (allRecipes.Count - 1) / itemsPerPage;

        if (currentPage < maxPage)
        {
            currentPage++; // Go to next page
        }
        else
        {
            currentPage = 0; // Loop back to the very beginning!
        }

        UpdatePageDisplay();
    }

    public void PreviousPage()
    {
        if (allRecipes.Count == 0) return;

        int maxPage = (allRecipes.Count - 1) / itemsPerPage;

        if (currentPage > 0)
        {
            currentPage--; // Go to previous page
        }
        else
        {
            currentPage = maxPage; // Loop back to the very end!
        }

        UpdatePageDisplay();
    }

    private void UpdatePageDisplay()
    {
        // Calculate where to start reading from the list (e.g., Page 0 starts at index 0. Page 1 starts at index 4)
        int startIndex = currentPage * itemsPerPage;

        for (int i = 0; i < slots.Count; i++)
        {
            int recipeIndex = startIndex + i;

            // If we have a recipe for this slot, turn the slot ON and feed it the data
            if (recipeIndex < allRecipes.Count)
            {
                slots[i].gameObject.SetActive(true);
                slots[i].SetupSlot(allRecipes[recipeIndex], this);
            }
            // If we ran out of recipes (e.g., the last page only has 2 drinks), turn the empty slots OFF
            else
            {
                slots[i].gameObject.SetActive(false);
            }
        }
    }

    // --- TOOLTIP LOGIC ---
    public void ShowTooltip(string text)
    {
        tooltipText.text = text;
        tooltipPanel.SetActive(true);

        // Optional: If you want the tooltip to follow the mouse, you can add code here to set its position to Input.mousePosition!
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }
}