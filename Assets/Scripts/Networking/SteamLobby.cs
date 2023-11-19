using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;

public class SteamLobby : MonoBehaviour
{
    private BwudalingNetworkManager networkManager;

    private const string HostAddressKey = "HostAddress";

    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> LobbyJoinRequested;
    protected Callback<LobbyEnter_t> LobbyEntered;

    public CSteamID lobbyID { get; private set; }

    private void Start()
    {
        networkManager = GetComponent<BwudalingNetworkManager>();

        if (!SteamManager.Initialized)
            return;

        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        LobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoinRequested);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void HostSteamLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);
    }

    public void JoinSteamLobby()
    {
        SteamFriends.ActivateGameOverlay("Friends");
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
            // Show host button
            return;

        networkManager.StartHost();
        lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        SteamMatchmaking.SetLobbyData(lobbyID, HostAddressKey, SteamUser.GetSteamID().ToString());
    }

    private void OnLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (NetworkServer.active)
            return;

        lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        string hostAddress = SteamMatchmaking.GetLobbyData(lobbyID, HostAddressKey);

        networkManager.networkAddress = hostAddress;
        networkManager.StartClient();

        // Hide host/join button
    }
}
