using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MenuHead : MonoBehaviour
{
    [SerializeField] private int headId;
    [SerializeField] private GameObject speechBubble;

    [SerializeField] private UnityEvent OnSelect;

    private void Awake()
    {
        speechBubble.SetActive(false);
    }

    public void ButtonClicked()
    {
        //MenuHeadController.Instance.SelectHead(headId, () => { OnSmack(); });
        MenuHeadController.Instance.SelectHead(headId, null);
        OnSmack();
        speechBubble.SetActive(false);
    }

    private void OnSmack()
    {
        OnSelect?.Invoke();
    }

    public void MouseEntered()
    {
        speechBubble.SetActive(true);

        MenuHeadController.Instance.HoverHead(headId);

    }

    public void MouseExited()
    {
        speechBubble.SetActive(false);

        MenuHeadController.Instance.UnhoverHead(headId);
    }
}
