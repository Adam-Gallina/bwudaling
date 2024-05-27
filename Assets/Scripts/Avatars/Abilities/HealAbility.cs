using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class HealAbility : TargetAbility
{
    [SerializeField] private AbilityUpgrade shield;

    [SerializeField] private bool canTargetLiving;
    [SerializeField] private bool canTargetDead;

    protected override bool OnUseAbility(int level)
    {
        bool hit = false;

        if (controller.dead ? canTargetDead : canTargetLiving)
        {
            controller.CmdHealTarget(controller, (int)shield.CalcValue(level));
            hit = true;
        }

        Debug.Log(currReticlePos);
        Collider[] players = Physics.OverlapSphere(currReticlePos, radius.CalcValue(level), 1 << Constants.PlayerLayer);
        foreach (Collider c in players)
        {
            PlayerAvatar p = c.GetComponent<PlayerAvatar>();
            Debug.Log(c + " " + p + " " + c.GetComponentInParent<PlayerAvatar>());
            if (p && (p.dead ? canTargetDead : canTargetLiving))
            {
                if (p.dead)
                    controller.stats?.AddRescue();

                controller.CmdHealTarget(p, (int)shield.CalcValue(level));
                hit = true;
            }
        }

        return hit;
    }
}
