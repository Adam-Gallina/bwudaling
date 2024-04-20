using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public static class AbilityLevels
{
    public const int SaveVersion = 1;
    public const int MaxCharacters = 10;

    #region Ability Loading
    private const string CharSaveFile = "CharSaves.bwuda";
    private const string CharAbilitySuffix = ".bwuda";

    [Serializable]
    public struct CharacterSaves
    {
        public int saveVersion;
        public int nextSaveID;
        public List<int> saveIDs;
        public List<string> classes;
        public List<int> levels;
    }
    private static bool loadedCharSaves = false;
    private static CharacterSaves charSaves;
    public static CharacterSaves CharSaves { 
        get {
            if (!loadedCharSaves)
            {
                LoadCharacterSaves();
                loadedCharSaves = true;
            }

            return charSaves;
        } 
    }

    public static Abilities LoadedAbilities { get; private set; }
    public static event Action OnAbilitiesLoaded;

    public static string UserID { get { return ManagerDebug.Instance.DEBUG_useKcpManager ? "kcp_player" : SteamUser.GetSteamID().ToString(); } }
    public static string UserPath { get { return Application.persistentDataPath + $"/{UserID}/"; } }

    public static bool LoadCharacterSaves()
    {
        if (File.Exists(UserPath + CharSaveFile))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream stream = new FileStream(UserPath + CharSaveFile, FileMode.Open);

            try
            {
                charSaves = (CharacterSaves)bf.Deserialize(stream);
            }
            catch (SerializationException e)
            {
                if (saveDataLoadFailed)
                    return false;

                stream.Close();
                saveDataLoadFailed = true;
                return RecoverCharSaveFile();
            }

            if (charSaves.saveVersion != SaveVersion)
            {
                throw new Exception($"File migration is not setup ({charSaves.saveVersion} -> {SaveVersion})");
            }

            stream.Close();
            saveDataLoadFailed = false;
            return true;
        }
        else
        {
            charSaves = new CharacterSaves();
            charSaves.saveVersion = SaveVersion;
            charSaves.saveIDs = new List<int>();
            charSaves.classes = new List<string>();
            charSaves.levels = new List<int>();

            BinaryFormatter bf = new BinaryFormatter();

            if (!Directory.Exists(UserPath + CharSaveFile))
                Directory.CreateDirectory(UserPath);
            FileStream stream = new FileStream(UserPath + CharSaveFile, FileMode.Create);

            bf.Serialize(stream, charSaves);
            stream.Close();

            return true;
        }
    }

    public static void SaveCharacterSaves()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream stream = new FileStream(UserPath + CharSaveFile, FileMode.Create);

        bf.Serialize(stream, CharSaves);
        stream.Close();
    }

    public static void CreateNewCharacter(AvatarClass avatarClass)
    {
        if (!loadedCharSaves)
            LoadCharacterSaves();

        if (LoadedAbilities != null)
            SaveAbilities();

        LoadedAbilities = new Abilities(new AbilityVals(++charSaves.nextSaveID, avatarClass, 0, TalentPointsPerLevel, 0, 0, 0, 0, /*0,*/ -1, -1, -1));
        CharSaves.saveIDs.Add(LoadedAbilities.vals.id);
        CharSaves.classes.Add(avatarClass.ToString());
        CharSaves.levels.Add(0);

        SaveCharacterSaves();
        SaveAbilities();

        OnAbilitiesLoaded?.Invoke();
    } 

    public static bool LoadAbilities(int id)
    {
        if (File.Exists(UserPath + id + CharAbilitySuffix))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream stream = new FileStream(UserPath + id.ToString() + CharAbilitySuffix, FileMode.Open);

            try
            {
                LoadedAbilities = new Abilities((AbilityVals)bf.Deserialize(stream));
                OnAbilitiesLoaded?.Invoke();
            }
            catch (SerializationException e) 
            {
                Debug.LogError("Could not read " + id + CharAbilitySuffix);
                return false;
            } 
            finally 
            { 
                stream.Close();
            }

            return true;
        }
        else
        {
            Debug.LogError("Trying to load non-existant character");

            return false;
        }
    }

    public static void SaveAbilities()
    {
        int i = CharSaves.saveIDs.IndexOf(LoadedAbilities.vals.id);
        if (i == -1)
        {
            Debug.LogWarning($"Trying to save non-existant Bwudaling (lvl {LoadedAbilities.vals.level} {LoadedAbilities.vals.avatarClass})");
            return;
        }

        if (LoadedAbilities.vals.level != CharSaves.levels[i])
        {
            charSaves.levels[i] = LoadedAbilities.vals.level;
            SaveCharacterSaves();
        }

        BinaryFormatter bf = new BinaryFormatter();
        FileStream stream = new FileStream(UserPath + LoadedAbilities.vals.id.ToString() + CharAbilitySuffix, FileMode.Create);

        bf.Serialize(stream, LoadedAbilities.vals);

        stream.Close();
    }

    public static void DeleteAbilities(int id)
    {
        File.Delete(UserPath + id + CharAbilitySuffix);

        if (!loadedCharSaves)
            LoadCharacterSaves();

        int i = charSaves.saveIDs.IndexOf(id);
        charSaves.saveIDs.RemoveAt(i);
        charSaves.classes.RemoveAt(i);
        charSaves.levels.RemoveAt(i);

        if (LoadedAbilities != null && id == LoadedAbilities.vals.id)
            LoadedAbilities = null;

        SaveCharacterSaves();
    }

    #region Save Data Recovery
    private static bool saveDataLoadFailed = false;
    private static bool RecoverCharSaveFile()
    {
        charSaves = new CharacterSaves();
        charSaves.saveVersion = SaveVersion;
        charSaves.saveIDs = new List<int>();
        charSaves.classes = new List<string>();
        charSaves.levels = new List<int>();

        foreach (string f in Directory.GetFiles(UserPath))
        {
            if (Path.GetExtension(f) != CharAbilitySuffix) continue;
            string fname = Path.GetFileNameWithoutExtension(f);
            
            if (!int.TryParse(fname, out int id)) continue;

            if (!LoadAbilities(id)) continue;

            if (charSaves.nextSaveID <= id)
                charSaves.nextSaveID = id + 1;

            charSaves.saveIDs.Add(id);
            charSaves.classes.Add(LoadedAbilities.vals.avatarClass.ToString());
            charSaves.levels.Add(LoadedAbilities.vals.level);
        }

        LoadedAbilities = null;

        // Pretend CharSaves was loaded successfully to attempt a normal save
        loadedCharSaves = true;
        SaveCharacterSaves();

        // Attempt to load CharSaves normally now
        loadedCharSaves = false;
        return LoadCharacterSaves();
    }
    #endregion
    #endregion

    #region Required XP Calculations
    public const int BaseXp = 5;
    public const float XpPow1 = 5;
    public const float XpPow2 = 0.1f;

    public static int XpForNextLevel(int level)
    {
        return (int)(BaseXp + XpPow1 * level + XpPow2 * level * level);
    }
    #endregion

    public const int TalentPointsPerLevel = 4;

    public const int BasicAbilityCost = 1;
    public const int SpecialAbilityCost = 3;

    public const int SpecialAbilityMax = 9;

    #region Speed
    public const float BaseSpeed = 5;
    public const float SpeedPerLevel = 0.25f;

    public static float CalcSpeed(int level)
    {
        return BaseSpeed + level * SpeedPerLevel;
    }

    public static int CalcSpeedAnim(float speed, bool running)
    {
        float level = speed / 15f;

        return (int)level + (running && level % 1 >= .25f ? 1 : 0);
    }
    #endregion

    #region Boost Speed
    public const float BaseBoostMod = 1.25f;
    public const float BoostModPerLevel = .035f;
    public static float CalcBoostSpeed(int level)
    {
        return BaseBoostMod + BoostModPerLevel * level;
    }
    #endregion

    #region Boost Max
    public const float BaseBoostMax = 2;
    public const float BoostMaxPerLevel = 0.25f;
    public static float CalcBoostMax(int level)
    {
        return BaseBoostMax + BoostMaxPerLevel * level;
    }
    #endregion

    #region Boost Recharge
    public const float BaseBoostRecharge = .5f;
    public const float BoostRechargePerLevel = .05f;
    public static float CalcBoostRecharge(int level)
    {
        return BaseBoostRecharge + BoostRechargePerLevel * level;
    }
    #endregion
}

