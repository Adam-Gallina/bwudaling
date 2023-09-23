using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum ItemType { None, BwudaHaiw }
public class ItemBase : NetworkBehaviour
{
    public ItemType itemType;
    protected PlayerAvatar targetPlayer;
    [SerializeField] protected bool canSteal;

    [ServerCallback]
    protected virtual void OnTriggerEnter(Collider other)
    {
        if ((canSteal || targetPlayer == null) && other.gameObject.layer == Constants.PlayerLayer)
        {
            PlayerAvatar p = other.GetComponentInParent<PlayerAvatar>();
            if (p && p.heldItem == null)
            {
                OnCollect(p);
            }
        }
    }

    [Server]
    protected virtual void OnCollect(PlayerAvatar p)
    {
        targetPlayer = p;
        targetPlayer.heldItem = this;
    }

    [Server]
    public virtual void Drop()
    {
        if (targetPlayer)
            targetPlayer.heldItem = null;
        targetPlayer = null;
    }
}
