using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DanceSelect : MonoBehaviour
{
    private int currDance = 0;
    private string[] availableDances;
    public static string CurrDanceId { get { return danceData.id; } }
    private static DanceData danceData;

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

        /*if (BwudalingNetworkManager.Instance.mode != Mirror.NetworkManagerMode.Offline)
        {
            if (BwudalingNetworkManager.Instance.ActivePlayer.shirtTextureId == "")
                shirtData = AchievmentController.Shirts[savedShirt];
            else
                shirtData = AchievmentController.Shirts[BwudalingNetworkManager.Instance.ActivePlayer.shirtTextureId];

            for (int i = 0; i < availableShirts.Length; i++)
            {
                if (availableShirts[i] == shirtData.id)
                {
                    currShirt = i;
                    break;
                }
            }
        }
        else
        {*/
            danceData = AchievmentController.Dances[savedDance];
        //}
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
    }
}
