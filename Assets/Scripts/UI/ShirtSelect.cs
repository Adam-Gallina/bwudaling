using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ShirtSelect : MonoBehaviour
{
    private int currShirt = 0;
    private string[] availableShirts;
    public string CurrShirtId { get { return shirtData.id; } }
    private ShirtData shirtData;

    private void Start()
    {
        availableShirts = AchievmentController.Instance.GetUnlockedShirts();

        if (BwudalingNetworkManager.Instance.mode != Mirror.NetworkManagerMode.Offline)
        {
            if (BwudalingNetworkManager.Instance.ActivePlayer.shirtTextureId == "")
                shirtData = AchievmentController.Shirts[AchievmentController.DefaultShirtId];
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
        {
            shirtData = AchievmentController.Shirts[AchievmentController.DefaultShirtId];
        }
        UpdateShirtDisplay();
    }

    public void NextShirt()
    {
        currShirt++;
        if (currShirt >= availableShirts.Length)
            currShirt = 0;

        shirtData = AchievmentController.Shirts[availableShirts[currShirt]];
        UpdateShirtDisplay();
        BwudalingNetworkManager.Instance.ActivePlayer.SetAvatarShirt(shirtData.id);
    }

    public void LastShirt()
    {
        currShirt--;
        if (currShirt < 0)
            currShirt = availableShirts.Length - 1;


        shirtData = AchievmentController.Shirts[availableShirts[currShirt]];
        UpdateShirtDisplay();
        BwudalingNetworkManager.Instance.ActivePlayer.SetAvatarShirt(shirtData.id);
    }

    private void UpdateShirtDisplay()
    {
        Material m = shirtData.mat;
        GameObject.Find("Preview Cam").GetComponent<MenuPlayerPreview>().SetMaterial(m);
    }
}
