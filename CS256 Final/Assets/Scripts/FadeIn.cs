using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; //  Added to allow scene switching

public class FadeIn : MonoBehaviour
{
    public Image fadeSprite;

    public void fadeIn() // This is called by your Start Button 
    {
        fadeSprite.gameObject.SetActive(true);
        StartCoroutine(fadeTimer()); // 
    }

    IEnumerator fadeTimer()
    {
        float currentalpha = 0f;

    
        while (currentalpha < 1f)
        {
            yield return new WaitForSeconds(.05f); // [cite: 4]
            currentalpha += .05f; // [cite: 5]
            fadeSprite.color = new Color(fadeSprite.color.r, fadeSprite.color.g, fadeSprite.color.b, currentalpha); // [cite: 5]
        }

        // --- NEW LOGIC BELOW ---

        // Once the loop above finishes, load the next scene in your Build Settings
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        // Ensure there actually IS a next scene to avoid errors
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("You are on the last scene! No next scene to load.");
        }
    }

    void Start()
    {

    }
 
}