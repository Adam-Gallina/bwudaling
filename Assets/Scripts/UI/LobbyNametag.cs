using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyNametag : NametagUI
{
    [SerializeField] private TMPro.TMP_Text playerReadyText;
    [SerializeField] private TMPro.TMP_Text playerLevelText;
    [SerializeField] private TMPro.TMP_Text playerCharText;

    private MainMenu menu;
    private MainMenu Menu { get { 
            if (menu == null) menu = (MainMenu)GameUI.Instance; 
            return menu;
    } }

    private void Awake()
    {
        menu = GameObject.Find("Canvas").GetComponent<MainMenu>();
    }

    public override void SetLinkedPlayer(NetworkPlayer player)
    {
        LinkedPlayer = player;
        GameUI.Instance.AddNametag(this);

        if (!player.hasAuthority)
            return;

        if (string.IsNullOrEmpty(player.displayName) || player.displayName.Equals("Unnamed Player"))
            SetDisplayName(MainMenu.DisplayName);
        GameObject.Find("Preview Cam").GetComponent<MenuPlayerPreview>().SetColor(player.avatarColor);

        Menu.startGameButton.gameObject.SetActive(player.IsLeader);
        Menu.mapPackSelect.gameObject.SetActive(player.IsLeader);

        Menu.readyButton.onClick.AddListener(ToggleReady);
        Menu.startGameButton.onClick.AddListener(StartGame);
    }

    protected override void Update()
    {
        if (LinkedPlayer.gameAvatarClass != AvatarClass.None)
        {
            playerLevelText.text = "Level " + (LinkedPlayer.avatarLevel + 1);
            playerCharText.text = Constants.LoadAvatarPrefab(LinkedPlayer.gameAvatarClass).AvatarName;
        }

        if (!LinkedPlayer.IsLeader) return;

        Menu.startGameButton.interactable = BwudalingNetworkManager.Instance.IsReadyToStart();
    }

    private void OnDestroy()
    {
        Menu.RemoveNametag(this);

        if (LinkedPlayer.hasAuthority)
        {
            Menu.readyButton.onClick.RemoveListener(ToggleReady);
            Menu.startGameButton.onClick.RemoveListener(StartGame);
        }

        ColorSelect.Instance?.PlayerSelectedColor(LinkedPlayer.avatarColor, Color.white, LinkedPlayer.hasAuthority);
    }

    public void UpdateUI()
    {
        playerColorImg.color = LinkedPlayer.avatarColor;
        playerNameText.text = LinkedPlayer.displayName;
        playerReadyText.text = LinkedPlayer.IsReady ? "<color=green>Ready</color>" : "<color=red>Not Ready</color>";
    }

    public void ToggleReady()
    {
        if (!LinkedPlayer.CanReady())
            return;

        Menu.readyButton.GetComponentInChildren<TMPro.TMP_Text>().text = !LinkedPlayer.IsReady ? "Unready" : "Ready";
        LinkedPlayer.CmdSetIsReady(!LinkedPlayer.IsReady);
    }

    public void StartGame()
    {
        if (!LinkedPlayer.IsLeader) return;

        MapPack pack = Constants.Maps[menu.mapPackSelect.value];
        BwudalingNetworkManager.Instance.StartGame(0, pack, Constants.EndScreen);
    }

    #region Getters/Setters
    private void SetDisplayName(string displayName)
    {
        LinkedPlayer.CmdSetDisplayName(displayName);
    }
    #endregion
}
