using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Bwuda : PlayerAvatar
{
    [Header("Resurrect")]
    [SerializeField] private AbilityUpgrade resurrectCooldown;
    [SerializeField] private AbilityUpgrade resurrectShield;

    [Header("Dab Run")]
    [SerializeField] private AbilityUpgrade dabDuration;
    [SerializeField] private AbilityUpgrade dabSpeedMod;
    private float dabEnd = 0;
    [SyncVar]
    private bool dabbing = false;

    [Header("Saw Smack")]
    [SerializeField] private AbilityUpgrade smackRadius;
    private bool showSmackIndicator = false;

    protected override void Update()
    {
        if (hasAuthority)
        {
            if (inp.special1.down && player.abilities.special1Level != -1)
                DoSpecial1(player.abilities.special1Level);

            if (Time.time > dabEnd)
                CmdDabEnd();
            else
                nextSpecial2 = Time.time + special2Cooldown;
        }

        base.Update();
    }

    protected override void CheckInput()
    {
        base.CheckInput();

        if (showSmackIndicator)
        {
            showSmackIndicator = CheckShowIndicator();
            ShowIndicator(showSmackIndicator, smackRadius.CalcValue(player.abilities.special3Level), 0, DoSmack);
        }
    }

    public override void Damage()
    {
        if (!dabbing)
            base.Damage();
    }

    // Resurrect
    protected override void DoSpecial1(int level)
    {
        if (CheckSpecial(nextSpecial1) || BwudalingNetworkManager.Instance.DEBUG_IgnoreCooldown)
        {
            if (dead)
            {
                stats?.AddAbility();

                nextSpecial1 = Time.time + special1Cooldown + resurrectCooldown.CalcValue(level);
                CmdHealTarget(this, 1);// (int)resurrectShield.CalcValue(level));
                stats?.AddRescue();
            }
        }
    }

    // Dab run
    protected override void DoSpecial2(int level)
    {
        if (CheckSpecial(nextSpecial2) || BwudalingNetworkManager.Instance.DEBUG_IgnoreCooldown)
        {
            stats?.AddAbility();

            dabEnd = Time.time + dabDuration.CalcValue(level);

            CmdDabStart();
        }
    }

    [Command]
    private void CmdDabStart()
    {
        dabbing = true;

        RpcDabStart();
    }

    [ClientRpc]
    private void RpcDabStart()
    {

    }

    [Command]
    private void CmdDabEnd()
    {
        dabbing = false;

        RpcDabEnd();
    }

    [ClientRpc]
    private void RpcDabEnd()
    {

    }

    protected override float CalcSpeed()
    {
        if (dabbing)
            return base.CalcSpeed() * dabSpeedMod.CalcValue(player.abilities.special2Level);
        return base.CalcSpeed();
    }

    // Saw Smack
    protected override void DoSpecial3(int level)
    {
        if (CheckSpecial(nextSpecial3) || BwudalingNetworkManager.Instance.DEBUG_IgnoreCooldown)
        {
            showSmackIndicator = true;
        }
    }

    private void DoSmack()
    {
        stats?.AddAbility();

        showSmackIndicator = false;
        nextSpecial3 = Time.time + special3Cooldown;
        CmdDoSmack(currReticlePos, smackRadius.CalcValue(player.abilities.special3Level));
    }

    [Command]
    private void CmdDoSmack(Vector3 pos, float radius)
    {
        foreach (Collider c in Physics.OverlapSphere(pos, radius, 1 << Constants.HazardLayer))
        {
            BasicSaw s = c.GetComponentInParent<BasicSaw>();
            if (s)
            {
                NetworkServer.Destroy(s.gameObject);
            }
        }
    }
}
