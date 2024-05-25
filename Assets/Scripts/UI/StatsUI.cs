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

    [Header("Times")]
    [SerializeField] protected TMPro.TMP_Text currRunTitle;
    [SerializeField] protected TMPro.TMP_Text currRunTitleBkgd;
    [SerializeField] protected TMPro.TMP_Text currRunTime;
    [SerializeField] protected TMPro.TMP_Text bestAllRunTime;
    [SerializeField] protected TMPro.TMP_Text bestWuva1RunTime;
    [SerializeField] protected TMPro.TMP_Text bestDogie1RunTime;
    [SerializeField] protected TMPro.TMP_Text bestPiest1RunTime;
    [SerializeField] protected TMPro.TMP_Text bestBwuda1RunTime;
    [SerializeField] protected TMPro.TMP_Text bestWuvaRunTime;
    [SerializeField] protected TMPro.TMP_Text bestDogieRunTime;
    [SerializeField] protected TMPro.TMP_Text bestPiestRunTime;
    [SerializeField] protected TMPro.TMP_Text bestBwudaRunTime;

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

        bestBwudaRunTime.transform.parent.gameObject.SetActive(AchievmentController.BwudaUnlocked);
    }

    protected override void Start()
    {
        base.Start();

        DisplayRunTimes();
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
            if (!BwudalingNetworkManager.Instance.DEBUG_SkipStatsAnim)
                didiyenuAudio.Play();
            speedMod = .125f;

            if (winnerName == BwudalingNetworkManager.Instance.ActivePlayer.displayName)
                AchievmentController.Instance.AddStat(PlayerStatType.Best, 1);
        }
        smackAudio.gameObject.SetActive(speedMod == 1);

        if (!BwudalingNetworkManager.Instance.DEBUG_SkipStatsAnim)
            StartCoroutine(AnimStatDisplay(s.statName, w, y, speedMod, a, !playerWon));
    }

    private void DisplayRunTimes()
    {
        currRunTitle.text = currRunTitleBkgd.text = BwudalingNetworkManager.Instance.currMaps.name + " Time";

        currRunTime.text = Constants.FormatRunTime(BasicGameController.ElapsedTime, true);
        bestAllRunTime.text = Constants.FormatRunTime(AchievmentController.GetCurrMapPackTime(), true);

        bestWuva1RunTime.text = "Level 1: " + Constants.FormatRunTime(AchievmentController.FastestWuva1, true);
        bestDogie1RunTime.text = "Level 1: " + Constants.FormatRunTime(AchievmentController.FastestDogie1, true);
        bestPiest1RunTime.text = "Level 1: " + Constants.FormatRunTime(AchievmentController.FastestPiest1, true);
        bestBwuda1RunTime.text = "Level 1: " + Constants.FormatRunTime(AchievmentController.FastestBwuda1, true);

        bestWuvaRunTime.text = "Any lvl: " + Constants.FormatRunTime(AchievmentController.FastestWuvaAny, true);
        bestDogieRunTime.text = "Any lvl: " + Constants.FormatRunTime(AchievmentController.FastestDogieAny, true);
        bestPiestRunTime.text = "Any lvl: " + Constants.FormatRunTime(AchievmentController.FastestPiestAny, true);
        bestBwudaRunTime.text = "Any lvl: " + Constants.FormatRunTime(AchievmentController.FastestBwudaAny, true);
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