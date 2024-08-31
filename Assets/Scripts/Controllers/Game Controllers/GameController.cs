using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Unity.IO.LowLevel.Unsafe;
using System;

public enum MapType { Normal, Boss }
public abstract class GameController : NetworkBehaviour
{
    public static GameController Instance;
    public static float ElapsedTime { get; protected set; } = 0;

    protected int spawnedPlayers = 0;

    [HideInInspector] public bool playing = false;

    public MapType mapType;

    protected virtual void Awake()
    {
        Instance = this;
    }

    public override void OnStartServer()
    {
        BwudalingNetworkManager.OnServerReadied += SpawnPlayer;
    }

    public override void OnStopServer()
    {
        BwudalingNetworkManager.OnServerReadied -= SpawnPlayer;
    }

    [Server]
    public virtual void HandleReadyToStart(bool ready)
    {
        RpcHandleReadyToStart(ready);
    }

    [ClientRpc]
    protected virtual void RpcHandleReadyToStart(bool ready)
    {
        Debug.LogWarning("RpcHandleReadyToStart not implemented (recieved '" + ready + "')");
    }

    [Server]
    protected virtual void CheckRoundEnd()
    {

    }

    [Server]
    public virtual void HandlePlayerWin(NetworkPlayer p)
    {
        Debug.LogWarning("HandlePlayerWin not implemented ('" + p.displayName + "' won)");
    }

    [Server]
    public virtual void SpawnPlayer(NetworkConnection conn, int player)
    {
        Transform t = MapController.Instance.GetSpawnPos(player);

        NetworkPlayer np = conn.identity.GetComponent<NetworkPlayer>();

        PlayerAvatar prefab = Constants.LoadAvatarPrefab(np.gameAvatarClass);
        PlayerAvatar avatar = Instantiate(prefab, t.position, t.rotation);
        NetworkServer.Spawn(avatar.gameObject, conn);

        np.RpcOnAvatarSpawned(avatar.gameObject);

        spawnedPlayers += 1;
    }

    public static void ResetTimer()
    {
        ElapsedTime = 0;
    }

    [Command]
    public void CmdSendBannerMessage(string message, float duration) { RpcSendServerBannerMessage(message, duration); }

    [ClientRpc]
    public void RpcSendServerBannerMessage(string message, float duration)
    {
        if (!GameUI.Instance)
        {
            Debug.LogWarning("Trying to send message '" + message + "' to players, but no GameUI exists");
            return;
        }

        GameUI.Instance.SetBannerText(message, duration);
    }
    [ClientRpc]
    public void RpcSendServerBannerMessage(string message, Color col, float duration)
    {
        if (!GameUI.Instance)
        {
            Debug.LogWarning("Trying to send message '" + message + "' to players, but no GameUI exists");
            return;
        }

        GameUI.Instance.SetBannerText(message, col, duration);
    }

    [ClientRpc]
    protected void RpcSendServerAudioClip(int clipNum)
    {
        OnPlayServerAudioClip(clipNum);
    }
    protected virtual void OnPlayServerAudioClip(int clipNum)
    {

    }

    [Command]
    public void CmdSetPlayerMovement(bool canMove)
    {
        RpcSetPlayerMovement(canMove);
    }
    [ClientRpc]
    protected void RpcSetPlayerMovement(bool canMove)
    {
        MapController.Instance.canControlAvatars = canMove;
    }
}
