using UnityEngine;

public class CauldronInteract : MonoBehaviour
{
    [Header("References")]
    public BrewingUIManager uiManager;

    private bool isPlayerInRange = false;

    void Start()
    {
        // Auto-find the UI manager if the slot is empty
        if (uiManager == null)
        {
            uiManager = Object.FindFirstObjectByType<BrewingUIManager>();

            if (uiManager != null)
                Debug.Log("Success: Cauldron found the UI Manager automatically!");
            else
                Debug.LogError("CRITICAL: There is no BrewingUIManager anywhere in the scene!");
        }
    }

    // --- REPLACED: Update() is gone. We use the Mouse now! ---
    void OnMouseDown()
    {
        // 1. Are we standing close enough to the cauldron?
        if (isPlayerInRange && uiManager != null)
        {
            // 2. Is the screen clear of other menus/dialogue?
            if (!GameManager.Instance.isUIActive)
            {
                uiManager.ShowConfirmPrompt();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;

            if (uiManager != null)
            {
                uiManager.CancelBrewing();
            }
        }
    }
}