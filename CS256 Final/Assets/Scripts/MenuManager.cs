using UnityEngine;

public class MenuManager : MonoBehaviour
{

    public GameObject optionsMenu;
    public GameObject mainMenu;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void OpenOptions()
    {
        mainMenu.SetActive(false);
        optionsMenu.SetActive(true);
    }

    public void BackToMain()
    {
        optionsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
