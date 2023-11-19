using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public static OptionsMenu Instance;

    [SerializeField] private GameObject menu;
    [SerializeField] private OptionsPage displayMenu;
    [SerializeField] private OptionsPage audioMenu;
    [SerializeField] private OptionsPage controlsMenu;
    [SerializeField] private OptionsPage creditsMenu;

    [Header("Elements")]
    [Header("Controls")]
    [SerializeField] private GameObject keySelector;
    [SerializeField] private TMPro.TMP_Text keySelectText;

    [Header("Display")]
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Audio")]
    [SerializeField] private Slider masterVolume;
    [SerializeField] private Slider musicVolume;
    [SerializeField] private Slider sfxVolume;
    [SerializeField] private Slider uiVolume;

    private KeybindOption[] keybinds;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Make sure button is lowered on start
        SetMenu(1);
        SetMenu(0);
        menu.SetActive(false);
        keySelector.SetActive(false);
    }

    private void Start()
    {
        PlayerSettings.Instance.LoadAudioPrefs();

        InputController.Instance.LoadKeybinds();
        keybinds = GetComponentsInChildren<KeybindOption>(includeInactive: true);
        foreach (KeybindOption keybind in keybinds)
            keybind.DisplayKeybinds();

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
                displayMenu.Show();
                audioMenu.Hide();
                controlsMenu.Hide();
                creditsMenu.Hide();
                break;
            case 1:
                displayMenu.Hide();
                audioMenu.Show();
                controlsMenu.Hide();
                creditsMenu.Hide();
                break;
            case 2:
                displayMenu.Hide();
                audioMenu.Hide();
                controlsMenu.Show();
                creditsMenu.Hide();
                break;
            case 3:
                displayMenu.Hide();
                audioMenu.Hide();
                controlsMenu.Hide();
                creditsMenu.Show();
                break;
            default:
                Debug.LogError("Could not show requested menu");
                break;
        }
    }

    private Coroutine keySelect;
    private KeyCode[] keys;
    public void StartKeySelector(KeybindOption key, bool changeAlt, string description="")
    {
        keySelector.SetActive(true);
        keySelectText.text = description;
        keySelect = StartCoroutine(ChangeKey(key, changeAlt));
    }
    public void StopKeySelector()
    {
        keySelector.SetActive(false);
        if (keySelect != null)
            StopCoroutine(keySelect);
    }
    private IEnumerator ChangeKey(KeybindOption key, bool changeAlt)
    {
        if (keys == null)
            keys = (KeyCode[])System.Enum.GetValues(typeof(KeyCode));

        bool foundKey = false;
        while (!foundKey)
        {
            while (!Input.anyKeyDown)
                yield return null;

            foreach (KeyCode k in keys)
            {
                if (Input.GetKey(k))
                {
                    if (k == KeyCode.Mouse0 || k == KeyCode.Mouse1)
                        break;

                    KeyCode kc = k != KeyCode.Escape ? k : KeyCode.None;
                    if (changeAlt == false)
                        InputController.Instance.GetKey(key.key).SetTargetKey(kc);
                    else
                        InputController.Instance.GetKey(key.key).SetAlternateKey(kc);

                    foundKey = true;
                    break;
                }
            }

            yield return new WaitForEndOfFrame();
        }

        keySelector.SetActive(false);
        key.DisplayKeybinds();
        CheckKeyBinds();
    }

    private void CheckKeyBinds()
    {
        foreach (KeybindOption k in keybinds)
        {
            k.SetPrimaryWarning(false);
            k.SetAlternateWarning(false);
        }

        for (int i = 0; i < keybinds.Length - 1; i++)
        {
            Key k1 = InputController.Instance.GetKey(keybinds[i].key);

            if (k1.targetKey == KeyCode.None && k1.alternateKey == KeyCode.None)
            {
                keybinds[i].SetPrimaryWarning(true);
                continue;
            }

            for (int j = i + 1; j < keybinds.Length; j++)
            {
                
                Key k2 = InputController.Instance.GetKey(keybinds[j].key);

                if (k1.targetKey == k2.targetKey && k1.targetKey != KeyCode.None)
                {
                    keybinds[i].SetPrimaryWarning(true);
                    keybinds[j].SetPrimaryWarning(true);
                }
                if (k1.targetKey == k2.alternateKey && k1.targetKey != KeyCode.None)
                {
                    keybinds[i].SetPrimaryWarning(true);
                    keybinds[j].SetAlternateWarning(true);
                }
                if (k1.alternateKey == k2.alternateKey && k1.alternateKey != KeyCode.None)
                {
                    keybinds[i].SetAlternateWarning(true);
                    keybinds[j].SetAlternateWarning(true);
                }
                if (k1.alternateKey == k2.targetKey && k1.alternateKey != KeyCode.None)
                {
                    keybinds[i].SetAlternateWarning(true);
                    keybinds[j].SetPrimaryWarning(true);
                }
            }
        }
    }
}

[System.Serializable]
public class OptionsPage
{
    public RectTransform button;
    public GameObject page;
    private Coroutine currRoutine;

    public void Show()
    {
        if (!page.activeSelf)
        {
            if (currRoutine != null)
                OptionsMenu.Instance.StopCoroutine(currRoutine);
            currRoutine = OptionsMenu.Instance.StartCoroutine(LowerBtn());
        }
        page.SetActive(true);
    }

    public void Hide()
    {
        if (page.activeSelf)
        {
            if (currRoutine != null)
                OptionsMenu.Instance.StopCoroutine(currRoutine);
            currRoutine = OptionsMenu.Instance.StartCoroutine(RaiseBtn());
        }
        page.SetActive(false);
    }

    private IEnumerator LowerBtn()
    {
        Vector3 pos = button.localPosition;
        float end = Time.time + 0.25f;
        while (Time.time < end)
        {
            float t = 1 - ((end - Time.time) / 0.25f);
            pos.y = -10 * t;
            button.localPosition = pos;

            yield return null;
        }

        pos.y = -10;
        button.localPosition = pos;
    }
    private IEnumerator RaiseBtn()
    {
        Vector3 pos = button.localPosition;
        float end = Time.time + 0.25f;
        while (Time.time < end)
        {
            float t = 1 - ((end - Time.time) / 0.25f);
            pos.y = -10 + 10 * t;
            button.localPosition = pos;

            yield return null;
        }

        pos.y = 0;
        button.localPosition = pos;
    }
}