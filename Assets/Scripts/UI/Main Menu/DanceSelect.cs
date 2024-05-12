using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DanceSelect : MonoBehaviour
{
    private int currDance = 0;
    private string[] availableDances;
    public static string CurrDanceId { get { return danceData.id; } }
    private static DanceData danceData;

    [SerializeField] private TMPro.TMP_Text danceName;
    [SerializeField] private TMPro.TMP_Text danceNameOutline;

    private string savedDance
    {
        get
        {
            return PlayerPrefs.GetString("LastUsedDance", AchievmentController.DefaultDanceId);
        }
        set
        {
            PlayerPrefs.SetString("LastUsedDance", value);
        }
    }

    private void Start()
    {
        availableDances = AchievmentController.Instance.GetUnlockedDances();

        if (BwudalingNetworkManager.Instance.mode != Mirror.NetworkManagerMode.Offline)
        {
            if (BwudalingNetworkManager.Instance.ActivePlayer.danceId == "")
                danceData = AchievmentController.Dances[savedDance];
            else
                danceData = AchievmentController.Dances[BwudalingNetworkManager.Instance.ActivePlayer.danceId];

            for (int i = 0; i < availableDances.Length; i++)
            {
                if (availableDances[i] == danceData.id)
                {
                    currDance = i;
                    break;
                }
            }
        }
        else
        {
            danceData = AchievmentController.Dances[savedDance];
        }
        UpdateDanceDisplay();
    }

    public void NextDance()
    {
        currDance++;
        if (currDance >= availableDances.Length)
            currDance = 0;

        danceData = AchievmentController.Dances[availableDances[currDance]];
        UpdateDanceDisplay();
    }

    public void LastDance()
    {
        currDance--;
        if (currDance < 0)
            currDance = availableDances.Length - 1;


        danceData = AchievmentController.Dances[availableDances[currDance]];
        UpdateDanceDisplay();
    }

    private void UpdateDanceDisplay()
    {
        GameObject.Find("Shirt Preview Cam").GetComponent<MenuPlayerPreview>().SetDance(danceData.animId);
        savedDance = danceData.id;

        danceName.text = danceData.name;
        danceNameOutline.text = danceData.name;
    }
}
