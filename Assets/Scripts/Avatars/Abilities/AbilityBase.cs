using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbilityBase : MonoBehaviour
{
    protected PlayerAvatar controller;

    public string abilityName;
    [SerializeField] private string abilityTooltip;
    [SerializeField] private Sprite abilityIcon;
    protected float nextAbility = 0;
    [SerializeField] private AbilityUpgrade abilityCooldown;

    protected bool abilityQueued = false;

    //[Header("UI")]
    protected AbilityCooldown cooldownUI;

    public void SetController(PlayerAvatar controller)
    {
        this.controller = controller;
    }

    public void LinkUI(AbilityCooldown abilityCooldown, TooltipController tooltip) 
    { 
        cooldownUI = abilityCooldown;
        cooldownUI.abilityImage.sprite = abilityIcon;
        tooltip.tooltip = abilityTooltip;
    }


    public virtual void QueueAbility(int level)
    {
        if (level == -1)
            return;

        abilityQueued = true;
        UseAbility(level);
    }

    protected void UseAbility(int level)
    {
        if (nextAbility < Time.time || BwudalingNetworkManager.Instance.DEBUG_IgnoreCooldown)
        {
            controller.stats?.AddAbility();

            OnUseAbility(level);
            nextAbility = Time.time + CalcNextAbility(level);

            abilityQueued = false;
        }
    }
    protected abstract void OnUseAbility(int level);

    public virtual void CancelAbility()
    {
        abilityQueued = false;
    }


    protected virtual float CalcNextAbility(int level)
    {
        return abilityCooldown.CalcValue(level);
    }


    protected void DoServerAbility(int level) { controller.UseAbility(this, level); }
    [Server]
    public virtual void OnUseServerAbility(int level)
    {

    }

    [Client]
    public virtual void OnUseClientAbility(int level)
    {

    }


    public void UpdateUI(int level)
    {
        cooldownUI.SetCooldown(nextAbility - Time.time, CalcNextAbility(level));
    }
}
