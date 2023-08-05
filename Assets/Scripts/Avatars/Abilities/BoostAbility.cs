using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostAbility : AbilityBase
{
    [SerializeField] private AbilityUpgrade speedMod;
    [SerializeField] private AbilityUpgrade duration;

    [SerializeField] protected bool invulnerable = false;

    protected override bool OnUseAbility(int level)
    {
        if (invulnerable)
        {
            controller.SetInvulnerable(true);
            Invoke(nameof(ClearInvulnerable), duration.CalcValue(level));
        }
        
        controller.CmdApplySpeedMod(speedMod.CalcValue(level), duration.CalcValue(level));

        return true;
    }

    private void ClearInvulnerable() { controller.SetInvulnerable(false); }

    protected override float CalcNextAbility(int level)
    {
        return base.CalcNextAbility(level) + duration.CalcValue(level);
    }
}
