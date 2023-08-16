using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI Instance;

    [SerializeField] private RectTransform staminaBarObj;
    [SerializeField] private Image staminaBar;

    private Transform target;

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    private void Awake()
    {
        Instance = this;
    }


    private void Update()
    {
        if (target) { transform.position = target.position; }
    }

    public void SetStaminaBar(bool visible)
    {
        staminaBarObj.gameObject.SetActive(visible);
    }

    public void SetStaminaBarFillAmount(float amount)
    {
        staminaBar.fillAmount = amount;
    }
}
