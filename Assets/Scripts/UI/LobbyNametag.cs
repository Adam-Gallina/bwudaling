using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyNametag : NametagUI
{
    [SerializeField] private Image playerColorImg;
    [SerializeField] private TMPro.TMP_Text playerReadyText;
    [SerializeField] private TMPro.TMP_Text playerLevelText;
    [SerializeField] private TMPro.TMP_Text playerCharText;

    private MainMenu menu;

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

        menu.startGameButton.gameObject.SetActive(player.IsLeader);
        menu.debugGameButton.gameObject.SetActive(player.IsLeader);

        menu.readyButton.onClick.AddListener(ToggleReady);
        menu.startGameButton.onClick.AddListener(StartGame);
        menu.debugGameButton.onClick.AddListener(StartDebug);
    }

    protected override void Update()
    {
        if (LinkedPlayer.gameAvatarClass != AvatarClass.None)
        {
            playerLevelText.text = "Level " + (LinkedPlayer.avatarLevel + 1);
            playerCharText.text = Constants.LoadAvatarPrefab(LinkedPlayer.gameAvatarClass).AvatarName;
        }
    }

    private void OnDestroy()
    {
        menu.RemoveNametag(this);

        if (LinkedPlayer.hasAuthority)
        {
            menu.readyButton.onClick.RemoveListener(ToggleReady);
            menu.startGameButton.onClick.RemoveListener(StartGame);
            menu.debugGameButton.onClick.RemoveListener(StartDebug);
        }

        ColorSelect.Instance.PlayerSelectedColor(LinkedPlayer.avatarColor, Color.white);
    }

    public void UpdateUI()
    {
        playerColorImg.color = LinkedPlayer.avatarColor;
        playerNameText.text = LinkedPlayer.displayName;
        playerReadyText.text = LinkedPlayer.IsReady ? "<color=green>Ready</color>" : "<color=red>Not Ready</color>";
    }

    public void ToggleReady()
    {
        LinkedPlayer.CmdSetIsReady(!LinkedPlayer.IsReady);
    }

    public void StartGame()
    {
        if (!LinkedPlayer.IsLeader) return;

        BwudalingNetworkManager.Instance.StartGame(0, Constants.Maps, Constants.EndScreen);
    }

    public void StartDebug()
    {
        if (!LinkedPlayer.IsLeader) return;

        BwudalingNetworkManager.Instance.StartGame(0, Constants.DebugMaps, Constants.EndScreen);
    }

    #region Getters/Setters
    private void SetDisplayName(string displayName)
    {
        LinkedPlayer.CmdSetDisplayName(displayName);
    }
    #endregion
}
