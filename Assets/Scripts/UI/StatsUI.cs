using Mirror;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class StatsUI : GameUI
{
    [SerializeField] private GameObject allStats;

    [SerializeField] private GameObject viewStatsBtn;

    [Header("Stats")]
    public StatDisplay distance;
    public StatDisplay abilities;
    public StatDisplay deaths;
    public StatDisplay heals;
    public StatDisplay dodges;
    public StatDisplay haiws;
    public StatDisplay favorite;
    [SerializeField] private TMPro.TMP_Text deathsListL;
    [SerializeField] private TMPro.TMP_Text deathsListR;

    [Header("Anim")]
    [SerializeField] private RectTransform stat1Block;
    [SerializeField] private TMPro.TMP_Text stat1text;
    [SerializeField] private TMPro.TMP_Text stat1textBkgd;
    [SerializeField] private RectTransform stat2Block;
    [SerializeField] private TMPro.TMP_Text stat2text;
    [SerializeField] private TMPro.TMP_Text stat2textBkgd;
    [SerializeField] private RectTransform winnerBlock;
    [SerializeField] private TMPro.TMP_Text winnerText;
    [SerializeField] private RectTransform playerBlock;
    [SerializeField] private TMPro.TMP_Text playerText;
    [SerializeField] private float slideTime;
    [SerializeField] private RangeF slideOffset;
    [SerializeField] private float targetOffset = -35;
    [SerializeField] private float dropTime;
    [SerializeField] private RangeF dropRotation;
    [SerializeField] private AudioSource smackAudio;
    [SerializeField] private AudioSource didiyenuAudio;

    private Animator anim;

    protected override void Awake()
    {
        base.Awake();

        anim = GetComponent<Animator>();

        stat1Block.gameObject.SetActive(true);
        stat2Block.gameObject.SetActive(true);
        allStats.gameObject.SetActive(false);
        viewStatsBtn.SetActive(false);
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
            case PlayerStatType.Best: return favorite;
            default:
                Debug.LogWarning("No UI set up for stat " + stat.ToString());
                return distance;
        }
    }

    public void SetStatDisplay(PlayerStatType stat, string winnerName, int winnerVal, int playerVal)
    {
        StatDisplay s = GetStatDisplay(stat);
        bool playerWon = winnerName == BwudalingNetworkManager.Instance.ActivePlayer.displayName;

        string w = playerWon ? s.SetWinnerText("You", winnerVal) : s.SetWinnerText(winnerName, winnerVal);
        string y = s.SetPlayerText("You", playerVal);

        Animator a = null;
        foreach (NetworkPlayer p in BwudalingNetworkManager.Instance.Players)
        {
            if (p.displayName == winnerName)
            {
                a = p.avatar.GetComponentInChildren<Animator>();
                break;
            }
        }

        float speedMod = 1;
        if (stat == PlayerStatType.Best)
        {
            playerWon = true;
            didiyenuAudio.Play();
            speedMod = .125f;
        }
        smackAudio.gameObject.SetActive(speedMod == 1);
        StartCoroutine(AnimStatDisplay(s.statName, w, y, speedMod, a, !playerWon));
    }

    public void Smackins()
    {
        smackAudio.Play();
    }

    private Animator lastAnimated = null;
    private bool statInPlace = true;
    private IEnumerator AnimStatDisplay(string statName, string winner, string player, float speedMod, Animator winnerAnim, bool showPlayer)
    {
        if (lastAnimated)
        {
            lastAnimated.SetBool("Dancing", false);
            lastAnimated = null;
        }

        statInPlace = false;
        stat1text.text = stat1textBkgd.text = statName;
        anim.SetTrigger("NextStat");
        anim.speed = speedMod;

        float rotDir = Mathf.Sign(Random.Range(-1, 1));
        float offset = slideOffset.RandomVal;
        float rot = dropRotation.RandomVal * rotDir;
        float startRot = Random.Range(-90, 90) + rot;
        float start = Time.time;
        while (Time.time < start + (slideTime * 1 / speedMod))
        {
            float t = (Time.time - start) / (slideTime * 1 / speedMod);
            float x = stat1Block.anchoredPosition.x;
            stat1Block.anchoredPosition = new Vector2(x, offset + (targetOffset - offset) * t);
            stat1Block.eulerAngles = new Vector3(0, 0, startRot + (rot - startRot) * t);

            yield return new WaitForEndOfFrame();
        }

        yield return new WaitUntil(() => statInPlace);
        stat2Block.eulerAngles = new Vector3(0, 0, rot);
        winnerBlock.eulerAngles = Vector3.zero;
        playerBlock.eulerAngles = Vector3.zero;

        stat2text.text = stat2textBkgd.text = statName;
        winnerText.text = winner;
        playerText.text = player;
        playerBlock.gameObject.SetActive(showPlayer);

        anim.SetTrigger("ShowWinner");

        
        if (winnerAnim)
        {
            winnerAnim.SetBool("Dancing", true);
            winnerAnim.SetInteger("Dance", 1);
            lastAnimated = winnerAnim;
        }


        float wRot = dropRotation.RandomVal * -rotDir;
        float yRot = dropRotation.RandomVal * rotDir;
        start = Time.time;
        while (Time.time < start + dropTime)
        {
            float wt = Mathf.Clamp01((Time.time - start) / (dropTime / 2));
            winnerBlock.eulerAngles = new Vector3(0, 0, wRot * wt);

            float yt = (Time.time - start) / dropTime;
            playerBlock.eulerAngles = new Vector3(0, 0, yRot * yt);

            yield return new WaitForEndOfFrame();
        }
    }
    public void StatBoxInPlace() { statInPlace = true; }

    public void HideStatDisplay()
    {
        /*stat1Block.gameObject.SetActive(false);
        stat2Block.gameObject.SetActive(false);
        allStats.gameObject.SetActive(true);*/

        viewStatsBtn.SetActive(true);

        ListDeathCounts();
    }

    private void ListDeathCounts()
    {
        List<int> deaths = new List<int>();
        List<string> names = new List<string>();

        foreach (NetworkPlayer p in BwudalingNetworkManager.Instance.Players)
        {
            int i;
            for (i = 0; i < deaths.Count; i++)
                if (deaths[i] > p.currPlayerStats.Deaths)
                    break;

            deaths.Insert(i, p.currPlayerStats.Deaths);
            names.Insert(i, p.displayName);
        }

        deathsListL.text = deathsListR.text = "";
        for (int i = 0; i < deaths.Count; i++)
        {
            if (i < BwudalingNetworkManager.Instance.Players.Count / 2)
                deathsListL.text += $"{names[i]}: {deaths[i]}\n";
            else
                deathsListL.text += $"{names[i]}: {deaths[i]}\n";
        }
    }
}