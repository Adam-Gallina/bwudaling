using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BasicGameController : GameController
{
    [SerializeField] protected string[] startBannerMessages = new string[] { "READY?", "3", "2", "1", "GO!" };
    //[SerializeField] protected string[] endBannerMessages = new string[] { "Returning to lobby..." };
    [SerializeField] protected float messageTime = 1;
    [SerializeField] protected float endMessageTime = 3;

    private bool started = false;
    private bool ended = false;

    public override void OnStartClient()
    {
        GameUI.Instance?.SetBannerText("Waiting for players...");
    }

    private void Update()
    {
        if (BwudalingNetworkManager.Instance.DEBUG_AllowKeyCheats)
            if (Input.GetKeyDown(KeyCode.Backslash))
                BwudalingNetworkManager.Instance.NextMap();
    }

    [Server]
    public override void HandleReadyToStart(bool ready)
    {
        if (!started && ready)// && spawnedPlayers == BwudalingNetworkManager.Instance.Players.Count)
        {
            started = true;
            StartCoroutine(StartSequence());
            RpcSendServerBannerMessage(string.Empty, 0);
        }
    }

    [ClientRpc]
    protected override void RpcHandleReadyToStart(bool ready)
    {
        if (ready)
        {
            playing = true;
            MapController.Instance.startWall.SetActive(false);
        }
    }

    [Server]
    private IEnumerator StartSequence()
    {
        SpawnAllMapEnemies();

        yield return StartCoroutine(BeforeStartMessages());

        foreach (string m in startBannerMessages)
        {
            RpcSendServerBannerMessage(m, 0);
            yield return new WaitForSeconds(messageTime);
        }

        RpcSendServerBannerMessage(string.Empty, 0);

        RpcHandleReadyToStart(true);
    }

    [Server]
    protected virtual IEnumerator BeforeStartMessages()
    {
        yield return null;
    }

    [Server]
    public override void HandlePlayerWin(NetworkPlayer p)
    {
        if (!ended)
        {
            foreach (NetworkPlayer n in BwudalingNetworkManager.Instance.Players)
                n.avatar.RpcAddXp(MapController.Instance.winXp);

            ended = true;
            StartCoroutine(EndSequence(p));
        }
    }

    [Server]
    protected virtual IEnumerator EndSequence(NetworkPlayer p)
    {
        RpcSetCamera(p, 1);

        //foreach (string m in endBannerMessages)
        //{
            RpcSendServerBannerMessage(p.displayName + " won!", 0);//\n"); + m, 0);
            yield return new WaitForSeconds(messageTime);
        //}

        RpcSendServerBannerMessage(string.Empty, 0);
        yield return null;

        BwudalingNetworkManager.Instance.NextMap();
    }

    [ClientRpc]
    protected void RpcSetCamera(NetworkBehaviour target, int importance)
    {
        CameraController.Instance.SetTarget(target.transform, importance);
    }

    [ClientRpc]
    protected void RpcSetPlayerCamera()
    {
        foreach (NetworkPlayer p in BwudalingNetworkManager.Instance.Players)
        {
            if (p.hasAuthority)
            {
                CameraController.Instance.SetTarget(p.avatar.transform, 1);
            }
        }
    }

    #region Map Enemies
    private void SpawnAllMapEnemies()
    {
        foreach (NetworkEnemySpawnPos spawner in MapController.Instance.GetMapEnemies())
            if (spawner.spawn)
                spawner.SpawnEnemy();
    }
    #endregion
}