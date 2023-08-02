using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsGameController : GameController
{
    private bool started = false;

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
        RpcSendServerBannerMessage("Welcome to the stats screen...", 0);
        yield break;
    }


    [Server]
    public override void SpawnPlayer(NetworkConnection conn, int player)
    {
        NetworkPlayer p = conn.identity.GetComponent<NetworkPlayer>();

    }


}
