using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class AbilityLevels
{
    #region Ability Loading
    public static Abilities LoadAbilities(string prefsName)
    {
        return new Abilities(PlayerPrefs.GetInt(prefsName + Constants.LevelPref,            0), 
                             PlayerPrefs.GetInt(prefsName + Constants.TalentPointsPref,     TalentPointsPerLevel),
                             PlayerPrefs.GetInt(prefsName + Constants.XpPref,               0),
                             PlayerPrefs.GetInt(prefsName + Constants.SpeedLvlPref,         0),
                             PlayerPrefs.GetInt(prefsName + Constants.BoostLvlPref,         0),
                             PlayerPrefs.GetInt(prefsName + Constants.BoostMaxLvlPref,      0),
                             PlayerPrefs.GetInt(prefsName + Constants.BoostRechargeLvlPref, 0),
                             PlayerPrefs.GetInt(prefsName + Constants.Special1LvlPref,      -1),
                             PlayerPrefs.GetInt(prefsName + Constants.Special2LvlPref,      -1),
                             PlayerPrefs.GetInt(prefsName + Constants.Special3LvlPref,      -1));
    }

    public static void SaveAbilities(string prefsName, Abilities abilities)
    {
        PlayerPrefs.SetInt(prefsName + Constants.LevelPref,         abilities.level);
        PlayerPrefs.SetInt(prefsName + Constants.TalentPointsPref,  abilities.talentPoints);
        PlayerPrefs.SetInt(prefsName + Constants.XpPref,            abilities.currXp);
        PlayerPrefs.SetInt(prefsName + Constants.SpeedLvlPref,      abilities.speedLevel);
        PlayerPrefs.SetInt(prefsName + Constants.BoostLvlPref,      abilities.boostSpeedLevel);
        PlayerPrefs.SetInt(prefsName + Constants.BoostMaxLvlPref,   abilities.boostMaxLevel);
        PlayerPrefs.SetInt(prefsName + Constants.BoostRechargeLvlPref, abilities.boostRechargeLevel);
        PlayerPrefs.SetInt(prefsName + Constants.Special1LvlPref,   abilities.special1Level);
        PlayerPrefs.SetInt(prefsName + Constants.Special2LvlPref,   abilities.special2Level);
        PlayerPrefs.SetInt(prefsName + Constants.Special3LvlPref,   abilities.special3Level);
    }

    public static void ResetAbilities(string prefsName)
    {
        PlayerPrefs.SetInt(prefsName + Constants.LevelPref,         0);
        PlayerPrefs.SetInt(prefsName + Constants.TalentPointsPref,  TalentPointsPerLevel);
        PlayerPrefs.SetInt(prefsName + Constants.XpPref,            0);
        PlayerPrefs.SetInt(prefsName + Constants.SpeedLvlPref,      0);
        PlayerPrefs.SetInt(prefsName + Constants.BoostLvlPref,      0);
        PlayerPrefs.SetInt(prefsName + Constants.BoostMaxLvlPref,   0);
        PlayerPrefs.SetInt(prefsName + Constants.BoostRechargeLvlPref, 0);
        PlayerPrefs.SetInt(prefsName + Constants.Special1LvlPref,   -1);
        PlayerPrefs.SetInt(prefsName + Constants.Special2LvlPref,   -1);
        PlayerPrefs.SetInt(prefsName + Constants.Special3LvlPref,   -1);
    }
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
    public const float SpeedPerLevel = 0.5f;

    public static float CalcSpeed(int level)
    {
        return BaseSpeed + level * SpeedPerLevel;
    }
    #endregion

    #region Boost Speed
    public const float BaseBoostMod = 1.25f;
    public const float BoostModPerLevel = .05f;
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
        return BaseBoostMax + BoostMaxPerLevel;
    }
    #endregion

    #region Boost Recharge
    public const float BaseBoostRecharge = .5f;
    public const float BoostRechargePerLevel = .1f;
    public static float CalcBoostRecharge(int level)
    {
        return BaseBoostRecharge + BoostRechargePerLevel * level;
    }
    #endregion
}

public enum AbilityType { speed, boostSpeed, boostMax, boostRecharge, special1, special2, special3 };
public class Abilities
{
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
    public float BoostRechargeVal { get { return AbilityLevels.CalcBoostRecharge(boostRechargeLevel); } }
    public int boostRechargeLevel;

    public string special1Name;
    public string special1Tooltip;
    public Sprite special1Image;
    public string special2Name;
    public string special2Tooltip;
    public Sprite special2Image;
    public string special3Name;
    public string special3Tooltip;
    public Sprite special3Image;

    public int special1Level;
    public int special2Level;
    public int special3Level;

    public event Action OnAddXp;
    public event Action OnLevelUp;
    public event Action OnUpgrade;

    public Abilities(int level, int talentPoints, int currXp, int speedLevel, int boostLevel, int boostMaxLevel, int boostRechargeLevel, int special1Level, int special2Level, int special3Level)
    {
        this.level = level;
        this.talentPoints = talentPoints;
        this.currXp = currXp;
        nextXp = AbilityLevels.XpForNextLevel(level);

        this.speedLevel = speedLevel;

        this.boostSpeedLevel = boostLevel;
        this.boostMaxLevel = boostMaxLevel;
        this.boostRechargeLevel = boostRechargeLevel;

        this.special1Level = special1Level;
        this.special2Level = special2Level;
        this.special3Level = special3Level;
    }

    public void AddXp(int amount)
    {
        currXp += amount;

        if (currXp >= nextXp)
        {
            level += 1;
            talentPoints += AbilityLevels.TalentPointsPerLevel;

            currXp -= nextXp;
            nextXp = AbilityLevels.XpForNextLevel(level);

            OnLevelUp?.Invoke();

            if (currXp >= nextXp)
                AddXp(0);
        }

        OnAddXp?.Invoke();
    }

    private bool SpendTalentPoints(int amount)
    {
        if (talentPoints >= amount)
        {
            talentPoints -= amount;
            return true;
        }
        return false;
    }

    public void UpgradeAbility(AbilityType ability)
    {

        switch (ability)
        {
            case AbilityType.speed:
                speedLevel += SpendTalentPoints(AbilityLevels.BasicAbilityCost) ? 1 : 0;
                break;
            case AbilityType.boostSpeed:
                boostSpeedLevel += SpendTalentPoints(AbilityLevels.BasicAbilityCost) ? 1 : 0;
                break;
            case AbilityType.boostMax:
                boostMaxLevel += SpendTalentPoints(AbilityLevels.BasicAbilityCost) ? 1 : 0;
                break;
            case AbilityType.boostRecharge:
                boostRechargeLevel += SpendTalentPoints(AbilityLevels.BasicAbilityCost) ? 1 : 0;
                break;
            case AbilityType.special1:
                if (special1Level == AbilityLevels.SpecialAbilityMax)
                    return;
                special1Level += SpendTalentPoints(AbilityLevels.SpecialAbilityCost) ? 1 : 0;
                break;
            case AbilityType.special2:
                if (special2Level == AbilityLevels.SpecialAbilityMax)
                    return;
                special2Level += SpendTalentPoints(AbilityLevels.SpecialAbilityCost) ? 1 : 0;
                break;
            case AbilityType.special3:
                if (special3Level == AbilityLevels.SpecialAbilityMax)
                    return;
                special3Level += SpendTalentPoints(AbilityLevels.SpecialAbilityCost) ? 1 : 0;
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