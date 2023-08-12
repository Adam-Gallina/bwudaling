using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReflectAbility : TargetAbility
{
    [SerializeField] private AbilityUpgrade reflectSlowMod;
    [SerializeField] private AbilityUpgrade reflectSlowDuration;

    protected override bool OnUseAbility(int level)
    {
        DoServerAbility(currReticlePos, level);

        return true;
    }


    [Server]
    public override void OnUseServerAbility(Vector3 target, int level)
    {
        foreach (Collider c in Physics.OverlapSphere(target, radius.CalcValue(level), 1 << Constants.HazardLayer))
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
