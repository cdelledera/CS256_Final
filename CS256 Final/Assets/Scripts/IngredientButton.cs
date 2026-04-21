using UnityEngine;

public class IngredientButton : MonoBehaviour
{
    [Header("Ingredient Settings")]
    
    public IngredientData myIngredient;

    [Header("Manager Reference")]
    public BrewingUIManager uiManager;

    public void SendIngredientToManager()
    {
       
        if (myIngredient == null)
        {
            Debug.LogError("There is no IngredientData plugged into this button!");
            return;
        }

        
        if (myIngredient.type == ItemType.SpecialModifier)
        {
            
            uiManager.SelectSpecialIngredient(myIngredient);
        }
        else
        {
            
            uiManager.SelectBaseIngredient(myIngredient);
        }
    }
}