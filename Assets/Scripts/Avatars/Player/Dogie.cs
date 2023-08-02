using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Dogie : PlayerAvatar
{
    [SerializeField] private Transform bulletSpawn;

    [Header("Fetch")]
    [SerializeField] private Projectile fetchBullet;
    [SerializeField] private float fetchBulletSpeed = 7.5f;
    [SerializeField] private AbilityUpgrade fetchDuration;
    [SerializeField] private AbilityUpgrade fetchSpeed;
    private PlayerAvatar fetchTarget;
    private float fetchEnd;
    //This is dumb. Find a better way
    private float fetchDur;
    private float fetchSpe;

    [Header("Dig")]
    [SerializeField] private AbilityUpgrade digSpeedMod;
    [SerializeField] private AbilityUpgrade digDuration;
    [SyncVar]
    private bool digging = false;
    private float digEnd;

    [Header("Bark")]
    [SerializeField] private AbilityUpgrade barkRadius;
    [SerializeField] private AbilityUpgrade barkStunDuration;
    private bool showBarkIndicator = false;

    protected override void Update()
    {
        if (hasAuthority && digging && Time.time > digEnd) 
        {
            CmdEndDig();
        }
        else if (digging)
        {
            nextSpecial2 = Time.time + special2Cooldown;
        }

        base.Update();
    }

    protected override void CheckInput()
    {
        base.CheckInput();

        if (showBarkIndicator)
        {
            showingIndicator = showBarkIndicator = CheckShowIndicator();
            ShowIndicator(showBarkIndicator, barkRadius.CalcValue(player.abilities.special3Level), barkRadius.CalcValue(player.abilities.special3Level), DoBark);
        }
    }

    protected override float CalcSpeed()
    {
        if (digging)
            return base.CalcSpeed() * digSpeedMod.CalcValue(player.abilities.special2Level);

        return base.CalcSpeed();
    }

    [Server]
    public override void Damage()
    {
        if (!digging)
            base.Damage();
    }

    protected override void OnDeath()
    {
        if (!digging)
            base.OnDeath();
    }


    // Fetch
    protected override void DoSpecial1(int level)
    {
        if (CheckSpecial(nextSpecial1) || BwudalingNetworkManager.Instance.DEBUG_IgnoreCooldown)
        {
            stats?.AddAbility();

            nextSpecial1 = Time.time + special1Cooldown;

            CmdSetFetchVals(level);
            CmdSpawnFetchBullet(level);
        }
    }

    [Command]
    private void CmdSpawnFetchBullet(int level)
    {
        FireProjectiles(fetchBullet, bulletSpawn.position, bulletSpawn.eulerAngles, 1, 0, level, OnSpawnFetchBullet);

        RpcOnFireFetchBullet();
    }

    [Command]
    private void CmdSetFetchVals(int level)
    {
        fetchDur = fetchDuration.CalcValue(level);
        fetchSpe = fetchSpeed.CalcValue(level);
    }

    [Server]
    private void OnSpawnFetchBullet(Projectile b, int level)
    {
        b.SetSpeed(fetchBulletSpeed);
        b.SetSource(this, 1 << Constants.PlayerLayer);
    }

    [ClientRpc]
    private void RpcOnFireFetchBullet()
    {

    }

    [Server]
    public void SetFetchTarget(PlayerAvatar target)
    {
        target.DragEffect(transform, fetchDur, fetchSpe);
    }

    // Dig
    protected override void DoSpecial2(int level)
    {
        if (CheckSpecial(nextSpecial2) || BwudalingNetworkManager.Instance.DEBUG_IgnoreCooldown)
        {
            stats?.AddAbility();

            digEnd = Time.time + digDuration.CalcValue(level);

            CmdStartDig();
        }
    }

    [Command]
    private void CmdStartDig()
    {
        digging = true;

        RpcOnDigStart();
    }

    [ClientRpc]
    private void RpcOnDigStart()
    {
    }

    [Command]
    private void CmdEndDig()
    {
        digging = false;

        RpcOnDigEnd();
    }

    [ClientRpc]
    private void RpcOnDigEnd()
    {
    }

    // Bark
    protected override void DoSpecial3(int level)
    {
        if (CheckSpecial(nextSpecial3) || BwudalingNetworkManager.Instance.DEBUG_IgnoreCooldown)
        {
            showBarkIndicator = true;
        }
    }

    private void DoBark()
    {
        stats?.AddAbility();

        nextSpecial3 = Time.time + special3Cooldown;
        showBarkIndicator = false;
        CmdDoBark(currReticlePos, player.abilities.special3Level);
    }

    [Command]
    private void CmdDoBark(Vector3 pos, int level)
    {
        foreach (Collider c in Physics.OverlapSphere(pos, barkRadius.CalcValue(level), 1 << Constants.HazardLayer))
        {
            BasicSaw saw = c.GetComponentInParent<BasicSaw>();
            if (saw)
            {
                saw.ApplySpeedMod(0, barkStunDuration.CalcValue(level));
            }
        }

        RpcOnBark();
    }

    [ClientRpc]
    private void RpcOnBark()
    {

    }
}
