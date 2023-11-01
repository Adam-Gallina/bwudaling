using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelect : MonoBehaviour
{
    private bool b, w, u, d, a;

    [SerializeField] private GameObject wuvaButton;
    [SerializeField] private GameObject dogieButton;
    [SerializeField] private GameObject piestButton;
    [SerializeField] private GameObject bwudaButton;

    [SerializeField] private Color selectedCol = new Color(1, 155/255, 0, 1);
    [SerializeField] private Color unselectedCol = Color.white;

    [SerializeField] private Button nextBtn;

    void Awake()
    {
        bwudaButton.SetActive(false);

        wuvaButton.transform.GetChild(1).GetComponent<TMPro.TMP_Text>().text = "Level " + (AbilityLevels.LoadAbilities(AvatarClass.Wuva.ToString()).level + 1);
        dogieButton.transform.GetChild(1).GetComponent<TMPro.TMP_Text>().text = "Level " + (AbilityLevels.LoadAbilities(AvatarClass.Dogie.ToString()).level + 1);
        piestButton.transform.GetChild(1).GetComponent<TMPro.TMP_Text>().text = "Level " + (AbilityLevels.LoadAbilities(AvatarClass.Piest.ToString()).level + 1);
        bwudaButton.transform.GetChild(1).GetComponent<TMPro.TMP_Text>().text = "Level " + (AbilityLevels.LoadAbilities(AvatarClass.Bwuda.ToString()).level + 1);
    }

    private void Start()
    {
        if (BwudalingNetworkManager.Instance.mode != Mirror.NetworkManagerMode.Offline)
        {
            switch (BwudalingNetworkManager.Instance.ActivePlayer.avatar.AvatarName)
            {
                case "Bwuda Wuva":
                    SelectCharBtn(1);
                    break;
                case "Dogie":
                    SelectCharBtn(2);
                    break;
                case "Piest":
                    SelectCharBtn(3);
                    break;
                case "Bwuda":
                    b = w = u = d = a = true;
                    SelectCharBtn(4);
                    break;
                default:
                    Debug.LogWarning("Failed to set previously selected class " + BwudalingNetworkManager.Instance.ActivePlayer.avatar.AvatarName);
                    break;
            }
        }
    }

    public void SelectCharBtn(int avatar)
    {
        foreach (NetworkPlayer p in BwudalingNetworkManager.Instance.Players)
        {
            if (p.hasAuthority)
            {
                p.SetAvatar((AvatarClass)avatar);

                for (int i = 0; i < transform.childCount; i++) {
                    ColorBlock cols = transform.GetChild(i).GetComponent<Button>().colors;
                    cols.normalColor = i+1 == avatar ? selectedCol : unselectedCol;
                    transform.GetChild(i).GetComponent<Button>().colors = cols;
                }

                nextBtn.interactable = true;

                break;
            }
        }
    }

    void Update()
    {
        CheckBwuda();

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
        {
            foreach (NetworkPlayer p in BwudalingNetworkManager.Instance.Players)
            {
                if (p.hasAuthority)
                {
                    AbilityLevels.ResetAbilities(p.gameAvatarClass.ToString());
                    p.SetAvatar(p.gameAvatarClass);
                    break;
                }
            }
        }
    }

    private void CheckBwuda()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            b = true;
        }
        else if (b && Input.GetKeyDown(KeyCode.W))
        {
            w = true;
        }
        else if (w && Input.GetKeyDown(KeyCode.U))
        {
            u = true;
        }
        else if (u && Input.GetKeyDown(KeyCode.D))
        {
            d = true;
        }
        else if (d && Input.GetKeyDown(KeyCode.A))
        {
            a = true;
        }

        if (b && w && u && d && a)
            bwudaButton.SetActive(true);
    }
}
