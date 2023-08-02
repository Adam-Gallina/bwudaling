using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FetchProjectileAbility : ProjectileSpawnAbility
{
    [Server]
    protected override void OnSpawnProjectile(Projectile b, int level)
    {
        base.OnSpawnProjectile(b, level);

    }
}
