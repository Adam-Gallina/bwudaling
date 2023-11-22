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

    public static AchievmentController Instance { get; private set; }

    public static string DefaultShirtId = "col_red";

    [SerializeField] private string[] startingShirtIds = new string[] { DefaultShirtId };

    [System.Serializable]
    private struct AchievementShirt
    {
        public string achievementName;
        public string shirtID;
    }
    [SerializeField] private AchievementShirt[] unlockableShirts;
    private Dictionary<string, string> AchievementShirts = new Dictionary<string, string>();

    [SerializeField] private ShirtData[] shirts = new ShirtData[0];
    public static Dictionary<string, ShirtData> Shirts = new Dictionary<string, ShirtData>();

    protected Callback<UserStatsReceived_t> StatsReceived;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (AchievementShirt s in unlockableShirts)
        {
            if (AchievementShirts.ContainsKey(s.achievementName))
                Debug.LogError($"Duplicate shirt id {s.achievementName}, skipping {s.shirtID}");
            else
                AchievementShirts.Add(s.achievementName, s.shirtID);
        }

        foreach (ShirtData s in shirts)
        {
            if (Shirts.ContainsKey(s.id))
                Debug.LogError($"Duplicate shirt id {s.id}, skipping {s.name}");
            else
                Shirts.Add(s.id, s);
        }

        SteamUserStats.RequestCurrentStats();
    }

    public string[] GetUnlockedShirts()
    {
        List<string> ids = new List<string>();
        foreach (string s in startingShirtIds)
            ids.Add(s);

        foreach (string s in AchievementShirts.Keys)
            if (SteamUserStats.GetAchievement(s, out bool unlocked))
                if (unlocked)
                    ids.Add(AchievementShirts[s]);

        return ids.ToArray();
    }
    
    private void OnEnable()
    {
        AbilityLevels.OnAbilitiesLoaded += OnAbilitiesLoaded;
        BwudalingNetworkManager.OnSceneChanged += SaveStats;
        BasicGameController.OnMapCompleted += OnMapCompleted;
    }

    private void OnDisable()
    {
        AbilityLevels.OnAbilitiesLoaded -= OnAbilitiesLoaded;
        BwudalingNetworkManager.OnSceneChanged -= SaveStats;
        BasicGameController.OnMapCompleted -= OnMapCompleted;
    }

    private void OnAbilitiesLoaded()
    {
        AbilityLevels.LoadedAbilities.OnLevelUp += UpdateLevelUpStats;
    }

    private void UpdateStatIfLower(string stat, int val)
    {
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

    private void OnMapCompleted()
    {
        if (GameController.Instance.mapType == MapType.Boss)
        {
            switch (((BossMapController)MapController.Instance).bossPrefab.bossName)
            {
                case "Siwy Cwab":
                    SteamUserStats.SetAchievement(SiwyCwab);
                    break;
                case "Big Wed":
                    SteamUserStats.SetAchievement(BigWed);
                    break;
                case "Twenty":
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
        SteamUserStats.StoreStats();
    }
}

[System.Serializable]
public struct ShirtData
{
    public string name;
    public string id;
    public Material mat;
}