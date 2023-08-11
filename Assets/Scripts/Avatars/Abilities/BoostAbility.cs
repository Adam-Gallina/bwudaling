using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostAbility : AbilityBase
{
    [SerializeField] private AbilityUpgrade speedMod;
    [SerializeField] private AbilityUpgrade duration;

    [SerializeField] protected bool invulnerable = false;
    [SerializeField] protected float invulnerabilityPadding = 0.75f;

    protected override bool OnUseAbility(int level)
    {
        if (invulnerable)
        {
            controller.SetInvulnerable(true);
            Invoke(nameof(EndInvulnerability), duration.CalcValue(level));
        }
        
        controller.CmdApplySpeedMod(speedMod.CalcValue(level), duration.CalcValue(level));
        if (abilityEnd != null)
            Invoke(nameof(OnEndAbility), duration.CalcValue(level) - invulnerabilityPadding);

        return true;
    }

    private void EndInvulnerability() { controller.SetInvulnerable(false); }

    protected override float CalcNextAbility(int level)
    {
        return base.CalcNextAbility(level) + duration.CalcValue(level);
    }

    public override void UpdateUI(int level)
    {
        float actualCooldown = abilityCooldown.CalcValue(level);
        cooldownUI.SetCooldown(nextAbility - Time.time, actualCooldown);
    }
}
