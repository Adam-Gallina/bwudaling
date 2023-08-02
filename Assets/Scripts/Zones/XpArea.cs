using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class XpArea : SafeArea
{
    [SerializeField] protected ParticleSystem reachEffect;

    [Client]
    protected override void ClientOnTouchPlayer(PlayerAvatar target)
    {
        if (IsNewPlayer(target) && target.hasAuthority)
            reachEffect?.Play();
    }

    [Server]
    protected override void ServerOnTouchPlayer(PlayerAvatar target)
    {
        if (IsNewPlayer(target))
        {
            target.RpcAddXp(MapController.Instance.safeZoneXp);
        }
    }
}
