using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelect : MonoBehaviour
{
    private bool b, w, u, d, a;

    [SerializeField] private RectTransform saveBtnParent;
    [SerializeField] private Button loadSaveBtnPrefab;

    [SerializeField] private GameObject bwudaButton;

    [SerializeField] private Color selectedCol = new Color(1, 155/255, 0, 1);
    [SerializeField] private Color unselectedCol = Color.white;

    [SerializeField] private Button nextBtn;

    void Awake()
    {
        bwudaButton.SetActive(false);
    }

    private void Start()
    {
        AbilityLevels.CharacterSaves saves = AbilityLevels.CharSaves;
        for (int i = 0; i < saves.saveIDs.Count; i++)
            AddLoadBtn(saves.saveIDs[i], saves.classes[i], saves.levels[i]);

        if (BwudalingNetworkManager.Instance.mode != Mirror.NetworkManagerMode.Offline)
            PressLoad(AbilityLevels.LoadedAbilities.vals.id);
    }

    private void AddLoadBtn(int id, string avatar, int level)
    {        
        // Check if max chars have been created
        //int rows = lobbyPlayers.Count < BwudalingNetworkManager.Instance.maxConnections ? lobbyPlayers.Count + 1 : lobbyPlayers.Count;
        int rows = AbilityLevels.CharSaves.saveIDs.Count;
        saveBtnParent.sizeDelta = new Vector2(0, rows * 48 + 2);

        void SetButtonListener(Button b, int id)
        {
            b.onClick.AddListener(() => { PressLoad(id); });
        }

        Button b = Instantiate(loadSaveBtnPrefab, saveBtnParent);
        b.transform.GetChild(0).GetComponent<TMPro.TMP_Text>().text = avatar;
        b.transform.GetChild(1).GetComponent<TMPro.TMP_Text>().text = "Level " + (level + 1);
        b.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -2 - ((saveBtnParent.childCount - 1) * 48));

        SetButtonListener(b, id);
    }


    void Update()
    {
        CheckBwuda();
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

    #region Buttons
    private AvatarClass selectedAvatar = AvatarClass.None;
    public void SelectCharBtn(int avatar)
    {
        selectedAvatar = (AvatarClass)avatar;
        /*for (int i = 0; i < transform.childCount; i++)
        {
            ColorBlock cols = transform.GetChild(i).GetComponent<Button>().colors;
            cols.normalColor = i + 1 == avatar ? selectedCol : unselectedCol;
            transform.GetChild(i).GetComponent<Button>().colors = cols;
        }*/

        nextBtn.interactable = true;
    }
    private int saveID = -1;
    public void PressLoad(int save)
    {
        saveID = save;
        nextBtn.interactable = true;
    }

    public void PressNext()
    {
        NetworkPlayer p = BwudalingNetworkManager.Instance.ActivePlayer;

        if (selectedAvatar != AvatarClass.None)
        {
            AbilityLevels.CreateNewCharacter(selectedAvatar);
            AddLoadBtn(AbilityLevels.LoadedAbilities.vals.id, AbilityLevels.LoadedAbilities.vals.avatarClass.ToString(), AbilityLevels.LoadedAbilities.vals.level);
        }
        else if (saveID != -1)
        {
            AbilityLevels.LoadAbilities(saveID);
        }
        else
        {
            Debug.LogError("Trying to load no character");
            return;
        }

        selectedAvatar = AvatarClass.None;
        saveID = -1;
        p.LoadAvatar();
    }

    public void PressDelete()
    {

    }
    #endregion
}
