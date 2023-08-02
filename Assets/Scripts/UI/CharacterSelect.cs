using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelect : MonoBehaviour
{
    private bool b, w, u, d;

    [SerializeField] private GameObject wuvaButton;
    [SerializeField] private GameObject dogieButton;
    [SerializeField] private GameObject piestButton;
    [SerializeField] private GameObject bwudaButton;

    void Awake()
    {
        bwudaButton.SetActive(false);

        wuvaButton.transform.GetChild(1).GetComponent<TMPro.TMP_Text>().text = "Level " + (AbilityLevels.LoadAbilities(AvatarClass.Wuva.ToString()).level + 1);
        dogieButton.transform.GetChild(1).GetComponent<TMPro.TMP_Text>().text = "Level " + (AbilityLevels.LoadAbilities(AvatarClass.Dogie.ToString()).level + 1);
        piestButton.transform.GetChild(1).GetComponent<TMPro.TMP_Text>().text = "Level " + (AbilityLevels.LoadAbilities(AvatarClass.Piest.ToString()).level + 1);
        bwudaButton.transform.GetChild(1).GetComponent<TMPro.TMP_Text>().text = "Level " + (AbilityLevels.LoadAbilities(AvatarClass.Bwuda.ToString()).level + 1);
    }

    public void SelectCharBtn(int avatar)
    {
        foreach (NetworkPlayer p in BwudalingNetworkManager.Instance.Players)
        {
            if (p.hasAuthority)
            {
                p.SetAvatar((AvatarClass)avatar);
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
            bwudaButton.SetActive(true);
        }
    }
}
