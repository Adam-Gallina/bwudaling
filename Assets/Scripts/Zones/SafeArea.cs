using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SafeArea : MonoBehaviour
{    
    protected List<PlayerAvatar> reached = new List<PlayerAvatar>();

    //protected List<PlayerAvatar> contained = new List<PlayerAvatar>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == Constants.PlayerLayer)
        {
            PlayerAvatar target = other.GetComponentInParent<PlayerAvatar>();

            if (!target)
                return;
            
            //contained.Add(target);

            if (target.isClient)
                ClientOnTouchPlayer(target);
            if (target.isServer)
                ServerOnTouchPlayer(target);

            if (IsNewPlayer(target))
                reached.Add(target);

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == Constants.PlayerLayer)
        {
            PlayerAvatar target = other.GetComponentInParent<PlayerAvatar>();

            if (!target)
                return;

            //contained.Remove(target);
        }
    }

    protected bool IsNewPlayer(PlayerAvatar p)
    {
        return !reached.Contains(p);
    }

    [Client]
    protected virtual void ClientOnTouchPlayer(PlayerAvatar target)
    {
    }
    [Server]
    protected virtual void ServerOnTouchPlayer(PlayerAvatar target)
    {
    }
}
