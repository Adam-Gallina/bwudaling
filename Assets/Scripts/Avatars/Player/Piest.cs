using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Piest : PlayerAvatar
{
    [SerializeField] private Transform bulletSpawn;
    [SerializeField] private float bulletSpread;

    [Header("Heal Bullet")]
    [SerializeField] private Projectile healBulletPrefab;
    [SerializeField] private AbilityUpgrade bulletCount;
    [SerializeField] private AbilityUpgrade bulletSpeed;
    [SerializeField] private AbilityUpgrade bulletBounce;

    [Header("Teleport")]
    [SerializeField] private AbilityUpgrade tpDist;
    [SerializeField] private AbilityUpgrade tpCooldown;
    private bool showTpIndicator;

    [Header("Saw Shrink")]
    [SerializeField] private Projectile shrinkBulletPrefab;
    [SerializeField] private AbilityUpgrade shrinkBulletCount;
    [SerializeField] private AbilityUpgrade shrinkBulletSpeed;
    [SerializeField] private AbilityUpgrade shrinkSize;
    [SerializeField] private AbilityUpgrade shrinkDuration;

    protected override void CheckInput()
    {
        base.CheckInput();

        if (showTpIndicator)
        {
            showingIndicator = showTpIndicator = CheckShowIndicator();
            ShowIndicator(showTpIndicator, .5f, tpDist.CalcValue(player.abilities.special2Level), DoTeleport);
        }
    }

    // Heal Bullet
    protected override void DoSpecial1(int level)
    {
        if (CheckSpecial(nextSpecial1) || BwudalingNetworkManager.Instance.DEBUG_IgnoreCooldown)
        {
            stats?.AddAbility();

            CmdSpawnHealBullets(level);
            nextSpecial1 = Time.time + special1Cooldown;
        }
    }

    [Command]
    private void CmdSpawnHealBullets(int level)
    {
        FireProjectiles(healBulletPrefab, bulletSpawn.position, bulletSpawn.eulerAngles, (int)bulletCount.CalcValue(level), bulletSpread, level, OnSpawnHealBullet);

        RpcOnFireHealBullet();
    }

    [Server]
    private void OnSpawnHealBullet(Projectile b, int level)
    {
        HealProjectile hp = (HealProjectile)b;
        hp.SetSpeed(bulletSpeed.CalcValue(level));
        hp.SetMaxBounces((int)bulletBounce.CalcValue(level));
        hp.HitTarget += RpcStatsHealPlayer;
    }

    [ClientRpc]
    private void RpcOnFireHealBullet()
    {

    }

    // Teleport
    protected override void DoSpecial2(int level)
    {
        if (CheckSpecial(nextSpecial2) || BwudalingNetworkManager.Instance.DEBUG_IgnoreCooldown)
        {
            showTpIndicator = true;
        }
    }

    private void DoTeleport()
    {
        if (!BwudalingNetworkManager.Instance.DEBUG_TpWalls)
        {
            Collider[] colls = Physics.OverlapSphere(currReticlePos, 0.1f, 1 << Constants.TeleportAreaLayer);
            if (colls.Length == 0)
                return;
            foreach (Collider c in colls)
            {
                TeleportArea tp = c.GetComponent<TeleportArea>();
                if (tp && !tp.validTp)
                    return;
            }
        }

        stats?.AddAbility();

        nextSpecial2 = Time.time + special2Cooldown - tpCooldown.CalcValue(player.abilities.special2Level);
        showTpIndicator = false;

        transform.position = currReticlePos;
        targetPos = transform.position;
        CmdOnTeleport(currReticlePos);
    }

    [Command]
    private void CmdOnTeleport(Vector3 pos)
    {
        RpcOnTeleport(transform.position, pos);
    }

    [ClientRpc]
    private void RpcOnTeleport(Vector3 start, Vector3 target)
    {

    }

    // Shrink saws
    protected override void DoSpecial3(int level)
    {
        if (CheckSpecial(nextSpecial3) || BwudalingNetworkManager.Instance.DEBUG_IgnoreCooldown)
        {
            stats?.AddAbility();

            CmdFireShrinkBullets(level);
            nextSpecial3 = Time.time + special3Cooldown;
        }
    }

    [Command]
    private void CmdFireShrinkBullets(int level)
    {
        FireProjectiles(shrinkBulletPrefab, bulletSpawn.position, bulletSpawn.eulerAngles, (int)shrinkBulletCount.CalcValue(level), bulletSpread, level, OnSpawnShrinkBullet);

        RpcOnFireShrinkBullet();
    }

    [Server]
    private void OnSpawnShrinkBullet(Projectile b, int level)
    {
        ShrinkProjectile sp = (ShrinkProjectile)b;
        sp.SetSpeed(bulletSpeed.CalcValue(level));

        sp.sizeDebuff = shrinkSize.CalcValue(level);
        sp.debuffDuration = shrinkDuration.CalcValue(level);
    }

    [ClientRpc]
    private void RpcOnFireShrinkBullet()
    {

    }
}
