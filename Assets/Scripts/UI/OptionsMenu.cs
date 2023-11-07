using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsMenu : MonoBehaviour
{
    public static OptionsMenu Instance;

    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject displayMenu;
    [SerializeField] private GameObject audioMenu;
    [SerializeField] private GameObject controlsMenu;
    [SerializeField] private GameObject creditsMenu;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        menu.SetActive(false);
    }

    public void ToggleOptionsMenu()
    {
        menu.SetActive(!menu.activeSelf);

        if (!menu.activeSelf && MenuHeadController.Instance)
        {
            MenuHeadController.Instance.ShowHeads(2);
        } 
    }

    public void SetMenu(int m)
    {
        switch (m)
        {
            case 0:
                displayMenu.SetActive(true);
                audioMenu.SetActive(false);
                controlsMenu.SetActive(false);
                creditsMenu.SetActive(false);
                break;
            case 1:
                menu.SetActive(false);
                audioMenu.SetActive(true);
                controlsMenu.SetActive(false);
                creditsMenu.SetActive(false);
                break;
            case 2:
                menu.SetActive(false);
                audioMenu.SetActive(false);
                controlsMenu.SetActive(true);
                creditsMenu.SetActive(false);
                break;
            case 3:
                menu.SetActive(false);
                audioMenu.SetActive(false);
                controlsMenu.SetActive(false);
                creditsMenu.SetActive(true);
                break;
            default:
                Debug.LogError("Could not show requested menu");
                break;
        }
    }
}
