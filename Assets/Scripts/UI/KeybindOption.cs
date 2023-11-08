using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeybindOption : MonoBehaviour
{
    public KeybindType key;

    [SerializeField] private TMPro.TMP_Text primaryText;
    [SerializeField] private GameObject primaryWarning;
    [SerializeField] private TMPro.TMP_Text alternateText;
    [SerializeField] private GameObject alternateWarning;

    private void Awake()
    {
        primaryWarning.SetActive(false);
        alternateWarning.SetActive(false);
    }

    public void DisplayKeybinds()
    {
        primaryText.text = InputController.Instance.GetKey(key).targetKey.ToString();
        alternateText.text = InputController.Instance.GetKey(key).alternateKey.ToString();
    }

    public void SetPrimaryWarning(bool show) { primaryWarning.SetActive(show); }
    public void SetAlternateWarning(bool show) { alternateWarning.SetActive(show); }

    public void StartChangingPrimaryKey()
    {
        OptionsMenu.Instance.StartKeySelector(this, false, key.ToString());
    }

    public void StartChangingAltKey()
    {
        OptionsMenu.Instance.StartKeySelector(this, true, key.ToString() + " (alternate)");
    }
}
