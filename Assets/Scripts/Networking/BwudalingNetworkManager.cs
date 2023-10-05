using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using UnityEditor;

public class BwudalingNetworkManager : NetworkManager
{
    public static BwudalingNetworkManager Instance;
    private GameController gc;

    [HideInInspector] public int currLevel = 0;
    private MapPack currMaps;
    private GameScene currEndScene;
    [HideInInspector] public bool sceneChanging = false;

    public List<NetworkPlayer> Players { get; } = new List<NetworkPlayer>();
    private NetworkPlayer activePlayer;
    public NetworkPlayer ActivePlayer { 
        get {
            if (!activePlayer || !activePlayer.hasAuthority)
            {
                foreach (NetworkPlayer p in Players)
                {
                    if (p.hasAuthority)
                    {
                        activePlayer = p;
                        break;
                    }
                }
            }

            return activePlayer;
        } 
    }
    [SerializeField] private int minPlayers = 2;

    [Header("Debug")]
    public bool DEBUG_AllowMapSkip;
    public bool DEBUG_AllowKeyCheats;
    public bool DEBUG_IgnoreCooldown;
    public bool DEBUG_TpWalls;


    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    public static event Action OnClientJoinFailed;
    public static event Action<NetworkConnection, int> OnServerReadied;

    public override void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        base.Awake();

        Instance = this;
    }
    public override void OnStartServer()
    {
        OnServerSceneChanged(SceneManager.GetActiveScene().name);
    }

    #region Server Callbacks
    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        if (numPlayers >= maxConnections)
        {
            conn.Disconnect();
            return;
        }

        if (SceneManager.GetActiveScene().buildIndex != Constants.MainMenu.buildIndex)
        {
            conn.Disconnect();
            return;
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if (conn.identity != null)
        {
            NetworkPlayer player = conn.identity.gameObject.GetComponent<NetworkPlayer>();
            Players.Remove(player);
        }

        base.OnServerDisconnect(conn);
    }

    public override void OnClientError(TransportError error, string reason)
    {
        if (error == TransportError.DnsResolve) 
            OnClientJoinFailed?.Invoke();
        else
            base.OnClientError(error, reason);
    }

    public override void OnStopServer()
    {
        Players.Clear();
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        GameObject p = Instantiate(playerPrefab);
        NetworkServer.AddPlayerForConnection(conn, p);

        p.GetComponent<NetworkPlayer>().IsLeader = Players.Count == 0;
    }

    public override void OnServerChangeScene(string newSceneName)
    {
        foreach (NetworkPlayer p in Players)
            p.IsReady = false;

        sceneChanging = true;

        base.OnServerChangeScene(newSceneName);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        sceneChanging = false;

        if (!MapController.Instance)
            return;

        gc = Instantiate(MapController.Instance.gameControllerPrefab);
        NetworkServer.Spawn(gc.gameObject);
    }

    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);

        if (SceneManager.GetActiveScene().buildIndex != Constants.MainMenu.buildIndex)
        {
            OnServerReadied?.Invoke(conn, Players.IndexOf(conn.identity.GetComponent<NetworkPlayer>()));
            conn.identity.GetComponent<NetworkPlayer>().IsReady = true;
            NotifyPlayersOfReadyState();
        }
    }
    #endregion

    #region Client Callbacks
    public override void OnClientConnect()
    {
        base.OnClientConnect();

        OnClientConnected?.Invoke();
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();

        OnClientDisconnected?.Invoke();
    }
    #endregion

    #region Lobby
    public void NotifyPlayersOfReadyState()
    {
        gc?.HandleReadyToStart(IsReadyToStart());
    }

    private bool IsReadyToStart()
    {
        if (numPlayers < minPlayers)
            return false;

        foreach (NetworkPlayer p in Players)
        {
            if (!p.IsReady)
            {
                return false;
            }
        }

        return true;
    }

    private void Update()
    {
        if (DEBUG_AllowMapSkip && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.S))
        {
            foreach (NetworkPlayer p in Players)
            {
                if (p.hasAuthority)
                {
                    if (p.IsLeader)
                        NextMap();

                    break;
                }
            }
        }
    }

    public void ReturnToLobby()
    {
        ServerChangeScene(Constants.MainMenu.name);
    }

    public void NextMap()
    {
        ChangeToLevel(currLevel + 1);
    }

    public void RestartMap()
    {
        ChangeToLevel(currLevel);
    }

    public void StartGame(int level, MapPack mapSet, GameScene endScene)
    {
        if (SceneManager.GetActiveScene().buildIndex == Constants.MainMenu.buildIndex)
            if (!IsReadyToStart())
                return;

        currMaps = mapSet;
        currEndScene = endScene;
        ChangeToLevel(level);
    }

    public void ChangeToLevel(int level)
    {
        if (level < 0)
        {
            Debug.LogError("Trying to change to invalid map " + level + ", defaulting to Test Map");
            ServerChangeScene(Constants.TestMap.name);
            return;
        }
        else if (level >= currMaps.maps.Length)
        {
            ServerChangeScene(currEndScene.name);
            return;
        }

        ServerChangeScene(currMaps.maps[level].name);
        currLevel = level;
    }
    #endregion
}
