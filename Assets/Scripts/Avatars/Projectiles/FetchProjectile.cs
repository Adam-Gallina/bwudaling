using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FetchProjectile : Projectile
{
    protected override void OnHitTarget(Collider other)
    {
        PlayerAvatar p = other.GetComponentInParent<PlayerAvatar>();

        if (p && p.dead)
        {
            ((Dogie)spawner).SetFetchTarget(p);

            base.OnHitTarget(other);
        }
    }
}
