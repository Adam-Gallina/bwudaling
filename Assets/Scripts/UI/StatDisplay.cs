using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatDisplay : MonoBehaviour
{
    public string statName;
    [SerializeField] private TMPro.TMP_Text winnerText;
    [SerializeField] private TMPro.TMP_Text loserText;
    [SerializeField] private TMPro.TMP_Text playerText;
    [SerializeField] private string statTemplate;

    public string SetWinnerText(string name, int stat)
    {
        if (winnerText)
            winnerText.text = FillTemplate(name, stat);

        return FillTemplate(name, stat);
    }

    public string SetLoserText(string name, int stat)
    {
        if (loserText)
            loserText.text = FillTemplate(name, stat);

        return FillTemplate(name, stat);
    }

    public string SetPlayerText(string name, int stat)
    {
        if (playerText)
            playerText.text = FillTemplate(name, stat);

        return FillTemplate(name, stat);
    }

    private string FillTemplate(string name, int stat)
    {
        return statTemplate.Replace("{p}", name).Replace("{s}", stat.ToString());
    }
}
