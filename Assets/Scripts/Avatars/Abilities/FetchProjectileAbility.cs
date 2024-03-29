using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FetchProjectileAbility : TargetAbility
{
    [SerializeField] private AbilityUpgrade fetchDuration;
    [SerializeField] private AbilityUpgrade fetchSpeed;

    [Header("Projectile")]
    [SerializeField] protected Transform projectileSpawn;

    [Header("Projectile")]
    [SerializeField] protected Projectile projectilePrefab;
    [Tooltip("Value added to players base speed")]
    [SerializeField] protected float speedMod;
    [SerializeField] protected UnityEvent onHitTarget;


    protected override bool OnUseAbility(int level)
    {
        bool validTarget = false;
        foreach (Collider c in Physics.OverlapSphere(currReticlePos, radius.CalcValue(level), 1 << Constants.PlayerLayer))
        {
            PlayerAvatar p = c.GetComponent<PlayerAvatar>();
            if (p == null || p == controller)
                continue;
            if (!p.dead)
                continue;
            validTarget = true;
            break;
        }

        if (!validTarget)
            return false;

        DoServerAbility(currReticlePos, level);
        return true;
    }


    [Server]
    public override void OnUseServerAbility(Vector3 target, int level)
    {
        foreach (Collider c in Physics.OverlapSphere(target, radius.CalcValue(level), 1 << Constants.PlayerLayer))
        {
            PlayerAvatar p = c.GetComponentInParent<PlayerAvatar>();
            if (p && p != controller)
                FireProjectiles(projectilePrefab, projectileSpawn.position, new Vector3(0, Vector3.SignedAngle(Vector3.forward, p.transform.position - controller.transform.position, Vector3.up), 0), level);
        }
    }


    [Server]
    protected void FireProjectiles(Projectile prefab, Vector3 spawnPos, Vector3 spawnRot, int level)
    {
        Projectile b = Instantiate(prefab, spawnPos, Quaternion.Euler(spawnRot));
        NetworkServer.Spawn(b.gameObject);

        OnSpawnProjectile(b, level);
    }

    [Server]
    protected virtual void OnSpawnProjectile(Projectile b, int level)
    {
        b.SetSource(controller);

        b.SetSpeed(controller.currSpeed + speedMod);
        b.HitTarget = onHitTarget;

        ((FetchProjectile)b).SetFetchVals(fetchDuration.CalcValue(level), fetchSpeed.CalcValue(level));
    }
}
