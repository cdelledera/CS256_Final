using UnityEngine;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    [Header("UI References")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Make sure it starts hidden
        tooltipPanel.SetActive(false);
    }

    // Notice: The Update() loop is completely gone! 
    // The panel will now just stay exactly where you put it in the Canvas.

    public void ShowTooltip(string itemName, string itemDescription)
    {
        nameText.text = itemName;
        descriptionText.text = itemDescription;
        tooltipPanel.SetActive(true);
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
        nameText.text = "";
        descriptionText.text = "";
    }
}