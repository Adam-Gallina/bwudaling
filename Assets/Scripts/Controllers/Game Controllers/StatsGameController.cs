using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsGameController : GameController
{
    private bool started = false;

    [SerializeField] private float timeBetweenStats;

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

        yield return new WaitForSeconds(1);
        ShowStat(PlayerStatType.Distance);
        yield return new WaitForSeconds(timeBetweenStats);
        ShowStat(PlayerStatType.Abilities);
        yield return new WaitForSeconds(timeBetweenStats);
        ShowStat(PlayerStatType.Deaths, true);
        yield return new WaitForSeconds(timeBetweenStats);
        ShowStat(PlayerStatType.Heals);
        yield return new WaitForSeconds(timeBetweenStats);
        ShowStat(PlayerStatType.Dodges);
        yield return new WaitForSeconds(timeBetweenStats);
        ShowStat(PlayerStatType.Haiws);
        yield return new WaitForSeconds(timeBetweenStats);

        Dictionary<NetworkPlayer, int> wins = new Dictionary<NetworkPlayer, int>();
        void AddWin(NetworkPlayer p) 
        { 
            if (!wins.ContainsKey(p))
                wins.Add(p, 0);
            wins[p]++;
        }
        AddWin(GetStatWinner(PlayerStatType.Distance));
        AddWin(GetStatWinner(PlayerStatType.Abilities));
        AddWin(GetStatWinner(PlayerStatType.Deaths, true));
        AddWin(GetStatWinner(PlayerStatType.Heals));
        AddWin(GetStatWinner(PlayerStatType.Dodges));
        AddWin(GetStatWinner(PlayerStatType.Haiws));

        int mostWins = 0;
        NetworkPlayer f = null;
        foreach (NetworkPlayer p in wins.Keys)
        {
            if (wins[p] > mostWins)
            {
                mostWins = wins[p];
                f = p;
            }
        }
        RpcShowStat(PlayerStatType.Best, f.displayName, 0, false);
        yield return new WaitForSeconds(timeBetweenStats * 3);

        RpcHideStats();

        yield break;
    }

    [Server]
    private void ShowStat(PlayerStatType stat, bool lowestWins=false)
    {
        NetworkPlayer w = GetStatWinner(stat, lowestWins);
        RpcShowStat(stat, w.displayName, w.currPlayerStats.StatVals[stat], true);
    }

    private NetworkPlayer GetStatWinner(PlayerStatType stat, bool lowestWins=false)
    {
        NetworkPlayer player = BwudalingNetworkManager.Instance.Players[0];
        int winningVal = lowestWins ? int.MaxValue : 0;

        foreach (NetworkPlayer p in BwudalingNetworkManager.Instance.Players)
        {
            if (lowestWins ? p.currPlayerStats.StatVals[stat] < winningVal : p.currPlayerStats.StatVals[stat] > winningVal)
            {
                player = p;
                winningVal = p.currPlayerStats.StatVals[stat];
            }
        }

        return player;
    }

    [ClientRpc] 
    private void RpcShowStat(PlayerStatType stat, string winningPlayer, int winningVal, bool showLocalPlayer)
    {
        NetworkPlayer p = BwudalingNetworkManager.Instance.ActivePlayer;    

        int pVal = showLocalPlayer ? p.GetComponent<PlayerStats>().currStats.StatVals[stat] : 0;
        StatsUI.SetStatDisplay(stat, winningPlayer, winningVal, pVal);
    }

    [ClientRpc]
    private void RpcHideStats()
    {
        StatsUI.HideStatDisplay();
    }

    [Server]
    public override void SpawnPlayer(NetworkConnection conn, int player)
    {
        NetworkPlayer p = conn.identity.GetComponent<NetworkPlayer>();
        p.RpcGetPlayerStats();

        base.SpawnPlayer(conn, player);
    }
}
