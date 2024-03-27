using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TeleportArea : SafeArea
{
    [HideInInspector] public bool validTp = false;

    [Client]
    protected override void ClientOnTouchPlayer(PlayerAvatar target)
    {
        if (target.hasAuthority && GameController.Instance.playing)
            validTp = true;
    }

    [Client]
    public void OnChildTouchPlayer(PlayerAvatar target)
    {
        ClientOnTouchPlayer(target);
    }
}
