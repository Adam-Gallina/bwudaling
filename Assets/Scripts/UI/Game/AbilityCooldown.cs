using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityCooldown : MonoBehaviour
{
    [SerializeField] private AbilityType abilityType;

    [SerializeField] private Image cooldownImg;
    [SerializeField] private Image activeImg;
    [SerializeField] private TMPro.TMP_Text activeCountdown;
    [SerializeField] private TMPro.TMP_Text keyHintText;
    [SerializeField] private TMPro.TMP_Text keyHintTextBkgd;
    public Image abilityImage;

    public void UpdateKeySet(bool mainKeys = true)
    {
        switch (abilityType)
        {
            case AbilityType.special1:
                SetKeyHintText(mainKeys ? InputController.Instance.special1.GetKey() : InputController.Instance.special1.GetAltKey());
                break;
            case AbilityType.special2:
                SetKeyHintText(mainKeys ? InputController.Instance.special2.GetKey() : InputController.Instance.special2.GetAltKey());
                break;
            case AbilityType.special3:
                SetKeyHintText(mainKeys ? InputController.Instance.special3.GetKey() : InputController.Instance.special3.GetAltKey());
                break;
        }
    }

    private void SetKeyHintText(string text)
    {
        keyHintText.text = text;
        keyHintTextBkgd.text = text;
    }

    public void SetCooldown(float remaining, float duration)
    {
        cooldownImg.gameObject.SetActive(true);
        activeImg.gameObject.SetActive(false);

        float t = Mathf.Clamp01(remaining / duration);
        cooldownImg.fillAmount = t;
    }

    public void SetActive(float remaining)
    {
        cooldownImg.gameObject.SetActive(false);
        activeImg.gameObject.SetActive(true);

        activeCountdown.text = Mathf.RoundToInt(remaining).ToString();
    }
}
