using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmackProjectile : RicochetProjectile
{
    [Server]
    protected override void OnHitTarget(Collider other)
    {
        BasicSaw s = other.GetComponentInParent<BasicSaw>();
        if (s)
        {
            NetworkServer.Destroy(s.gameObject);
        }

        base.OnHitTarget(other);
    }
}
