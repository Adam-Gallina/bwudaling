using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ShrinkProjectile : Projectile
{
    [HideInInspector] public float sizeDebuff;
    [HideInInspector] public float debuffDuration;

    [Server]
    protected override void OnHitTarget(Collider other)
    {
        other.GetComponentInParent<BasicSaw>()?.ApplySizeMod(sizeDebuff, debuffDuration);

        base.OnHitTarget(other);
    }
}
