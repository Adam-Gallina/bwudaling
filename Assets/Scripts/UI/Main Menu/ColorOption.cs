using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorOption : MonoBehaviour
{
    [SerializeField] private Button btn;

    [HideInInspector] public bool available = true;

    private Color col;

    public void SetColor(Color col)
    {
        this.col = col;

        ColorBlock cols = new ColorBlock();
        cols.normalColor = col;
        cols.highlightedColor = col;
        cols.pressedColor = col;
        cols.selectedColor = col;
        cols.disabledColor = col;
        cols.colorMultiplier = 1;
        btn.colors = cols;
    }

    public void SelectOption()
    {
        foreach (NetworkPlayer p in BwudalingNetworkManager.Instance.Players)
        {
            if (p.hasAuthority)
            {
                p.SetAvatarColor(col);
                break;
            }
        }
    }

    public void Enable()
    {
        btn.interactable = true;
        btn.gameObject.SetActive(true);
        available = true;
    }
    public void Disable() 
    {
        btn.interactable = false;
        btn.gameObject.SetActive(false);
        available = false;
    }
}
