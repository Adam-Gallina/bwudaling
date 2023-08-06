using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsUI : GameUI
{
    [Header("Stats")]
    public StatDisplay distance;
    public StatDisplay abilities;
    public StatDisplay deaths;
    public StatDisplay heals;
    public StatDisplay dodges;
    public StatDisplay haiws;

    protected override void Awake()
    {
        base.Awake();

        distance.gameObject.SetActive(false);
        abilities.gameObject.SetActive(false);
        deaths.gameObject.SetActive(false);
        heals.gameObject.SetActive(false);
        dodges.gameObject.SetActive(false);
        haiws.gameObject.SetActive(false);
    }

    public StatDisplay GetStatDisplay(PlayerStatType stat)
    {
        switch (stat)
        {
            case PlayerStatType.Distance: return distance;
            case PlayerStatType.Abilities: return abilities;
            case PlayerStatType.Deaths: return deaths;
            case PlayerStatType.Heals: return heals;
            case PlayerStatType.Dodges: return dodges;
            case PlayerStatType.Haiws: return haiws;
            default:
                Debug.LogWarning("No UI set up for stat " + stat.ToString());
                return distance;
        }
    }

    public void SetStatDisplay(PlayerStatType stat, string winnerName, int winnerVal, int playerVal)
    {
        StartCoroutine(AnimStatDisplay(GetStatDisplay(stat), winnerName, winnerVal, playerVal));
    }

    private IEnumerator AnimStatDisplay(StatDisplay stat, string winnerName, int winnerVal, int playerVal)
    {
        stat.gameObject.SetActive(true);
        stat.SetWinnerText(winnerName, winnerVal);
        stat.SetPlayerText("You", playerVal);
        yield break;
    }

}