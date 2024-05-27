using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

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

    [SyncVar]
    private bool started = false;
    [SyncVar]
    private bool ended = false;

    public static event Action OnMapStarted;
    public static event Action<NetworkPlayer> OnMapCompleted;

    protected override void Awake()
    {
        base.Awake();

        source = GetComponent<AudioSource>();
    }

    protected virtual void Update()
    {
        if (started && !ended)
        {
            ElapsedTime += Time.deltaTime;

            if (isServer)
            {
                bool allDead = true;
                foreach (NetworkPlayer p in BwudalingNetworkManager.Instance.Players)
                {
                    if (p.avatar && !p.avatar.dead)
                        allDead = false;
                }

                if (allDead)
                    RpcPlayersDead();
            }

        }
    }

    [ClientRpc]
    private void RpcPlayersDead()
    {
        ((LevelUI)GameUI.Instance).SetRespawnPanel(true);
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
            RpcOnLevelStart();
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

    [ClientRpc]
    private void RpcOnLevelStart()
    {
        OnMapStarted?.Invoke();
    }

    [Server]
    private IEnumerator StartSequence()
    {
        SpawnAllMapEnemies();

        yield return StartCoroutine(BeforeStartMessages());

        RpcPlayBackgroundStart();

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
                n.avatar.RpcAddXp(MapController.Instance.winXp, false);

            ended = true;
            StartCoroutine(EndSequence(p));

            RpcOnPlayerWin(p);
        }
    }

    [ClientRpc]
    private void RpcOnPlayerWin(NetworkPlayer p)
    {
        OnMapCompleted?.Invoke(p);
        BwudalingNetworkManager.Instance.ActivePlayer.avatar.DoDance();
    }

    [Server]
    protected virtual IEnumerator EndSequence(NetworkPlayer p)
    {
        RpcSetPlayerMovement(false);

        RpcSetCamera(p.avatar, 1);

        RpcSendServerBannerMessage(p.displayName + " won!", p.avatarColor, 0);
        yield return new WaitForSeconds(endMessageTime);
        RpcSendServerBannerMessage(string.Empty, 0);

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
        CameraController.Instance.SetZoom(0);
        CameraController.Instance.SetTarget(BwudalingNetworkManager.Instance.ActivePlayer.avatar.transform, 1);
    }

    [ClientRpc]
    protected void RpcPlayBackgroundStart()
    {
        BackgroundMusic.Instance.PlayStartClip();
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
