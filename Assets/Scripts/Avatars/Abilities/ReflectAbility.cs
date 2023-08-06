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
        DoServerAbility(level);

        return true;
    }


    [Server]
    public override void OnUseServerAbility(int level)
    {
        //Vector3 toArcCenter = currReticlePos - transform.position;

        foreach (Collider c in Physics.OverlapSphere(currReticlePos, radius.CalcValue(level), 1 << Constants.HazardLayer))
        {
            BasicSaw s = c.GetComponentInParent<BasicSaw>();
            if (s)
            {
                /*Vector3 toSaw = c.transform.position - transform.position;
                if (Vector3.Angle(toArcCenter, toSaw) > angle / 2)
                    continue;*/

                s.SetDirection(c.transform.position - transform.position);
                s.ApplySpeedMod(reflectSlowMod.CalcValue(level), reflectSlowDuration.CalcValue(level));
            }
        }
    }
}
