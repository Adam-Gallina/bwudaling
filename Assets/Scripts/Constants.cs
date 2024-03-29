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
        " ate a twenty haiw",
        " went swimming within 1 hour of eating",
        " died.",
        " was ugly in front of bwuda mudda",
        " was out-siwied by a cwab",
        " is no longer with us",
        " said Bwudaling was bad (it is)",
        " peeked the op",
        " forgot to breathe",
        " got rectangle foot",
        " was diagnosed with pearhead condition",
        " drew a 7",
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
        " was demoted",
        ". Oof.",
        " is a liiiiil stinky",
        " lost the game",
        "? What were you finkin?",
        " died a horrendous death",
        " sniffa'd a saw",
        " kinda sucks at this",
        " had a lil oopsy-poopsy",
        "'s fart wasn't just a fart",
        " went honk mimimimimi",
        " went honk shoooo",
        " fell apart",
        " is blowing chunks",
        " didn't give constructive criticism",
        " lost their growth mindset",
        " looks like an ogre",
        " got hurted",
        " said dogs are better than cats",
        " said a no-no word",
        " ate dirt",
        "...kinda cringe...",
        " got a lil dirt on their foot",
        " forgot to tie their shoelace",
        " pushed bugs to master",
        " posted thirst traps",
        " has the worst nickname",
        " got L'd on. LOSER",
        " peepeed their pantsies",
        " went out. BETTER LUCK NEXT TIME",
        " failed miserably",
        " died? You gonna cry about it?",
        "? AAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
        " was lost to the times"
    };
    public static float DeathBannerDuration = 2;

    public static float AvatarRadius = 0.5f;

    #region PlayerPrefs
    public const string PlayerNamePref = "PlayerName";
    public const string LastIpPref = "LastIP";
    #endregion

    #region Names
    public const string CwabName = "Siwy Cwab";
    public const string WedName = "Big Wed";
    public const string TwentName = "Twent";
    public const string RainbowName = "Rainbow String";
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
    public static GameScene TestMap = new GameScene(1, "Test Map");

    public static GameScene MainMenu = new GameScene(0, "Main Menu");
    public static GameScene Stats = new GameScene(10, "Stats Screen");

    public static GameScene Level1_1 = new GameScene(2, "Level 1-1");
    public static GameScene Level1_2 = new GameScene(3, "Level 1-2");
    public static GameScene Level1_3 = new GameScene(4, "Level 1-3");
    public static GameScene SiwyCwab = new GameScene(5, "Siwy Cwab Boss");

    public static GameScene Level2_1 = new GameScene(6, "Level 2-1");
    public static GameScene Level2_2 = new GameScene(7, "Level 2-2");
    public static GameScene Level2_3 = new GameScene(8, "Level 2-3");
    public static GameScene BigWed = new GameScene(14, "Big Wed Boss");

    public static GameScene Level3_1 = new GameScene(11, "Level 3-1");
    public static GameScene Level3_2 = new GameScene(12, "Level 3-2");
    public static GameScene Level3_3 = new GameScene(13, "Level 3-3");
    public static GameScene Twenty = new GameScene(9, "Twenty Boss");

    public static GameScene Level4_1 = new GameScene(15, "Level 4-1");
    public static GameScene Level4_2 = new GameScene(16, "Level 4-2");
    public static GameScene Level4_3 = new GameScene(17, "Level 4-3");
    public static GameScene RainbowString = new GameScene(18, "Rainbow String Boss");

    public static MapPack[] Maps = new MapPack[]
    {
        new MapPack("Wiggly", 1, MapPackType.Test, new GameScene[] { Level4_2 }),
        new MapPack("Debug", 0, MapPackType.Test, new GameScene[] { TestMap }),
        new MapPack("Debug Cwab", 0, MapPackType.Test, new GameScene[] { SiwyCwab }),
        new MapPack("Debug Wed", 0, MapPackType.Test, new GameScene[] { BigWed }),
        new MapPack("Debug Twent", 0, MapPackType.Test, new GameScene[] { Twenty }),
        new MapPack(CwabName, 1, MapPackType.Bwudaling, new GameScene[] { Level1_1, Level1_2, Level1_3, SiwyCwab }),
        new MapPack(RainbowName, 1, MapPackType.Bwudaling, new GameScene[] { Level4_1, Level4_2, Level4_3, RainbowString }),
        new MapPack(WedName, 2, MapPackType.Bwudaling, new GameScene[] { Level2_1, Level2_2, Level2_3, BigWed }),
        new MapPack(TwentName, 3, MapPackType.Bwudaling, new GameScene[] { Level3_1, Level3_2, Level3_3, Twenty }),
    };
    public static GameScene EndScreen = Stats;
    #endregion

    public static string FormatRunTime(float time, bool showHour = false)
    {
        if (time == -1)
            return "n/a";

        int ms = Mathf.RoundToInt(time % 1 * 1000);
        int s = (int)time;
        ms -= s;
        int m = s / 60;
        s %= 60;
        int h = m / 60;
        m %= 60;

        return h > 0 || showHour
            ? string.Format("{0:0}:{1:00}:{2:00}.{3:000}", h, m, s, ms)
            : string.Format("{0:0}:{1:00}.{2:000}", m, s, ms);
    }
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
public enum MapPackType { Test, Bwudaling }
public struct MapPack
{
    public string name;
    public int difficulty;
    public MapPackType packType;
    public GameScene[] maps;

    public MapPack(string name, int difficulty, MapPackType packType, GameScene[] maps)
    {
        this.name = name;
        this.difficulty = difficulty;
        this.packType = packType;
        this.maps = maps;
    }
}