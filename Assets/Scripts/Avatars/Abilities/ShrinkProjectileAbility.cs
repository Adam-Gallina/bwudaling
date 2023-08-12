using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrinkProjectileAbility : ProjectileSpawnAbility
{
    [Header("Shrink Projectile")]
    [SerializeField] private AbilityUpgrade shrinkSize;
    [SerializeField] private AbilityUpgrade shrinkDuration;

    [Server]
    protected override void OnSpawnProjectile(Projectile b, int level)
    {
        base.OnSpawnProjectile(b, level);

        ShrinkProjectile sp = (ShrinkProjectile)b;
        sp.sizeDebuff = shrinkSize.CalcValue(level);
        sp.debuffDuration = shrinkDuration.CalcValue(level);
    }
}