public enum AbilityType { speed, boostSpeed, boostMax, boostRecharge, special1, special2, special3 };
[Serializable]
public struct AbilityVals
{
    public int id;
    public AvatarClass avatarClass;

    public int level;
    public int talentPoints;
    public int currXp;
    public int nextXp;

    public float SpeedVal { get { return AbilityLevels.CalcSpeed(speedLevel); } }
    public int speedLevel;

    public float BoostSpeedVal { get { return AbilityLevels.CalcBoostSpeed(boostSpeedLevel); } }
    public int boostSpeedLevel;
    public float BoostMaxVal { get { return AbilityLevels.CalcBoostMax(boostMaxLevel); } }
    public int boostMaxLevel;
    public float BoostRechargeVal { get { return AbilityLevels.CalcBoostRecharge(boostMaxLevel); } }
    //public int boostRechargeLevel;

    public int special1Level;
    public int special2Level;
    public int special3Level;

    public AbilityVals(int id, AvatarClass avatarClass, int level, int talentPoints, int currXp, int speedLevel, int boostLevel, int boostMaxLevel, /*int boostRechargeLevel,*/ int special1Level, int special2Level, int special3Level)
    {
        this.id = id;
        this.avatarClass = avatarClass;

        this.level = level;
        this.talentPoints = talentPoints;
        this.currXp = currXp;
        nextXp = AbilityLevels.XpForNextLevel(level);

        this.speedLevel = speedLevel;

        this.boostSpeedLevel = boostLevel;
        this.boostMaxLevel = boostMaxLevel;
        //this.boostRechargeLevel = boostRechargeLevel;

        this.special1Level = special1Level;
        this.special2Level = special2Level;
        this.special3Level = special3Level;
    }
}

