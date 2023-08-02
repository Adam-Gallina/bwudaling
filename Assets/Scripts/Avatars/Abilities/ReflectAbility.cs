using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReflectAbility : TargetAbility
{
    [SerializeField] private AbilityUpgrade reflectSlowMod;
    [SerializeField] private AbilityUpgrade reflectSlowDuration;

    protected override void OnUseAbility(int level)
    {
        DoServerAbility(level);
    }


    [Server]
    public override void OnUseServerAbility(int level)
    {
        foreach (Collider c in Physics.OverlapSphere(transform.position, radius.CalcValue(level), 1 << Constants.HazardLayer))
        {
            BasicSaw s = c.GetComponentInParent<BasicSaw>();
            if (s)
            {
                s.SetDirection(c.transform.position - transform.position);
                s.ApplySpeedMod(reflectSlowMod.CalcValue(level), reflectSlowDuration.CalcValue(level));
            }
        }
    }
}
