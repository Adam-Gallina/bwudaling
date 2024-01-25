using Steamworks;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AchievmentController : MonoBehaviour
{
    #region Stat API names
    private const string Level = "highest_level";
    private const string WuvaLevel = "highest_level_wuva";
    private const string DogieLevel = "highest_level_dogie";
    private const string PiestLevel = "highest_level_piest";
    private const string BwudaLevel = "highest_level_bwuda";

    private const string CwabTime = "fastest_cwab";
    private const string WedTime = "fastest_wed";
    private const string TwentTime = "fastest_twent";
    #endregion
    #region Achievement API names
    private const string Level10 = "LEVEL_10_ANY";
    private const string Level20 = "LEVEL_20_ANY";
    private const string Level30 = "LEVEL_30_ANY";
    private const string Level15Wuva = "LEVEL_15_WUVA";
    private const string Level15Dogie = "LEVEL_15_DOGIE";
    private const string Level15Piest = "LEVEL_15_PIEST";
    private const string Level15Bwuda = "LEVEL_15_BWUDA";

    private const string SiwyCwab = "BOSS_SIWYCWAB";
    private const string BigWed = "BOSS_BIGWED";
    private const string Twent = "BOSS_TWENTY";

    private const string Favwit = "STATS_FAVWIT";
    #endregion
    #region UserPref names
    private const string FastestLvl1Time= "fastest_lvl1";
    private const string FastestClassTime = "fastest_any";
    private static string PrefTime(AvatarClass c) 
    {
        return BwudalingNetworkManager.Instance.currMaps.name + c;
    }
    #endregion

    #region Non-shirt Achievements
    private static bool b, w, u, d, a;
    public static bool BwudaUnlocked
    {
        get
        {
            if (BwudalingNetworkManager.Instance.DEBUG_ForceLockBwuda)
                return false;

            if (SteamManager.Initialized)
            {
                SteamUserStats.GetAchievement(Twent, out bool unlocked);
                return unlocked || (b && w && u && d && a);
            }
            else
                return b && w && u && d && a;
        }
    }
    #endregion

    #region Stat Values
    private static float GetTimeValue(string statName)
    {
        if (!SteamManager.Initialized) return -1;

        return SteamUserStats.GetStat(statName, out float value) ? value : -1;
    }

    public static float FastestCwab { get { return GetTimeValue(CwabTime); } }
    public static float FastestWed { get { return GetTimeValue(WedTime); } }
    public static float FastestTwent { get { return GetTimeValue(TwentTime); } }
    public static float GetCurrMapPackTime()
    {
        switch (BwudalingNetworkManager.Instance.currMaps.name)
        {
            case Constants.CwabName:
                return FastestCwab;
            case Constants.WedName:
                return FastestWed;
            case Constants.TwentName:
                return FastestTwent;
            default:
                Debug.LogError("Can't fetch best time for current map pack " + BwudalingNetworkManager.Instance.currMaps.name);
                return -1;
        }
    }

    public static float FastestWuva1 { get { return PlayerPrefs.GetFloat(PrefTime(AvatarClass.Wuva) + FastestLvl1Time, -1); } }
    public static float FastestWuvaAny { get { return PlayerPrefs.GetFloat(PrefTime(AvatarClass.Wuva) + FastestClassTime, -1); } }
    public static float FastestDogie1 { get { return PlayerPrefs.GetFloat(PrefTime(AvatarClass.Dogie) + FastestLvl1Time, -1); } }
    public static float FastestDogieAny { get { return PlayerPrefs.GetFloat(PrefTime(AvatarClass.Dogie) + FastestClassTime, -1); } }
    public static float FastestPiest1 { get { return PlayerPrefs.GetFloat(PrefTime(AvatarClass.Piest) + FastestLvl1Time, -1); } }
    public static float FastestPiestAny { get { return PlayerPrefs.GetFloat(PrefTime(AvatarClass.Piest) + FastestClassTime, -1); } }
    public static float FastestBwuda1 { get { return PlayerPrefs.GetFloat(PrefTime(AvatarClass.Bwuda) + FastestLvl1Time, -1); } }
    public static float FastestBwudaAny { get { return PlayerPrefs.GetFloat(PrefTime(AvatarClass.Bwuda) + FastestClassTime, -1); } }
    #endregion

    public static AchievmentController Instance { get; private set; }

    public static string DefaultShirtId { get; private set; }  = "col_red";


    [SerializeField] private ShirtData[] shirts = new ShirtData[0];
    public static Dictionary<string, ShirtData> Shirts = new Dictionary<string, ShirtData>();
    private Dictionary<string, string> AchievementShirts = new Dictionary<string, string>();

    protected Callback<UserStatsReceived_t> StatsReceived;
    protected Callback<UserAchievementStored_t> AchievementStored;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (ShirtData s in shirts)
        {
            if (s.achievementName != string.Empty)
            {
                if (AchievementShirts.ContainsKey(s.achievementName))
                    Debug.LogError($"Duplicate achievement id {s.achievementName}, skipping {s.name}");
                else
                    AchievementShirts.Add(s.achievementName, s.id);
            }

            if (Shirts.ContainsKey(s.id))
                Debug.LogError($"Duplicate shirt id {s.id}, skipping {s.name}");
            else
                Shirts.Add(s.id, s);
        }

        StatsReceived = Callback<UserStatsReceived_t>.Create(OnStatsReceived);
        AchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);

        if (SteamManager.Initialized)
            SteamUserStats.RequestCurrentStats();
    }

    private void OnStatsReceived(UserStatsReceived_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
            return;

        foreach (string a in AchievementShirts.Keys)
        {
            SteamUserStats.GetAchievement(a, out bool unlocked);
            ShirtData s = Shirts[AchievementShirts[a]];
            s.unlocked = unlocked;
            Shirts[AchievementShirts[a]] = s;
        }
    }

    private void OnAchievementStored(UserAchievementStored_t callback)
    {
        if (callback.m_nCurProgress == 0 && callback.m_nMaxProgress == 0)
        {
            if (!Shirts[AchievementShirts[callback.m_rgchAchievementName]].unlocked)
            {
                ShirtData s = Shirts[AchievementShirts[callback.m_rgchAchievementName]];
                s.unlocked = true;
                Shirts[AchievementShirts[callback.m_rgchAchievementName]] = s;
                if (s.previewImg)
                    ShirtNotification.Instance.SetNotification(s.previewImg);
            }
        }
    }

    public string[] GetUnlockedShirts()
    {
        List<string> ids = new List<string>();
        foreach (ShirtData s in Shirts.Values)
        {
            if (s.startUnlocked
                || s.achievementName != string.Empty && s.unlocked
                || BwudalingNetworkManager.Instance.DEBUG_UnlockAllShirts)
                ids.Add(s.id);
        }

        return ids.ToArray();
    }
    
    private void OnEnable()
    {
        AbilityLevels.OnAbilitiesLoaded += OnAbilitiesLoaded;
        BwudalingNetworkManager.OnSceneChanged += SaveStats;
        BasicGameController.OnMapStarted += OnMapStarted;
        BasicGameController.OnMapCompleted += OnMapCompleted;
    }

    private void OnDisable()
    {
        AbilityLevels.OnAbilitiesLoaded -= OnAbilitiesLoaded;
        BwudalingNetworkManager.OnSceneChanged -= SaveStats;
        BasicGameController.OnMapStarted -= OnMapStarted;
        BasicGameController.OnMapCompleted -= OnMapCompleted;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            b = true;
        }
        else if (b && Input.GetKeyDown(KeyCode.W))
        {
            w = true;
        }
        else if (w && Input.GetKeyDown(KeyCode.U))
        {
            u = true;
        }
        else if (u && Input.GetKeyDown(KeyCode.D))
        {
            d = true;
        }
        else if (d && Input.GetKeyDown(KeyCode.A))
        {
            a = true;
        }
    }

    private void OnAbilitiesLoaded()
    {
        AbilityLevels.LoadedAbilities.OnLevelUp += UpdateLevelUpStats;
    }

    private void UpdateStatIfLower(string stat, int val)
    {
        if (!SteamManager.Initialized) return;

        SteamUserStats.GetStat(stat, out int level);
        if (level < val)
            SteamUserStats.SetStat(stat, val);
    }
    private void UpdateLevelUpStats()
    {
        UpdateStatIfLower(Level, AbilityLevels.LoadedAbilities.vals.level + 1);
        switch (AbilityLevels.LoadedAbilities.vals.avatarClass)
        {
            case AvatarClass.Wuva:
                UpdateStatIfLower(WuvaLevel, AbilityLevels.LoadedAbilities.vals.level + 1);
                break;
            case AvatarClass.Dogie:
                UpdateStatIfLower(DogieLevel, AbilityLevels.LoadedAbilities.vals.level + 1);
                break;
            case AvatarClass.Piest:
                UpdateStatIfLower(PiestLevel, AbilityLevels.LoadedAbilities.vals.level + 1);
                break;
            case AvatarClass.Bwuda:
                UpdateStatIfLower(BwudaLevel, AbilityLevels.LoadedAbilities.vals.level + 1);
                break;
            default:
                Debug.LogError("Can't update stat for current class " + AbilityLevels.LoadedAbilities.vals.avatarClass.ToString());
                return;
        }

        // Immediately update and unlock corresponding achievements when reaching milestones
        switch (AbilityLevels.LoadedAbilities.vals.level + 1)
        {
            case 10:
            case 15:
            case 20:
            case 30:
                SaveStats();
                break;
        }
    }

    private void UpdatePrefIfLower(string pref, float time)
    {
        float lastTime = PlayerPrefs.GetFloat(pref, -1);
        Debug.Log(lastTime + " " + time);
        if (lastTime == -1 || time < lastTime)
            PlayerPrefs.SetFloat(pref, time);
    }
    private void OnMapStarted()
    {
        if (!SteamManager.Initialized) return;

        if (GameController.Instance.mapType == MapType.Boss)
        {

            // Check/Save fastest time for boss
            switch (((BossMapController)MapController.Instance).bossPrefab.bossName)
            {
                case Constants.CwabName:
                    //SteamUserStats.SetAchievement(SiwyCwab);
                    break;
                case Constants.WedName:
                    //SteamUserStats.SetAchievement(BigWed);
                    break;
                case Constants.TwentName:
                    //SteamUserStats.SetAchievement(Twent);
                    break;
                default:
                    //Debug.LogError("Can't update achievement for current boss " + ((BossMapController)MapController.Instance).bossPrefab.bossName);
                    return;
            }

            SaveStats();
        }
    }

    private void OnMapCompleted(NetworkPlayer p)
    {
        if (GameController.Instance.mapType == MapType.Boss)
        {
            UpdatePrefIfLower(PrefTime(BwudalingNetworkManager.Instance.ActivePlayer.gameAvatarClass) + FastestClassTime, BasicGameController.ElapsedTime);
            if (BwudalingNetworkManager.Instance.ActivePlayer.startedLevel1)
                UpdatePrefIfLower(PrefTime(BwudalingNetworkManager.Instance.ActivePlayer.gameAvatarClass) + FastestLvl1Time, BasicGameController.ElapsedTime);
        }

        if (!SteamManager.Initialized) return;

        if (GameController.Instance.mapType == MapType.Boss)
        {
            switch (((BossMapController)MapController.Instance).bossPrefab.bossName)
            {
                case Constants.CwabName:
                    SteamUserStats.SetAchievement(SiwyCwab);
                    break;
                case Constants.WedName:
                    SteamUserStats.SetAchievement(BigWed);
                    break;
                case Constants.TwentName:
                    SteamUserStats.SetAchievement(Twent);
                    break;
                default:
                    Debug.LogError("Can't update achievement for current boss " + ((BossMapController)MapController.Instance).bossPrefab.bossName);
                    return;
            }

            SaveStats();
        }
    }

    private void SaveStats()
    {
        if (SteamManager.Initialized)
            SteamUserStats.StoreStats();
    }
}

[System.Serializable]
public struct ShirtData
{
    public string name;
    public string id;
    public Material mat;

    public bool startUnlocked;
    public string achievementName;
    public Sprite previewImg;

    [HideInInspector] public bool unlocked;
}