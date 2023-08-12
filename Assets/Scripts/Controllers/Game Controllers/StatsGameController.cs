using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsGameController : GameController
{
    private bool started = false;

    private StatsUI statsUI;
    private StatsUI StatsUI { get
        {
            if (statsUI == null)
                statsUI = (StatsUI)GameUI.Instance;
            return statsUI;
        } }

    public override void HandleReadyToStart(bool ready)
    {
        if (!started && ready)
        {
            started = true;
            StartCoroutine(StatsAnim());
        }
    }

    private IEnumerator StatsAnim()
    {
        //RpcSendServerBannerMessage("Welcome to the stats screen...", 0);

        yield return new WaitUntil(() =>
        {
            foreach (NetworkPlayer p in BwudalingNetworkManager.Instance.Players)
            {
                if (!p.currPlayerStats.updated)
                    return false;
            }

            return true;
        });

        ShowStat(PlayerStatType.Distance);
        ShowStat(PlayerStatType.Abilities);
        ShowStat(PlayerStatType.Deaths, true);
        ShowStat(PlayerStatType.Heals);
        ShowStat(PlayerStatType.Dodges);
        ShowStat(PlayerStatType.Haiws);

        //RpcSendServerBannerMessage("Stats loaded!", 0);
        yield break;
    }

    [Server]
    private void ShowStat(PlayerStatType stat, bool doMin=false)
    {
        NetworkPlayer p = GetStatWinner(stat, doMin);
        RpcShowStat(stat, p.displayName, p.currPlayerStats.StatVals[stat]);
    }

    private NetworkPlayer GetStatWinner(PlayerStatType stat, bool doMin=false)
    {
        NetworkPlayer player = BwudalingNetworkManager.Instance.Players[0];
        int winningVal = doMin ? int.MaxValue : 0;

        foreach (NetworkPlayer p in BwudalingNetworkManager.Instance.Players)
        {
            if (doMin ? p.currPlayerStats.StatVals[stat] < winningVal : p.currPlayerStats.StatVals[stat] > winningVal)
            {
                player = p;
                winningVal = p.currPlayerStats.StatVals[stat];
            }
        }

        return player;
    }

    [ClientRpc] 
    private void RpcShowStat(PlayerStatType stat, string winningPlayer, int winningVal)
    {
        foreach (NetworkPlayer p in BwudalingNetworkManager.Instance.Players)
        {
            if (p.hasAuthority)
            {
                StatsUI.SetStatDisplay(stat, winningPlayer, winningVal, p.GetComponent<PlayerStats>().currStats.StatVals[stat]);
                break;
            }
        }

    }

    [Server]
    public override void SpawnPlayer(NetworkConnection conn, int player)
    {
        NetworkPlayer p = conn.identity.GetComponent<NetworkPlayer>();
        p.RpcGetPlayerStats();
    }
}
