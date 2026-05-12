using UnityEngine;
using System.Collections;

public class MenuTransitionManager : MonoBehaviour
{
    public static MenuTransitionManager Instance;

    [Header("Menu State")]
    public CanvasGroup masterMenuFade;
    public GameObject tavernBackground; // Leave empty if you put the image on the panel!
    public float startingBgZoom = 1.2f;

    [Header("Main HUD")]
    public GameObject mainHUD; // NEW: Drag your MainHUD panel here!

    [Header("UI Containers to Slide")]
    public RectTransform leftPanel;
    public RectTransform rightPanel;
    public RectTransform centerConsole;

    [Header("Animation Settings")]
    public float transitionSpeed = 0.5f;
    public AnimationCurve slideCurve;

    private Vector2 leftTarget;
    private Vector2 rightTarget;
    private Vector2 centerTarget;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        leftTarget = leftPanel.anchoredPosition;
        rightTarget = rightPanel.anchoredPosition;
        centerTarget = centerConsole.anchoredPosition;

        HideMenuInstantly();
    }

    public void HideMenuInstantly()
    {
        masterMenuFade.alpha = 0f;
        masterMenuFade.interactable = false;
        masterMenuFade.blocksRaycasts = false;

        if (tavernBackground != null) tavernBackground.SetActive(false);

        leftPanel.anchoredPosition = leftTarget + new Vector2(-800f, 0);
        rightPanel.anchoredPosition = rightTarget + new Vector2(800f, 0);
        centerConsole.anchoredPosition = centerTarget + new Vector2(0, 800f);
    }

    public void OpenBrewingMenu()
    {
        Time.timeScale = 1f;

        // NEW: Turn off the Main HUD when the menu opens!
        if (mainHUD != null) mainHUD.SetActive(false);

        StartCoroutine(PlayOpenTransition());
    }

    private IEnumerator PlayOpenTransition()
    {
        masterMenuFade.gameObject.SetActive(true);

        if (tavernBackground != null)
        {
            tavernBackground.SetActive(true);
            tavernBackground.transform.localScale = new Vector3(startingBgZoom, startingBgZoom, 1f);
        }

        masterMenuFade.interactable = true;
        masterMenuFade.blocksRaycasts = true;

        float elapsed = 0f;

        Vector3 startBgScale = new Vector3(startingBgZoom, startingBgZoom, 1f);
        Vector3 endBgScale = Vector3.one;

        // THE FIX: Mathematically force the start positions so it ALWAYS slides, 
        // even if another script closed the menu weirdly!
        Vector2 startLeft = leftTarget + new Vector2(-800f, 0);
        Vector2 startRight = rightTarget + new Vector2(800f, 0);
        Vector2 startCenter = centerTarget + new Vector2(0, 800f);

        // Snap them off-screen before the loop starts
        leftPanel.anchoredPosition = startLeft;
        rightPanel.anchoredPosition = startRight;
        centerConsole.anchoredPosition = startCenter;

        while (elapsed < transitionSpeed)
        {
            elapsed += Time.unscaledDeltaTime;

            float normalTime = Mathf.Clamp01(elapsed / transitionSpeed);

            float smoothStep = Mathf.SmoothStep(0, 1, normalTime);
            masterMenuFade.alpha = Mathf.Lerp(0f, 1f, smoothStep);

            if (tavernBackground != null)
                tavernBackground.transform.localScale = Vector3.Lerp(startBgScale, endBgScale, smoothStep);

            float curveValue = slideCurve.Evaluate(normalTime);
            leftPanel.anchoredPosition = Vector2.LerpUnclamped(startLeft, leftTarget, curveValue);
            rightPanel.anchoredPosition = Vector2.LerpUnclamped(startRight, rightTarget, curveValue);
            centerConsole.anchoredPosition = Vector2.LerpUnclamped(startCenter, centerTarget, curveValue);

            yield return null;
        }

        if (tavernBackground != null) tavernBackground.transform.localScale = endBgScale;
        masterMenuFade.alpha = 1f;
        leftPanel.anchoredPosition = leftTarget;
        rightPanel.anchoredPosition = rightTarget;
        centerConsole.anchoredPosition = centerTarget;
    }

    public void CloseBrewingMenu()
    {
        masterMenuFade.gameObject.SetActive(false);
        if (tavernBackground != null) tavernBackground.SetActive(false);

        // NEW: Turn the HUD back on when we close the menu!
        if (mainHUD != null) mainHUD.SetActive(true);

        HideMenuInstantly();
    }
}