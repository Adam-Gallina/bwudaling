using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public static OptionsMenu Instance;

    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject displayMenu;
    [SerializeField] private GameObject audioMenu;
    [SerializeField] private GameObject controlsMenu;
    [SerializeField] private GameObject creditsMenu;

    [Header("Elements")]
    [Header("Display")]
    [SerializeField] private Toggle fullscreenToggle;
    [Header("Audio")]
    [SerializeField] private Slider masterVolume;
    [SerializeField] private Slider musicVolume;
    [SerializeField] private Slider sfxVolume;
    [SerializeField] private Slider uiVolume;

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

    private void Start()
    {
        PlayerSettings.Instance.LoadPrefs();
        InitializeOptions();
    }

    private void InitializeOptions()
    {
        fullscreenToggle.isOn = PlayerSettings.Instance.fullscreen;

        masterVolume.value = PlayerSettings.Instance.masterVolume;
        masterVolume.onValueChanged.AddListener(PlayerSettings.Instance.SetMasterVolume);
        musicVolume.value = PlayerSettings.Instance.musicVolume;
        musicVolume.onValueChanged.AddListener(PlayerSettings.Instance.SetMusicVolume);
        sfxVolume.value = PlayerSettings.Instance.sfxVolume;
        sfxVolume.onValueChanged.AddListener(PlayerSettings.Instance.SetSfxVolume);
        uiVolume.value = PlayerSettings.Instance.uiVolume;
        uiVolume.onValueChanged.AddListener(PlayerSettings.Instance.SetUiVolume);
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
                displayMenu.SetActive(false);
                audioMenu.SetActive(true);
                controlsMenu.SetActive(false);
                creditsMenu.SetActive(false);
                break;
            case 2:
                displayMenu.SetActive(false);
                audioMenu.SetActive(false);
                controlsMenu.SetActive(true);
                creditsMenu.SetActive(false);
                break;
            case 3:
                displayMenu.SetActive(false);
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
