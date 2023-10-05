using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ColorSelect : MonoBehaviour
{
    public static ColorSelect Instance;

    [SerializeField] private ColorOption colorBtnPrefab;
    [SerializeField] private Transform btnParent;

    [SerializeField] private Color[] colorOptions;
    private Dictionary<Color, ColorOption> colorBtns;

    [SerializeField] private float btnSize;
    [SerializeField] private float btnOffset;
    [SerializeField] private float btnHeight;

    private void Awake()
    {
        Instance = this;

        SpawnButtons();
    }

    private void SpawnButtons()
    {
        if (colorBtns != null)
        {
            colorBtns.Values.ToList().ForEach((ColorOption o) => { o.Enable(); });
            return;
        }

        colorBtns = new Dictionary<Color, ColorOption>();

        for (int c = 0; c < colorOptions.Length; c++)
        {
            ColorOption b = Instantiate(colorBtnPrefab, btnParent);
            b.GetComponent<RectTransform>().anchoredPosition = new Vector2((c % 2 == 0 ? -1 : 1) * btnOffset, -btnHeight * (c / 2));
            b.SetColor(colorOptions[c]);
            b.Enable();
            colorBtns.Add(colorOptions[c], b);
        }
    }

    public Color GetNextAvailableColor()
    {
        if (colorBtns == null)
            SpawnButtons();

        foreach (Color c in colorOptions)
        {
            if (colorBtns[c].available)
            {
                colorBtns[c].Disable();
                return c;
            }
        }

        return Color.white;
    }

    public void PlayerSelectedColor(Color o, Color n)
    {
        if (o != Color.white)
            colorBtns[o].Enable();
        if (colorBtns.ContainsKey(n))
            colorBtns[n].Disable();
    }
}
