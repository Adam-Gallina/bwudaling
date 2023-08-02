using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EndArea : XpArea
{
    [Server]
    protected override void ServerOnTouchPlayer(PlayerAvatar target)
    {
        if (IsNewPlayer(target))
        {
            foreach (NetworkPlayer p in BwudalingNetworkManager.Instance.Players)
            {
                if (p.avatar == target)
                {
                    GameController.Instance.HandlePlayerWin(p);
                }
            }

            target.RpcAddXp(MapController.Instance.endZoneXp);
        }
    }
}
