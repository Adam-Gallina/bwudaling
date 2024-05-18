using Steamworks;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : GameUI
{
    private List<NametagUI> lobbyPlayers = new List<NametagUI>();

    [Header("Main Menu")]
    [SerializeField] private TMPro.TMP_InputField nameInputField;
    [SerializeField] private TMPro.TMP_InputField ipAddressField;

    [SerializeField] private GameObject joinFailedText;

    [Header("Lobby")]
    [SerializeField] private RectTransform inviteBtn;
    [SerializeField] private TMPro.TMP_Text lobbyCount;
    public Button readyButton;
    public Button startGameButton;
    public TMPro.TMP_Dropdown mapPackSelect;
    public Sprite[] difficultyIndicators;
    [SerializeField] private TutorialObject[] tutorials;
    private Dictionary<MapPackType, GameObject> tutorialObj;
    public Dictionary<MapPackType, GameObject> TutorialObj
    {
        get
        {
            if (tutorialObj == null)
            {
                tutorialObj = new Dictionary<MapPackType, GameObject>();
                foreach (TutorialObject t in tutorials)
                    tutorialObj.Add(t.packType, t.tutObject);
            }

            return tutorialObj;
        }
    }

    public static string DisplayName { get; private set; }

    private Animator anim;

    protected override void Awake()
    {
        base.Awake();

        startGameButton.gameObject.SetActive(false);
        mapPackSelect.gameObject.SetActive(true);
        SetSelectedMapPack(mapPackSelect.value);

        anim = GetComponent<Animator>();
    }

    protected override void Start()
    {
        base.Start();

        GameObject.Find("Dance Preview").SetActive(false);

        nameInputField.text = PlayerPrefs.GetString(Constants.PlayerNamePref, "Player");
        if (PlayerPrefs.HasKey(Constants.LastIpPref))
            ipAddressField.text = PlayerPrefs.GetString(Constants.LastIpPref);

        mapPackSelect.options.Clear();
        foreach (MapPack maps in Constants.Maps)
        {
            TMPro.TMP_Dropdown.OptionData data = new TMPro.TMP_Dropdown.OptionData(maps.name);
            if (difficultyIndicators.Length > maps.difficulty && maps.difficulty >= 0)
                data.image = difficultyIndicators[maps.difficulty];

            mapPackSelect.AddOptions(new List<TMPro.TMP_Dropdown.OptionData> { data });
        }

        if (BwudalingNetworkManager.Instance.mode != Mirror.NetworkManagerMode.Offline)
        {
            anim.SetTrigger("RejoinLobby");
            anim.SetBool("InLobby", true);
            NotificationUI.Instance.gameObject.SetActive(false);
        }
        else
        {
            SetPlayerName(nameInputField.text);
            NotificationUI.Instance.gameObject.SetActive(true);
        }
    }

    private void OnEnable()
    {
        BwudalingNetworkManager.OnClientConnected += HandleClientConnected;
        BwudalingNetworkManager.OnClientDisconnected += HandleClientDisconnected;
        BwudalingNetworkManager.OnClientJoinFailed += HandleClientJoinFailed;
    }

    private void OnDisable()
    {
        BwudalingNetworkManager.OnClientConnected -= HandleClientConnected;
        BwudalingNetworkManager.OnClientDisconnected -= HandleClientDisconnected;
        BwudalingNetworkManager.OnClientJoinFailed -= HandleClientJoinFailed;
    }

    public void SetPlayerName(string name)
    {
        DisplayName = name;
    }

    public void SetSelectedMapPack(int pack)
    {
        mapPackSelect.value = pack;

        foreach (GameObject obj in TutorialObj.Values)
            obj.SetActive(false);

        TutorialObj[Constants.Maps[pack].packType].SetActive(true);
    }

    public override void AddNametag(NametagUI nametag)
    {
        lobbyPlayers.Add(nametag);

        UpdateDisplay();
    }

    public override void RemoveNametag(NametagUI nametag)
    {
        lobbyPlayers.Remove(nametag);

        UpdateDisplay();
    }

    public override void UpdateDisplay()
    {
        for (int i = 0; i < lobbyPlayers.Count; i++)
        {
            lobbyPlayers[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -2 - (i * 48));
            ((LobbyNametag)lobbyPlayers[i])?.UpdateUI();
        }

        //if (ManagerDebug.Instance.DEBUG_useKcpManager)
        //    inviteBtn.GetComponentInChildren<Button>().gameObject.SetActive(false);
        inviteBtn.GetComponentInChildren<Button>().interactable = lobbyPlayers.Count < BwudalingNetworkManager.Instance.maxConnections;
        inviteBtn.anchoredPosition = new Vector2(0, -2 - (lobbyPlayers.Count * 48));
        lobbyCount.text = $"{lobbyPlayers.Count}/{BwudalingNetworkManager.Instance.maxConnections}";

        int rows = lobbyPlayers.Count < BwudalingNetworkManager.Instance.maxConnections ? lobbyPlayers.Count + 1 : lobbyPlayers.Count;
        nametagParent.GetComponent<RectTransform>().sizeDelta = new Vector2(nametagParent.GetComponent<RectTransform>().sizeDelta.x, rows * 48 + 2);
    }

    public void ClearPlayers()
    {
        foreach (LobbyNametag p in lobbyPlayers)
            Destroy(p.gameObject);
    }

    #region Buttons
    public void HostLobby()
    {
        if (ManagerDebug.Instance.DEBUG_useKcpManager)
        {
            BwudalingNetworkManager.Instance.StartHost();
        }
        else
        {
            ManagerDebug.Instance.steamTransport.GetComponent<SteamLobby>().HostSteamLobby();
        }
    }

    public void JoinLobby()
    {
        if (ManagerDebug.Instance.DEBUG_useKcpManager)
        {
            string ipAddress = ipAddressField.text;
            BwudalingNetworkManager.Instance.networkAddress = ipAddress;
            BwudalingNetworkManager.Instance.StartClient();
        }
        else
        {
            ManagerDebug.Instance.steamTransport.GetComponent<SteamLobby>().JoinSteamLobby();
        }

        //joinButton.interactable = false;
    }

    public void PressInvite()
    {
        if (ManagerDebug.Instance.DEBUG_useKcpManager)
            return;

        SteamLobby lobby = ManagerDebug.Instance.steamTransport.GetComponent<SteamLobby>();
        SteamFriends.ActivateGameOverlayInviteDialog(lobby.lobbyID);
    }

    public void PressLeave()
    {
        switch (BwudalingNetworkManager.Instance.mode)
        {
            case Mirror.NetworkManagerMode.ClientOnly:
                BwudalingNetworkManager.Instance.DisconnectClient();
                anim.SetBool("InLobby", false);
                break;
            case Mirror.NetworkManagerMode.Host:
                BwudalingNetworkManager.Instance.StopHost();
                anim.SetBool("InLobby", false);
                break;
            case Mirror.NetworkManagerMode.Offline:
                Debug.LogError("Trying to leave when already disconnected");
                break;
            default:
                Debug.LogError("Idk what happened but probably ur trying to make a server now so that's pretty cool");
                break;
        }

        ClearPlayers();
    }

    public void PressQuit()
    {
        Application.Quit();
    }

    public void ShowNotifications()
    {
        NotificationUI.Instance.gameObject.SetActive(true);
    }
    #endregion

    #region Callbacks
    private void HandleClientConnected()
    {
        anim.SetBool("InLobby", true);
        NotificationUI.Instance.gameObject.SetActive(false);

        PlayerPrefs.SetString(Constants.LastIpPref, ipAddressField.text);
        PlayerPrefs.SetString(Constants.PlayerNamePref, DisplayName);
    }

    private void HandleClientDisconnected()
    {
        anim.SetBool("InLobby", false);
        NotificationUI.Instance.gameObject.SetActive(true);

        readyButton.GetComponentInChildren<TMPro.TMP_Text>().text = "Ready";
    }

    private void HandleClientJoinFailed()
    {
        joinFailedText.SetActive(true);
        NotificationUI.Instance.gameObject.SetActive(true);
    }
    #endregion
}

[System.Serializable]
public struct TutorialObject
{
    public MapPackType packType;
    public GameObject tutObject;
}