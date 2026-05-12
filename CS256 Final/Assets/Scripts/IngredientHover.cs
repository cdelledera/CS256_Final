using UnityEngine;
using UnityEngine.EventSystems; // Required for the Hover interfaces!

// Make sure to add the IPointerEnterHandler and IPointerExitHandler interfaces here!
public class IngredientHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("The Ingredient Data")]
    
    public IngredientData myIngredient;

    // This triggers the exact frame the mouse enters the UI element
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Tell the TooltipManager to wake up and display this specific data!
        // (Make sure to change these variable names to match what is inside your ScriptableObject)
        TooltipManager.Instance.ShowTooltip(myIngredient.ingredientName, myIngredient.description);
    }

    // This triggers the exact frame the mouse leaves the UI element
    public void OnPointerExit(PointerEventData eventData)
    {
        // Tell the TooltipManager to hide!
        TooltipManager.Instance.HideTooltip();
    }
}