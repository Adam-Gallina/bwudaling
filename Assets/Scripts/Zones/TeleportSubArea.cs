using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportSubArea : SafeArea
{
    private TeleportArea p;

    private void Awake()
    {
        p = GetComponentInParent<TeleportArea>();
    }

    [Client]
    protected override void ClientOnTouchPlayer(PlayerAvatar target)
    {
        p.OnChildTouchPlayer(target);
    }
}
