using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelect : MonoBehaviour
{
    [SerializeField] private RectTransform createBtn;
    [SerializeField] private GameObject classUnlockIndicator;

    [SerializeField] private RectTransform saveBtnParent;
    [SerializeField] private Button loadSaveBtnPrefab;
    private List<Button> loadBtns = new List<Button>();

    [Header("Delete Char")]
    [SerializeField] private GameObject deleteConfirm;
    [SerializeField] private TMPro.TMP_Text deleteText;

    [SerializeField] private Transform classBtnParent;
    [SerializeField] private GameObject bwudaButton;

    [SerializeField] private Color selectedCol = new Color(1, 155/255, 0, 1);
    [SerializeField] private Color unselectedCol = Color.white;

    [SerializeField] private Button nextBtn;
    [SerializeField] private Button deleteBtn;

    [SerializeField] private GameObject loadCharError;
    [SerializeField] private TMPro.TMP_Text loadCharErrorText;

    void Awake()
    {
        bwudaButton.SetActive(false);
        deleteConfirm.SetActive(false);
        loadCharError.SetActive(false);
    }

    private void Start()
    {
        bool newCharUnlocked = PlayerPrefs.GetInt("ShownBwudaUnlockIndicator", 0) == 0 && AchievmentController.BwudaUnlocked;

        AbilityLevels.CharacterSaves saves = AbilityLevels.CharSaves;
        for (int i = 0; i < saves.saveIDs.Count; i++)
        {
            AddLoadBtn(saves.saveIDs[i], saves.classes[i], saves.levels[i]);

            if (saves.classes[i] == "Bwuda")
                newCharUnlocked = false;
        }

        classUnlockIndicator.SetActive(newCharUnlocked);
    }

    private void AddLoadBtn(int id, string avatar, int level)
    {
        int rows = AbilityLevels.CharSaves.saveIDs.Count + 1;
        saveBtnParent.sizeDelta = new Vector2(0, rows * 43 + 2);

        if (AbilityLevels.CharSaves.saveIDs.Count < AbilityLevels.MaxCharacters)
        {
            createBtn.GetComponentInChildren<TMPro.TMP_Text>().text = "Create new Bwudaling";
            createBtn.GetComponent<Button>().interactable = true;
        }
        else
        {
            createBtn.GetComponentInChildren<TMPro.TMP_Text>().text = "All Bwudaling slots used";
            createBtn.GetComponent<Button>().interactable = false;
        }
        createBtn.anchoredPosition = new Vector2(0, -2 - ((rows - 1) * 43));

        void SetButtonLoadListener(Button b, int id, int i)
        {
            b.onClick.AddListener(() => { PressLoad(id, i); });
        }

        Button b = Instantiate(loadSaveBtnPrefab, saveBtnParent);
        b.transform.GetChild(0).GetComponent<TMPro.TMP_Text>().text = avatar;
        b.transform.GetChild(1).GetComponent<TMPro.TMP_Text>().text = "Level " + (level + 1);
        b.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -2 - ((saveBtnParent.childCount - 2) * 43));

        SetButtonLoadListener(b, id, b.transform.GetSiblingIndex() - 1);

        loadBtns.Add(b);
        if (BwudalingNetworkManager.Instance.mode != Mirror.NetworkManagerMode.Offline && AbilityLevels.LoadedAbilities != null && id == AbilityLevels.LoadedAbilities.vals.id)
            PressLoad(AbilityLevels.LoadedAbilities.vals.id, b.transform.GetSiblingIndex() - 1);
    }


    void Update()
    {
        CheckBwuda();
    }

    private void CheckBwuda()
    {
        if (!bwudaButton.activeSelf)
            bwudaButton.SetActive(AchievmentController.BwudaUnlocked);
    }

    #region Buttons
    private AvatarClass selectedAvatar = AvatarClass.None;
    public void SelectCharBtn(int avatar)
    {
        selectedAvatar = (AvatarClass)avatar;
        saveID = -1;
        for (int i = 0; i < classBtnParent.childCount; i++)
        {
            ColorBlock cols = classBtnParent.GetChild(i).GetComponent<Button>().colors;
            cols.normalColor = i + 1 == avatar ? selectedCol : unselectedCol;
            cols.normalColor = i + 1 == avatar ? selectedCol : unselectedCol;
            cols.selectedColor = i + 1 == avatar ? selectedCol : unselectedCol;
            classBtnParent.GetChild(i).GetComponent<Button>().colors = cols;
        }

        nextBtn.interactable = true;

        if (selectedAvatar == AvatarClass.Bwuda)
            PlayerPrefs.SetInt("ShownBwudaUnlockIndicator", 1);
    }
    private int saveID = -1;
    public void PressLoad(int id, int btn)
    {
        for (int i = 0; i < loadBtns.Count; i++)
        {
            ColorBlock cols = loadBtns[i].colors;
            cols.normalColor = i == btn ? selectedCol : unselectedCol;
            cols.selectedColor = i == btn ? selectedCol : unselectedCol;
            loadBtns[i].colors = cols;
        }

        selectedAvatar = AvatarClass.None;
        saveID = id;
        nextBtn.interactable = true;
        deleteBtn.interactable = true;
    }

    public void PressNext()
    {
        NetworkPlayer p = BwudalingNetworkManager.Instance.ActivePlayer;

        if (selectedAvatar != AvatarClass.None)
        {
            AbilityLevels.CreateNewCharacter(selectedAvatar);
            AddLoadBtn(AbilityLevels.LoadedAbilities.vals.id, AbilityLevels.LoadedAbilities.vals.avatarClass.ToString(), AbilityLevels.LoadedAbilities.vals.level);
            selectedAvatar = AvatarClass.None;
            saveID = AbilityLevels.LoadedAbilities.vals.id;
        }
        else if (saveID != -1)
        {
            if (!AbilityLevels.LoadAbilities(saveID))
            {
                loadCharError.SetActive(true);
                loadCharErrorText.text = $"Error: Could not load level {AbilityLevels.LoadedAbilities.vals.level} {AbilityLevels.LoadedAbilities.vals.avatarClass}. File is likely corrupted, try selecting a different character :(";
            }
        }
        else
        {
            Debug.LogError("Trying to load no character");
            return;
        }

        p.LoadAvatar();
        GameObject.Find("Shirt Preview Cam").GetComponent<MenuPlayerPreview>().SetClass(AbilityLevels.LoadedAbilities.vals.avatarClass);
    }

    public void DeleteChar()
    {
        deleteConfirm.SetActive(true);

        int i = AbilityLevels.CharSaves.saveIDs.IndexOf(saveID);
        deleteText.text = $"Are you sure you want to delete your level {AbilityLevels.CharSaves.levels[i] + 1} {AbilityLevels.CharSaves.classes[i]}?";
    }

    public void ConfirmDelete()
    {
        AbilityLevels.DeleteAbilities(saveID);

        foreach (Button b in loadBtns)
            DestroyImmediate(b.gameObject);
        loadBtns.Clear();
        AbilityLevels.CharacterSaves saves = AbilityLevels.CharSaves;
        for (int i = 0; i < saves.saveIDs.Count; i++)
            AddLoadBtn(saves.saveIDs[i], saves.classes[i], saves.levels[i]);
    }
    #endregion
}
