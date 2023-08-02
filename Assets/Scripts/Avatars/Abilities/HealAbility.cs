using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class HealAbility : TargetAbility
{
    [SerializeField] private AbilityUpgrade shield;

    [SerializeField] private bool canTargetLiving;
    [SerializeField] private bool canTargetDead;

    protected override void OnUseAbility(int level)
    {
        if (controller.dead)
            controller.CmdHealTarget(controller, (int)shield.CalcValue(level));

        Collider[] players = Physics.OverlapSphere(currReticlePos, radius.CalcValue(level), 1 << Constants.PlayerLayer);
        foreach (Collider c in players)
        {
            PlayerAvatar p = c.GetComponent<PlayerAvatar>();
            if (p && (p.dead ? canTargetDead : canTargetLiving))
            {
                if (p.dead)
                    controller.stats?.AddRescue();

                controller.CmdHealTarget(p, (int)shield.CalcValue(level));
            }
        }
    }
}
