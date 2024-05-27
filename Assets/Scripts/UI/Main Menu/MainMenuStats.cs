using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MainMenuStats : MonoBehaviour
{
    public static MainMenuStats Instance;

    [Header("Fastest times")]
    [SerializeField] private StatsPage cwabTimes;
    [SerializeField] private StatsPage stwingTimes;
    [SerializeField] private StatsPage wedTimes;
    [SerializeField] private StatsPage twentTimes;

    [Header("Stats")]
    [SerializeField] private TMPro.TMP_Text distanceText;
    [SerializeField] private TMPro.TMP_Text deathsText;
    [SerializeField] private TMPro.TMP_Text revivesText;
    [SerializeField] private TMPro.TMP_Text haiwsText;
    [SerializeField] private TMPro.TMP_Text xpText;
    [SerializeField] private TMPro.TMP_Text levelsText;
    [SerializeField] private TMPro.TMP_Text dodgesText;
    [SerializeField] private TMPro.TMP_Text abilitiesText;

    protected Callback<UserStatsReceived_t> StatsReceived;

    private void Awake()
    {
        Instance = this;

        // Make sure button is lowered on start
        SetPanel(2);
        SetPanel(1);
        gameObject.SetActive(false);

        StatsReceived = Callback<UserStatsReceived_t>.Create(OnStatsReceived);

        if (AchievmentController.SteamStatsLoaded)
            UpdateStats();
    }

    private void OnStatsReceived(UserStatsReceived_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
            return;

        UpdateStats();
    }

    private void UpdateStats()
    {
        if (!SteamManager.Initialized) return;

        void SetTexti(TMPro.TMP_Text text, string stat, string pref, string suf)
        {
            SteamUserStats.GetStat(stat, out int n);
            text.text = $"{pref}: {n} {suf}";
        }

        SetTexti(distanceText, AchievmentController.TotalDistance, "Distance traveled", "bf");
        SetTexti(deathsText, AchievmentController.TotalDeaths, "Total deaths", "utter failures");
        SetTexti(revivesText, AchievmentController.TotalRevives, "Bwudalings revived", "helpin's");
        SetTexti(haiwsText, AchievmentController.TotalHaiws, "Haiws collected", "yummy snacks");
        SetTexti(xpText, AchievmentController.TotalXp, "XP earned", "xp");
        SetTexti(levelsText, AchievmentController.TotalLevels, "Levels gained", "new smakins");
        SetTexti(dodgesText, AchievmentController.TotalDodges, "Saws dodged", "close calls");
        SetTexti(abilitiesText, AchievmentController.TotalAbilities, "Abilities used", "");
    }

    public void SetPanel(int panel)
    {
        switch (panel)
        {
            case 1: 
                cwabTimes.Show();
                stwingTimes.Hide();
                wedTimes.Hide();
                twentTimes.Hide();
                break;
            case 2: 
                cwabTimes.Hide();
                stwingTimes.Show();
                wedTimes.Hide();
                twentTimes.Hide();
                break;
            case 3: 
                cwabTimes.Hide();
                stwingTimes.Hide();
                wedTimes.Show();
                twentTimes.Hide();
                break;
            case 4:
                cwabTimes.Hide();
                stwingTimes.Hide();
                wedTimes.Hide();
                twentTimes.Show();
                break;
            default:
                Debug.LogError("Could not swap to Stats menu " + panel);
                break;
        }
    }
}

[System.Serializable]
public class StatsPage
{
    public RectTransform button;
    public GameObject page;
    private Coroutine currRoutine;

    public void Show()
    {
        if (!page.activeSelf)
        {
            if (currRoutine != null)
                OptionsMenu.Instance.StopCoroutine(currRoutine);
            currRoutine = OptionsMenu.Instance.StartCoroutine(LowerBtn());
        }
        page.SetActive(true);
    }

    public void Hide()
    {
        if (page.activeSelf)
        {
            if (currRoutine != null)
                OptionsMenu.Instance.StopCoroutine(currRoutine);
            currRoutine = OptionsMenu.Instance.StartCoroutine(RaiseBtn());
        }
        page.SetActive(false);
    }

    private IEnumerator LowerBtn()
    {
        Vector3 pos = button.localPosition;
        float end = Time.time + 0.25f;
        while (Time.time < end)
        {
            float t = 1 - ((end - Time.time) / 0.25f);
            pos.x = -10 * t;
            button.localPosition = pos;

            yield return null;
        }

        pos.x = -10;
        button.localPosition = pos;
    }
    private IEnumerator RaiseBtn()
    {
        Vector3 pos = button.localPosition;
        float end = Time.time + 0.25f;
        while (Time.time < end)
        {
            float t = 1 - ((end - Time.time) / 0.25f);
            pos.x = -10 + 10 * t;
            button.localPosition = pos;

            yield return null;
        }

        pos.x = 0;
        button.localPosition = pos;
    }
}