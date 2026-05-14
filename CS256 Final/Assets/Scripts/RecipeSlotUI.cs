using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required for Hover events!
using TMPro;

// We add these interfaces to listen for the mouse
public class RecipeSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public Image potionIcon;
    public TextMeshProUGUI potionNameText;

    private RecipeData myRecipe;
    private RecipeBookManager bookManager;

    // The Manager calls this to set up the slot when you turn the page
    public void SetupSlot(RecipeData data, RecipeBookManager manager)
    {
        myRecipe = data;
        bookManager = manager;

        potionIcon.sprite = data.potionSprite;
        potionNameText.text = data.potionName;
    }

    // Triggered the moment the mouse touches the slot
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (myRecipe != null)
        {
            bookManager.ShowTooltip(myRecipe.recipeDescription);
        }
    }

    // Triggered the moment the mouse leaves the slot
    public void OnPointerExit(PointerEventData eventData)
    {
        bookManager.HideTooltip();
    }
}