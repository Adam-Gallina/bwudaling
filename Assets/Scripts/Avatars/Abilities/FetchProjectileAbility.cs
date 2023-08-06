using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FetchProjectileAbility : ProjectileSpawnAbility
{
    [SerializeField] private AbilityUpgrade fetchDuration;
    [SerializeField] private AbilityUpgrade fetchSpeed;


    [Server]
    protected override void OnSpawnProjectile(Projectile b, int level)
    {
        base.OnSpawnProjectile(b, level);

        ((FetchProjectile)b).SetFetchVals(fetchDuration.CalcValue(level), fetchSpeed.CalcValue(level));
    }
}
