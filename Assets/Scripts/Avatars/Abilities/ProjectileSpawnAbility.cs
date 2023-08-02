using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Events;

public class ProjectileSpawnAbility : AbilityBase
{
    [SerializeField] protected Transform projectileSpawn;
    [SerializeField] protected float bulletSpread;

    [Header("Projectile")]
    [SerializeField] protected Projectile projectilePrefab;
    [SerializeField] protected AbilityUpgrade count;
    //[SerializeField] protected AbilityUpgrade speed;
    [SerializeField] protected float speed;
    [SerializeField] protected AbilityUpgrade bounce;
    [SerializeField] protected UnityEvent onHitTarget;

    protected override void OnUseAbility(int level)
    {
        DoServerAbility(level);
    }

    [Server]
    public override void OnUseServerAbility(int level)
    {
        FireProjectiles(projectilePrefab, projectileSpawn.position, projectileSpawn.eulerAngles, (int)count.CalcValue(level), bulletSpread, level);
    }


    [Server]
    protected void FireProjectiles(Projectile prefab, Vector3 spawnPos, Vector3 spawnRot, int count, float spread, int level)
    {
        float startAng = count > 1 ? -spread / 2 : 0;
        float angStep = count > 1 ? spread / (count - 1) : 0;

        for (int i = 0; i < count; i++)
        {
            Projectile b = Instantiate(prefab, spawnPos, Quaternion.Euler(new Vector3(0, spawnRot.y + startAng + angStep * i, 0)));
            NetworkServer.Spawn(b.gameObject);

            OnSpawnProjectile(b, level);
        }
    }

    [Server]
    protected virtual void OnSpawnProjectile(Projectile b, int level)
    {
        b.SetSource(controller);
        b.SetSpeed(speed);//.CalcValue(level));
        int bounces = (int)bounce.CalcValue(level);
        if (bounces > 0) 
            ((RicochetProjectile)b).SetMaxBounces(bounces);
        b.HitTarget = onHitTarget;
    }
}
