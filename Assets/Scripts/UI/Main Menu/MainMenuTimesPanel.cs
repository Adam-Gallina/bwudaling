using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuTimesPanel : MonoBehaviour
{
    public string bossCategory;

    public TMPro.TMP_Text wuva1;
    public TMPro.TMP_Text wuvaAny;
    public TMPro.TMP_Text dogie1;
    public TMPro.TMP_Text dogieAny;
    public TMPro.TMP_Text piest1;
    public TMPro.TMP_Text piestAny;
    public TMPro.TMP_Text bwuda1;
    public TMPro.TMP_Text bwudaAny;

    public GameObject bwudaLine;

    private void Start()
    {
        SetText(wuva1, AchievmentController.PrefTime(bossCategory, AvatarClass.Wuva, AchievmentController.FastestLvl1Time), "Level 1: ");
        SetText(wuvaAny, AchievmentController.PrefTime(bossCategory, AvatarClass.Wuva, AchievmentController.FastestClassTime), "Any lvl: ");

        SetText(dogie1, AchievmentController.PrefTime(bossCategory, AvatarClass.Dogie, AchievmentController.FastestLvl1Time), "Level 1: ");
        SetText(dogieAny, AchievmentController.PrefTime(bossCategory, AvatarClass.Dogie, AchievmentController.FastestClassTime), "Any lvl: ");

        SetText(piest1, AchievmentController.PrefTime(bossCategory, AvatarClass.Piest, AchievmentController.FastestLvl1Time), "Level 1: ");
        SetText(piestAny, AchievmentController.PrefTime(bossCategory, AvatarClass.Piest, AchievmentController.FastestClassTime), "Any lvl: ");

        bwudaLine.SetActive(AchievmentController.BwudaUnlocked);
        SetText(bwuda1, AchievmentController.PrefTime(bossCategory, AvatarClass.Bwuda, AchievmentController.FastestLvl1Time), "Level 1: ");
        SetText(bwudaAny, AchievmentController.PrefTime(bossCategory, AvatarClass.Bwuda, AchievmentController.FastestClassTime), "Any lvl: ");
    }

    private void SetText(TMPro.TMP_Text text, string timePref, string prefix)
    {
        text.text = prefix + Constants.FormatRunTime(SpeedrunData.GetTime(timePref), true);
    }
}
