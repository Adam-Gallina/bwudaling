using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : GameUI
{
    private List<NametagUI> lobbyPlayers = new List<NametagUI>();

    [SerializeField] public GameObject startPagePanel;
    [SerializeField] public GameObject lobbyPanel;

    [Header("Main Menu")]
    [SerializeField] private TMPro.TMP_InputField nameInputField;
    [SerializeField] private TMPro.TMP_InputField ipAddressField;

    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;

    [Header("Lobby")]
    public Button readyButton;
    public Button startGameButton;
    public TMPro.TMP_Dropdown mapPackSelect;
    public Sprite[] difficultyIndicators;

    public static string DisplayName { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        startGameButton.gameObject.SetActive(false);
        mapPackSelect.gameObject.SetActive(false);
    }

    private void Start()
    {
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
            startPagePanel.SetActive(false);
            lobbyPanel.SetActive(true);
        }
        else
        {
            startPagePanel.SetActive(true);
            lobbyPanel.SetActive(false);
            SetPlayerName(nameInputField.text);
        }
    }

    private void OnEnable()
    {
        BwudalingNetworkManager.OnClientConnected += HandleClientConnected;
        BwudalingNetworkManager.OnClientDisconnected += HandleClientDisconnected;
    }

    private void OnDisable()
    {
        BwudalingNetworkManager.OnClientConnected -= HandleClientConnected;
        BwudalingNetworkManager.OnClientDisconnected -= HandleClientDisconnected;
    }

    public void SetPlayerName(string name)
    {
        DisplayName = name;
        UpdateButtons();
    }

    public void UpdateButtons()
    {
        hostButton.interactable = !string.IsNullOrEmpty(DisplayName);
        joinButton.interactable = !string.IsNullOrEmpty(DisplayName) && !string.IsNullOrEmpty(ipAddressField.text);
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
            lobbyPlayers[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -10 - (i * 50));
            ((LobbyNametag)lobbyPlayers[i])?.UpdateUI();
        }
    }

    public void ClearPlayers()
    {
        foreach (LobbyNametag p in lobbyPlayers)
            Destroy(p.gameObject);
    }

    #region Buttons
    public void HostLobby()
    {
        BwudalingNetworkManager.Instance.StartHost();

        startPagePanel.SetActive(false);
    }

    public void JoinLobby()
    {
        string ipAddress = ipAddressField.text;

        BwudalingNetworkManager.Instance.networkAddress = ipAddress;
        BwudalingNetworkManager.Instance.StartClient();

        joinButton.interactable = false;
    }

    public void PressLeave()
    {
        switch (BwudalingNetworkManager.Instance.mode)
        {
            case Mirror.NetworkManagerMode.ClientOnly:
                BwudalingNetworkManager.Instance.StopClient();
                break;
            case Mirror.NetworkManagerMode.Host:
                BwudalingNetworkManager.Instance.StopHost();
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
    #endregion

    #region Callbacks
    private void HandleClientConnected()
    {
        joinButton.interactable = true;

        startPagePanel.SetActive(false);
        lobbyPanel.SetActive(true);
        //LobbyUI.LInstance.gameObject.SetActive(true);

        PlayerPrefs.SetString(Constants.LastIpPref, ipAddressField.text);
        PlayerPrefs.SetString(Constants.PlayerNamePref, DisplayName);
    }

    private void HandleClientDisconnected()
    {
        joinButton.interactable = true;

        UpdateButtons();

        startPagePanel.SetActive(true);
        lobbyPanel.SetActive(false);
        //LobbyUI.LInstance.gameObject.SetActive(false);

        //LobbyUI.LInstance.ClearPlayers();
    }
    #endregion
}
