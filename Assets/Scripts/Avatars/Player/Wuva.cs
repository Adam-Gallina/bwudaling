using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Wuva : PlayerAvatar
{
    [Header("Worship")]
    [SerializeField] private AbilityUpgrade worshipDiameter;
    //[SerializeField] private AbilityUpgrade worshipShield;
    private bool showWorshipIndicator = false;

    [Header("Shield")]
    [SerializeField] private AbilityUpgrade shieldCooldown;
    [SerializeField] private AbilityUpgrade shieldDiameter;
    private bool showShieldIndicator = false;
    private bool shieldRecharging = true;

    [Header("Reflect")]
    [SerializeField] private AbilityUpgrade reflectDiameter;
    [SerializeField] private AbilityUpgrade reflectSlowMod;
    [SerializeField] private AbilityUpgrade reflectSlowDuration;
    private bool showReflectIndicator = false;

    protected override void Update()
    {
        base.Update();

        if (!hasAuthority)
            return;
        
        if (!shieldRecharging && shield == 0)
            shieldRecharging = true;
        else if (!shieldRecharging && shield > 0)
            nextSpecial2 = Time.time + special2Cooldown + shieldCooldown.CalcValue(player.abilities.special2Level);
    }

    protected override void CheckInput()
    {
        base.CheckInput();

        if (showWorshipIndicator)
        {
            showWorshipIndicator = CheckShowIndicator();
            ShowIndicator(showWorshipIndicator, worshipDiameter.CalcValue(player.abilities.special1Level), worshipDiameter.CalcValue(player.abilities.special1Level) / 2, DoWorship);
        }

        if (showShieldIndicator)
        {
            showShieldIndicator = CheckShowIndicator();
            ShowIndicator(showShieldIndicator, shieldDiameter.CalcValue(player.abilities.special2Level), shieldDiameter.CalcValue(player.abilities.special2Level) / 2, DoShield);
        }

        if (showReflectIndicator)
        {
            showReflectIndicator = CheckShowIndicator();
            ShowIndicator(showReflectIndicator, reflectDiameter.CalcValue(player.abilities.special3Level), reflectDiameter.CalcValue(player.abilities.special3Level), DoReflect);
        }

        showingIndicator = showWorshipIndicator || showReflectIndicator;
    }

    // Worship
    protected override void DoSpecial1(int level)
    {
        if (!showReflectIndicator && (CheckSpecial(nextSpecial1) || BwudalingNetworkManager.Instance.DEBUG_IgnoreCooldown))
        {
            showWorshipIndicator = true;
        }
    }

    private void DoWorship()
    {
        stats?.AddAbility();

        nextSpecial1 = Time.time + special1Cooldown;
        showWorshipIndicator = false;

        Collider[] players = Physics.OverlapSphere(currReticlePos, worshipDiameter.CalcValue(player.abilities.special3Level), 1 << Constants.PlayerLayer);

        foreach (Collider c in players)
        {
            PlayerAvatar p = c.GetComponent<PlayerAvatar>();
            if (p)
            {
                if (p.dead)
                    stats?.AddRescue();

                CmdHealTarget(p);//, (int)worshipShield.CalcValue(player.abilities.special1Level));
            }
        }

        //RpcOnWorship(worshipIndicator.position, worshipDiameter.CalcValue(player.abilities.special3Level));
    }

    [ClientRpc]
    private void RpcOnWorship(Vector3 targetPos, float radius)
    {
        // Not called by Command
    }

    // Shield
    protected override void DoSpecial2(int level)
    {
        if (!showShieldIndicator && ((CheckSpecial(nextSpecial2) && shield == 0) || BwudalingNetworkManager.Instance.DEBUG_IgnoreCooldown))
        {
            showShieldIndicator = true;
            //CmdHealTarget(this, 1);
            //nextSpecial2 = Time.time + special2Cooldown;
        }
    }

    private void DoShield()
    {
        stats?.AddAbility();
        showShieldIndicator = false;
        nextSpecial2 = Time.time + special2Cooldown + shieldCooldown.CalcValue(player.abilities.special2Level);

        Collider[] players = Physics.OverlapSphere(currReticlePos, shieldDiameter.CalcValue(player.abilities.special2Level), 1 << Constants.PlayerLayer);

        foreach (Collider c in players)
        {
            PlayerAvatar p = c.GetComponent<PlayerAvatar>();
            if (p && !p.dead)
            {
                CmdHealTarget(p, 1);
            }
        }
    }

    [ClientRpc]
    private void RpcOnShield()
    {
        //Not called by Command
    }

    // Reflect
    protected override void DoSpecial3(int level)
    {
        if (!showWorshipIndicator && (CheckSpecial(nextSpecial3) || BwudalingNetworkManager.Instance.DEBUG_IgnoreCooldown))
        {
            showReflectIndicator = true;
        }
    }

    private void DoReflect()
    {
        stats?.AddAbility();

        showReflectIndicator = false;
        nextSpecial3 = Time.time + special3Cooldown;
        CmdDoReflect(currReticlePos, player.abilities.special3Level);
    }

    [Command]
    private void CmdDoReflect(Vector3 pos, int level)
    {
        foreach (Collider c in Physics.OverlapSphere(pos, reflectDiameter.CalcValue(level), 1 << Constants.HazardLayer))
        {
            BasicSaw s = c.GetComponentInParent<BasicSaw>();
            if (s)
            {
                s.SetDirection(c.transform.position - transform.position);
                s.ApplySpeedMod(reflectSlowMod.CalcValue(level), reflectSlowDuration.CalcValue(level));
            }
        }

        RpcOnReflect();
    }

    [ClientRpc]
    private void RpcOnReflect()
    {

    }
}
