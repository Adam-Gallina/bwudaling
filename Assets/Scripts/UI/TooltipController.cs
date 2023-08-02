using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TooltipController : MonoBehaviour
{
    private float lastMouseOver;
    [SerializeField] private float showDelay = 0.5f;
    private bool mouseOver = false;

    public string tooltip;

    public void MouseEntered()
    {
        lastMouseOver = Time.time;
        mouseOver = true;
    }

    public void MouseExited()
    {
        mouseOver = false;
    }

    private bool updatedTipbox = true;
    private void Update()
    {
        if (mouseOver && Time.time > lastMouseOver + showDelay)
        {
            ((LevelUI)GameUI.Instance).ShowTooltip(Input.mousePosition, tooltip);
            updatedTipbox = false;
        }
        else if (!updatedTipbox)
        {
            updatedTipbox = true;
            ((LevelUI)GameUI.Instance).HideTooltip();
        }
    }
}
