using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BrewingVisuals : MonoBehaviour
{
    public static BrewingVisuals Instance;

    [Header("UI References")]
    public RectTransform dropZone;

    [Header("Physics & Visual Settings")]
    public float gravity = -2500f;
    public float bounceFactor = -0.4f;
    public float startingHeight = 400f;

    [Tooltip("How much to shrink the fruit (e.g., 0.5 is half size)")]
    public float fruitScale = 0.5f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void DropIngredientIntoGlass(IngredientData ingredient)
    {
        // 1. CHANGED: Now allows up to 3 items in the glass!
        if (dropZone.childCount >= 3)
        {
            return;
        }

        // 2. Failsafe duplicate check (just in case!)
        foreach (Transform child in dropZone)
        {
            Image childImage = child.GetComponent<Image>();

            if (childImage != null && childImage.sprite == ingredient.icon)
            {
                return; // Stop right here, no duplicates allowed!
            }
        }

        // If we passed both checks, drop the fruit!
        StartCoroutine(SimulateDrop(ingredient));
    }

    private IEnumerator SimulateDrop(IngredientData ingredient)
    {
        GameObject fallingItem = new GameObject("Falling_" + ingredient.ingredientName);
        fallingItem.transform.SetParent(dropZone, false);

        Image img = fallingItem.AddComponent<Image>();
        img.sprite = ingredient.icon;
        img.SetNativeSize();

        RectTransform rect = fallingItem.GetComponent<RectTransform>();

        float finalScale = fruitScale * ingredient.visualScaleMultiplier;
        rect.localScale = new Vector3(finalScale, finalScale, 1f);

        rect.anchoredPosition = new Vector2(0, startingHeight);

        float velocityY = 0f;
        float currentY = startingHeight;
        int bounces = 0;
        int maxBounces = 3;

        while (bounces < maxBounces)
        {
            if (rect == null) yield break;

            velocityY += gravity * Time.deltaTime;
            currentY += velocityY * Time.deltaTime;

            if (currentY <= 0)
            {
                currentY = 0;
                velocityY *= bounceFactor;
                bounces++;

                rect.localEulerAngles = new Vector3(0, 0, Random.Range(-15f, 15f));

                if (Mathf.Abs(velocityY) < 50f) break;
            }

            rect.anchoredPosition = new Vector2(0, currentY);

            yield return null;
        }

        if (rect != null)
        {
            rect.anchoredPosition = Vector2.zero;
        }
    }

    public void ClearGlassVisuals()
    {
        foreach (Transform child in dropZone)
        {
            Destroy(child.gameObject);
        }
    }
}