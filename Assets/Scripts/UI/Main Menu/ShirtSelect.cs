using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ShirtSelect : MonoBehaviour
{
    private int currShirt = 0;
    private string[] availableShirts;
    public static string CurrShirtId { get { return shirtData.id; } }
    private static ShirtData shirtData;

    [SerializeField] private TMPro.TMP_Text shirtName;
    [SerializeField] private TMPro.TMP_Text shirtNameOutline;

    private string savedShirt
    {
        get
        {
            return PlayerPrefs.GetString("LastUsedShirt", AchievmentController.DefaultShirtId);
        }
        set
        {
            PlayerPrefs.SetString("LastUsedShirt", value);
        }
    }

    private void Start()
    {
        shirtData = AchievmentController.Shirts[savedShirt];
        UpdateShirtDisplay();
    }

    private void CheckLoadShirts()
    {
        if (availableShirts == null)
        {
            availableShirts = AchievmentController.Instance.GetUnlockedShirts();

            if (BwudalingNetworkManager.Instance.mode != Mirror.NetworkManagerMode.Offline)
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
        }
    }

    public void NextShirt()
    {
        CheckLoadShirts();

        currShirt++;
        if (currShirt >= availableShirts.Length)
            currShirt = 0;

        shirtData = AchievmentController.Shirts[availableShirts[currShirt]];
        UpdateShirtDisplay();
    }

    public void LastShirt()
    {
        CheckLoadShirts();

        currShirt--;
        if (currShirt < 0)
            currShirt = availableShirts.Length - 1;


        shirtData = AchievmentController.Shirts[availableShirts[currShirt]];
        UpdateShirtDisplay();
    }

    private void UpdateShirtDisplay()
    {
        Material m = shirtData.mat;
        GameObject.Find("Shirt Preview Cam").GetComponent<MenuPlayerPreview>().SetMaterial(m);

        savedShirt = shirtData.id;

        shirtName.text = shirtData.name;
        shirtNameOutline.text = shirtData.name;
    }
}
