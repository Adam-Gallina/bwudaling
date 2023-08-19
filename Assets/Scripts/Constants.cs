using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AvatarClass { None, Wuva, Dogie, Piest, Bwuda }
public class Constants
{
    public static string RandomDeathPhrase { get { return DeathPhrases[Random.Range(0, DeathPhrases.Length)]; } }
    public static string[] DeathPhrases = new string[]
    {
        " was splattered",
        " tripped",
        " exploded",
        " stubbed their lil toe",
        " foughtaboutit",
        " ate too many bwuda haiws",
        " got a biiiiiig smackin",
        " was sent to the outside box",
        " died tragically",
        " got too close to twent",
        " said NEUMNEUMNEUMNEUM to an infant",
        " was eviscerated",
        " is very handsome (and dead)",
        " smelled a bwuda fart",
        " ate too spicy ketchup",
        " walked headfirst into a saw",
        " went kaboom",
        " is now shaking their head frantically",
        " forgot this wasn't runling",                // This one might need to be taken out in public releases
        " ate a twenty haiw",
        " went swimming within 1 hour of eating",
        " died.",
        " was ugly in front of bwuda mudda",
        " was out-sillied by a cwab",
        " is no longer with us",
        " said Bwudaling was bad (it is)",
        " wished they were James Bond",                // This one might need to be taken out in public releases
        " peeked the op",
        " forgot to breathe",
        " got rectangle foot",
        " was diagnosed with pearhead condition",
        " got rejected by the pung's",                 // This one might need to be taken out in public releases
        " drew a 7",
        " missed BeReal time",
        " was bonked by the sock rock",
        " was discovered to have acorn head",
        " was hit by a llama",
        " recieved a sniffa'd nippa",
        " died. What the stink???",
        "'s poop was busted",
        " is simply a wubber",
        " is such a silly wittle goose",
        " is a stinkin shrub",
        " had to look at Big Wed's memes",
        " was demoted"
    };
    public static float DeathBannerDuration = 2;

    public static float AvatarRadius = 0.5f;

    #region PlayerPrefs
    public const string PlayerNamePref = "PlayerName";
    public const string LastIpPref = "LastIP";
    public const string LevelPref = "Level";
    public const string TalentPointsPref = "TPoints";
    public const string XpPref = "XP";
    public const string SpeedLvlPref = "SpeedLvl";
    public const string BoostLvlPref = "BLvl";
    public const string BoostMaxLvlPref = "BMaxLvl";
    public const string BoostRechargeLvlPref = "BRechargeLvl";
    public const string Special1LvlPref = "Sp1Lvl";
    public const string Special2LvlPref = "Sp2Lvl";
    public const string Special3LvlPref = "Sp3Lvl";
    #endregion

    #region Avatar Resources
    public const string AvatarPrefabFolder = "Prefabs/Avatars";
    public const string WuvaPrefab = "Wuva Avatar";
    public const string DogiePrefab = "Dogie Avatar";
    public const string PiestPrefab = "Piest Avatar";
    public const string BwudaPrefab = "Bwuda Avatar";

    public static PlayerAvatar LoadAvatarPrefab(AvatarClass avatar) 
    {
        switch (avatar)
        {
            case AvatarClass.Wuva:
                return Resources.Load<PlayerAvatar>(AvatarPrefabFolder + "/" + WuvaPrefab);
            case AvatarClass.Dogie:
                return Resources.Load<PlayerAvatar>(AvatarPrefabFolder + "/" + DogiePrefab);
            case AvatarClass.Piest:
                return Resources.Load<PlayerAvatar>(AvatarPrefabFolder + "/" + PiestPrefab);
            case AvatarClass.Bwuda:
                return Resources.Load<PlayerAvatar>(AvatarPrefabFolder + "/" + BwudaPrefab);
        }

        Debug.LogError("Could not load " + avatar + " avatar");
        return null;
    }
    #endregion

    #region Layers
    public const int HazardBoundaryLayer = 6;
    public const int PlayerLayer = 7;
    public const int EnvironmentLayer = 8;
    public const int HazardLayer = 9;
    public const int PlayerProjectileLayer = 10;
    public const int GroundLayer = 11;
    public const int TeleportAreaLayer = 14;
    public const int SafeAreaLayer = 15;
    #endregion

    #region Scenes
    public static GameScene MainMenu = new GameScene(0, "Main Menu");
    public static GameScene TestMap = new GameScene(1, "Test Map");
    public static GameScene Level1_1 = new GameScene(2, "Level 1-1");
    public static GameScene Level1_2 = new GameScene(3, "Level 1-2");
    public static GameScene Level1_3 = new GameScene(4, "Level 1-3");
    public static GameScene SiwyCwab = new GameScene(5, "Siwy Cwab Boss");
    public static GameScene Level2_1 = new GameScene(6, "Level 2-1");
    public static GameScene Level2_2 = new GameScene(7, "Level 2-2");
    public static GameScene Level2_3 = new GameScene(8, "Level 2-3");
    public static GameScene BigWed = new GameScene(9, "Twenty Boss");
    public static GameScene Level3_1 = new GameScene(11, "Level 3-1");
    public static GameScene Level3_2 = new GameScene(12, "Level 3-2");
    public static GameScene Level3_3 = new GameScene(13, "Level 3-3");
    public static GameScene Twenty = new GameScene(9, "Twenty Boss");
    public static GameScene Stats = new GameScene(10, "Stats Screen");

    public static GameScene[] Maps = new GameScene[] 
    { 
        Level1_1, Level1_2, Level1_3, SiwyCwab, 
        Level2_1, Level2_2, Level2_3, BigWed, 
        Level3_1, Level3_2, Level3_3, Twenty 
    };
    public static GameScene[] DebugMaps = new GameScene[] { Level3_2 };
    public static GameScene EndScreen = Stats;
    #endregion
}

public struct GameScene
{
    public int buildIndex;
    public string name;

    public GameScene(int index, string name)
    {
        buildIndex = index;
        this.name = name;
    }
}