public class Abilities
{
    public AbilityVals vals;

    public event Action OnAddXp;
    public event Action OnLevelUp;
    public event Action OnUpgrade;

    public Abilities(AbilityVals vals)
    {
        this.vals = vals;
    }

    public void AddXp(int amount)
    {
        vals.currXp += amount;

        if (vals.currXp >= vals.nextXp)
        {
            vals.level += 1;
            vals.talentPoints += AbilityLevels.TalentPointsPerLevel;

            vals.currXp -= vals.nextXp;
            vals.nextXp = AbilityLevels.XpForNextLevel(vals.level);

            OnLevelUp?.Invoke();

            if (vals.currXp >= vals.nextXp)
                AddXp(0);
        }

        OnAddXp?.Invoke();
    }

    private bool SpendTalentPoints(int amount)
    {
        if (vals.talentPoints >= amount)
        {
            vals.talentPoints -= amount;
            return true;
        }
        return false;
    }

    public void UpgradeAbility(AbilityType ability)
    {

        switch (ability)
        {
            case AbilityType.speed:
                vals.speedLevel += SpendTalentPoints(AbilityLevels.BasicAbilityCost) ? 1 : 0;
                break;
            case AbilityType.boostSpeed:
                vals.boostSpeedLevel += SpendTalentPoints(AbilityLevels.BasicAbilityCost) ? 1 : 0;
                break;
            case AbilityType.boostMax:
                vals.boostMaxLevel += SpendTalentPoints(AbilityLevels.BasicAbilityCost) ? 1 : 0;
                break;
            case AbilityType.boostRecharge:
                //boostRechargeLevel += SpendTalentPoints(AbilityLevels.BasicAbilityCost) ? 1 : 0;
                Debug.LogWarning("Trying to upgrade unused ability");
                break;
            case AbilityType.special1:
                if (vals.special1Level == AbilityLevels.SpecialAbilityMax)
                    return;
                vals.special1Level += SpendTalentPoints(AbilityLevels.SpecialAbilityCost) ? 1 : 0;
                break;
            case AbilityType.special2:
                if (vals.special2Level == AbilityLevels.SpecialAbilityMax)
                    return;
                vals.special2Level += SpendTalentPoints(AbilityLevels.SpecialAbilityCost) ? 1 : 0;
                break;
            case AbilityType.special3:
                if (vals.special3Level == AbilityLevels.SpecialAbilityMax)
                    return;
                vals.special3Level += SpendTalentPoints(AbilityLevels.SpecialAbilityCost) ? 1 : 0;
                break;
        }

        OnUpgrade?.Invoke();
    }
}

[System.Serializable]
public class AbilityUpgrade
{
    public float startValue;
    public float upgradeValue;

    public bool upgradeEveryLevel = false;
    public int[] upgradeLevels;

    private int GetUpgradeLevel(int level)
    {
        if (upgradeEveryLevel)
            return level;
        else if (upgradeLevels == null || upgradeLevels.Length == 0)
            return 0;

        if (level < upgradeLevels[0])
            return 0;
        else if (level == upgradeLevels[0] || upgradeLevels.Length == 1)
            return 1;

        for (int i = 1; i < upgradeLevels.Length; i++)
        {
            if (level < upgradeLevels[i])
                return i;
        }

        return upgradeLevels.Length;
    }

    public float CalcValue(int level)
    {
        return startValue + upgradeValue * GetUpgradeLevel(level);
    }
}