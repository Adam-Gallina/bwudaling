using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class HealProjectile : RicochetProjectile
{
    [Server]
    protected override void OnHitTarget(Collider other)
    {
        PlayerAvatar player = other.GetComponentInParent<PlayerAvatar>();

        if (player && player.dead)
        {
            player.Heal();

            base.OnHitTarget(other);
        }
    }
}
