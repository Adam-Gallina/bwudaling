using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BasicGameController : GameController
{
    [SerializeField] protected string[] startBannerMessages = new string[] { "READY?", "3", "2", "1", "GO!" };
    [SerializeField] protected int[] startBannerSounds = new int[] { -1, 0, 0, 0, 1 };
    //[SerializeField] protected string[] endBannerMessages = new string[] { "Returning to lobby..." };
    [SerializeField] protected float messageTime = 1;
    [SerializeField] protected float endMessageTime = 3;
    [SerializeField] protected AudioClip boopClip;
    [SerializeField] protected AudioClip beepClip;
    protected AudioSource source;

    private bool started = false;
    private bool ended = false;

    protected override void Awake()
    {
        base.Awake();

        source = GetComponent<AudioSource>();
    }

    public override void OnStartClient()
    {
        GameUI.Instance?.SetBannerText("Waiting for players...");
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

        for (int i = 0; i < startBannerMessages.Length; i++)
        {
            RpcSendServerBannerMessage(startBannerMessages[i], 0);
            RpcSendServerAudioClip(startBannerSounds[i]);
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
        RpcSetCamera(p.avatar, 1);

        RpcSendServerBannerMessage(p.displayName + " won!", p.avatarColor, 0);
        yield return new WaitForSeconds(messageTime);

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


    protected override void OnPlayServerAudioClip(int clipNum)
    {
        switch (clipNum)
        {
            case -1:
                return;
            case 0:
                source.clip = boopClip;
                source.Play();
                return;
            case 1:
                source.clip = beepClip;
                source.Play();
                return;
            default:
                Debug.LogError("Can't play clip number " + clipNum);
                return;
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
