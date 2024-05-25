using Steamworks;
using System;
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
    private const string StwingTime = "fastest_stwing";

    private const string TotalDistance = "total_distance";
    private const string TotalDeaths = "total_deaths";
    private const string TotalRevives = "total_revives";
    private const string TotalHaiws = "total_haiws";
    private const string TotalFavwit = "total_favwit";
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
    private const string FastSiwyCwab = "BOSS_FAST_SIWYCWAB";
    private const string BigWed = "BOSS_BIGWED";
    private const string FastWed = "BOSS_FAST_BIGWED";
    private const string Twent = "BOSS_TWENTY";
    private const string FastTwent = "BOSS_FAST_TWENTY";
    private const string Stwing = "BOSS_RAINBOWSTWING";
    private const string FastStwing = "BOSS_FAST_RAINBOWSTWING";

    private const string BwudaBirthday = "BWUDA_BIRTHDAY";
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

        return SteamUserStats.GetStat(statName, out float value) ? -value : -1;
    }

    public static float FastestCwab { get { return GetTimeValue(CwabTime); } }
    public static float FastestWed { get { return GetTimeValue(WedTime); } }
    public static float FastestTwent { get { return GetTimeValue(TwentTime); } }
    public static float FastestStwing { get { return GetTimeValue(StwingTime); } }
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
            case Constants.RainbowName:
                return FastestStwing;
            default:
                Debug.LogWarning("Can't fetch best time for current map pack " + BwudalingNetworkManager.Instance.currMaps.name);
                return -1;
        }
    }

    public static float FastestWuva1 { get { return SpeedrunData.GetTime(PrefTime(AvatarClass.Wuva) + FastestLvl1Time); } }
    public static float FastestWuvaAny { get { return SpeedrunData.GetTime(PrefTime(AvatarClass.Wuva) + FastestClassTime); } }
    public static float FastestDogie1 { get { return SpeedrunData.GetTime(PrefTime(AvatarClass.Dogie) + FastestLvl1Time); } }
    public static float FastestDogieAny { get { return SpeedrunData.GetTime(PrefTime(AvatarClass.Dogie) + FastestClassTime); } }
    public static float FastestPiest1 { get { return SpeedrunData.GetTime(PrefTime(AvatarClass.Piest) + FastestLvl1Time); } }
    public static float FastestPiestAny { get { return SpeedrunData.GetTime(PrefTime(AvatarClass.Piest) + FastestClassTime); } }
    public static float FastestBwuda1 { get { return SpeedrunData.GetTime(PrefTime(AvatarClass.Bwuda) + FastestLvl1Time); } }
    public static float FastestBwudaAny { get { return SpeedrunData.GetTime(PrefTime(AvatarClass.Bwuda) + FastestClassTime); } }
    #endregion

    public static AchievmentController Instance { get; private set; }

    public static string DefaultShirtId { get; private set; } = "col_red";
    public static string DefaultDanceId { get; private set; } = "dental_hygiene";


    [SerializeField] private ShirtData[] shirts = new ShirtData[0];
    public static Dictionary<string, ShirtData> Shirts = new Dictionary<string, ShirtData>();
    private Dictionary<string, string> AchievementShirts = new Dictionary<string, string>();

    [SerializeField] private DanceData[] dances = new DanceData[0];
    public static Dictionary<string, DanceData> Dances = new Dictionary<string, DanceData>();
    private Dictionary<string, string> AchievementDances = new Dictionary<string, string>();

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

        foreach (DanceData d in dances)
        {
            if (d.achievementName != string.Empty)
            {
                if (AchievementDances.ContainsKey(d.achievementName))
                    Debug.LogError($"Duplicate dance achievement id {d.achievementName}, skipping {d.name}");
                else
                    AchievementDances.Add(d.achievementName, d.id);
            }

            if (Dances.ContainsKey(d.id))
                Debug.LogError($"Duplicate dance id {d.id}, skipping {d.name}");
            else
                Dances.Add(d.id, d);
        }

        if (SteamManager.Initialized)
        {
            if (BwudalingNetworkManager.Instance.DEBUG_ResetAchievements)
            {
                SteamUserStats.ResetAllStats(true);
                Debug.LogWarning("Reset all user stats and achievements");
            }

            StatsReceived = Callback<UserStatsReceived_t>.Create(OnStatsReceived);
            AchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);
            SteamUserStats.RequestCurrentStats();
        }
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
            bool unlocked = false;
            if (AchievementShirts.ContainsKey(callback.m_rgchAchievementName))
            {
                unlocked = true;
                if (!Shirts[AchievementShirts[callback.m_rgchAchievementName]].unlocked)
                {
                    ShirtData s = Shirts[AchievementShirts[callback.m_rgchAchievementName]];
                    s.unlocked = true;
                    Shirts[AchievementShirts[callback.m_rgchAchievementName]] = s;
                    if (s.previewImg)
                        ShirtNotification.Instance.SetNotification(s.previewImg);
                }
            }
            if (AchievementDances.ContainsKey(callback.m_rgchAchievementName))
            {
                unlocked = true;
                if (!Dances[AchievementDances[callback.m_rgchAchievementName]].unlocked)
                {
                    DanceData d = Dances[AchievementDances[callback.m_rgchAchievementName]];
                    d.unlocked = true;
                    Dances[AchievementDances[callback.m_rgchAchievementName]] = d;
                }
            }

            if (!unlocked)
                Debug.LogWarning($"{callback.m_rgchAchievementName} was recieved but has no reward");
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
    
    public string[] GetUnlockedDances()
    {
        List<string> ids = new List<string>();
        foreach (DanceData d in Dances.Values)
        {
            if (d.startUnlocked
                || d.achievementName != string.Empty && d.unlocked
                || BwudalingNetworkManager.Instance.DEBUG_UnlockAllShirts)
                ids.Add(d.id);
        }

        return ids.ToArray();
    }

    private void OnEnable()
    {
        AbilityLevels.OnAbilitiesLoaded += OnAbilitiesLoaded;
        BwudalingNetworkManager.OnSceneChanged += SaveStats;
        BwudalingNetworkManager.OnClientConnected += CheckBwudaBirthday;
        BasicGameController.OnMapStarted += OnMapStarted;
        BasicGameController.OnMapCompleted += OnMapCompleted;
    }

    private void OnDisable()
    {
        AbilityLevels.OnAbilitiesLoaded -= OnAbilitiesLoaded;
        BwudalingNetworkManager.OnSceneChanged -= SaveStats;
        BwudalingNetworkManager.OnClientConnected -= CheckBwudaBirthday;
        BasicGameController.OnMapStarted -= OnMapStarted;
        BasicGameController.OnMapCompleted -= OnMapCompleted;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
            b = true;
        else if (b && Input.GetKeyDown(KeyCode.W))
            w = true;
        else if (w && Input.GetKeyDown(KeyCode.U))
            u = true;
        else if (u && Input.GetKeyDown(KeyCode.D))
            d = true;
        else if (d && Input.GetKeyDown(KeyCode.A))
            a = true;
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

    private void CheckBwudaBirthday()
    {
        SteamUserStats.GetAchievement(BwudaBirthday, out bool u);
        if (!u && DateTime.Now.Day == 19 && DateTime.Now.Month == 4)
        {
            SteamUserStats.SetAchievement(BwudaBirthday);
            SaveStats();
        }
    }

    public void AddStat(PlayerStatType stat, int amount)
    {
        if (!SteamManager.Initialized) return;

        int n;
        switch (stat)
        {
            case PlayerStatType.Deaths:
                SteamUserStats.GetStat(TotalDeaths, out n);
                SteamUserStats.SetStat(TotalDeaths, n + amount);
                break;
            case PlayerStatType.Heals:
                SteamUserStats.GetStat(TotalRevives, out n);
                SteamUserStats.SetStat(TotalRevives, n + amount);
                break;
            case PlayerStatType.Haiws:
                SteamUserStats.GetStat(TotalHaiws, out n);
                SteamUserStats.SetStat(TotalHaiws, n + amount);
                break;
            case PlayerStatType.Distance:
                SteamUserStats.GetStat(TotalDistance, out n);
                SteamUserStats.SetStat(TotalDistance, n + amount);
                break;
            case PlayerStatType.Best:
                SteamUserStats.GetStat(TotalFavwit, out n);
                SteamUserStats.SetStat(TotalFavwit, n + amount);
                break;
            default:
                Debug.LogWarning($"Stat {stat} is not currently tracked");
                break;
        }

        SaveStats();
    }

    private void UpdateSavedTimeIfLower(string key, float time)
    {
        double lastTime = SpeedrunData.GetTime(key);
        if (lastTime == -1 || time < lastTime)
            SpeedrunData.SetTime(key, time);
    }

    private void UpdateTimeStatIfLower(string stat, float time)
    {
        if (!SteamManager.Initialized) return;

        time = -time;

        SteamUserStats.GetStat(stat, out float bestTime);
        if (time > bestTime)
        {
            SteamUserStats.SetStat(stat, time);
            SaveStats();
        }
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
                case Constants.RainbowName:
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
            UpdateSavedTimeIfLower(PrefTime(BwudalingNetworkManager.Instance.ActivePlayer.gameAvatarClass) + FastestClassTime, BasicGameController.ElapsedTime);
            if (BwudalingNetworkManager.Instance.ActivePlayer.startedLevel1)
                UpdateSavedTimeIfLower(PrefTime(BwudalingNetworkManager.Instance.ActivePlayer.gameAvatarClass) + FastestLvl1Time, BasicGameController.ElapsedTime);
        }

        if (!SteamManager.Initialized) return;

        if (GameController.Instance.mapType == MapType.Boss)
        {
            switch (((BossMapController)MapController.Instance).bossPrefab.bossName)
            {
                case Constants.CwabName:
                    UpdateTimeStatIfLower(CwabTime, GameController.ElapsedTime);
                    SteamUserStats.SetAchievement(SiwyCwab);
                    break;
                case Constants.WedName:
                    UpdateTimeStatIfLower(WedTime, GameController.ElapsedTime);
                    SteamUserStats.SetAchievement(BigWed);
                    break;
                case Constants.TwentName:
                    UpdateTimeStatIfLower(TwentTime, GameController.ElapsedTime);
                    SteamUserStats.SetAchievement(Twent);
                    break;
                case Constants.RainbowName:
                    UpdateTimeStatIfLower(StwingTime, GameController.ElapsedTime);
                    SteamUserStats.SetAchievement(Stwing);
                    break;
                default:
                    Debug.LogWarning("Can't update achievement for current boss " + ((BossMapController)MapController.Instance).bossPrefab.bossName);
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

[System.Serializable]
public struct DanceData
{
    public string name;
    public string id;
    public int animId;

    public bool startUnlocked;
    public string achievementName;
    public Sprite previewImg;

    [HideInInspector] public bool unlocked;
}