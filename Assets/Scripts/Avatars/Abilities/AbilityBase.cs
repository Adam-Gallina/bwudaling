using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UltEvents;

public abstract class AbilityBase : MonoBehaviour
{
    protected PlayerAvatar controller;

    public string abilityName;
    [SerializeField] private string abilityTooltip;
    [SerializeField] private string[] abilityUpgradesTooltip;
    [SerializeField] private Sprite abilityIcon;
    protected float nextAbility = 0;
    [SerializeField] protected AbilityUpgrade abilityCooldown;
    public bool canUseWhileDead = false;

    protected bool abilityQueued = false;

    //[Header("UI")]
    protected AbilityCooldown cooldownUI;

    [Header("Effects")]
    [SerializeField] protected ParticleSystem effect;

    [Header("Callbacks")]
    [SerializeField] protected UltEvent abilityStart;
    [SerializeField] protected UltEvent abilityEnd;

    public void SetController(PlayerAvatar controller) { this.controller = controller; }
    public void LinkUI(AbilityCooldown abilityCooldown, TooltipController tooltip) 
    { 
        cooldownUI = abilityCooldown;
        cooldownUI.abilityImage.sprite = abilityIcon;
        tooltip.tooltipName = abilityName;
        tooltip.tooltip = abilityTooltip;
        tooltip.upgrades = abilityUpgradesTooltip;
    }

    protected virtual bool CanUseAbility(int level)
    {
        if (level == -1)
            return false;

        return Time.time >= nextAbility || BwudalingNetworkManager.Instance.DEBUG_IgnoreCooldown;
    }

    public virtual void QueueAbility(int level)
    {
        if (!CanUseAbility(level))
            return;

        abilityQueued = true;
        UseAbility(level);
    }

    protected void UseAbility(int level)
    {
        if (CanUseAbility(level))
        {
            abilityStart?.Invoke();
            controller.stats?.AddAbility();

            DoEffect(level);
            if (OnUseAbility(level))
                nextAbility = Time.time + CalcNextAbility(level);

            abilityQueued = false;

        }
    }
    protected abstract bool OnUseAbility(int level);

    protected virtual void OnEndAbility()
    {
        abilityEnd?.Invoke();
    }

    public virtual void CancelAbility()
    {
        abilityQueued = false;
    }


    protected virtual float CalcNextAbility(int level)
    {
        return abilityCooldown.CalcValue(level);
    }


    protected void DoServerAbility(Vector3 target, int level) { controller.UseAbility(this, target, level); }
    [Server]
    public virtual void OnUseServerAbility(Vector3 target, int level)
    {

    }

    [Client]
    public virtual void OnUseClientAbility(Vector3 target, int level)
    {

    }

    protected virtual void DoEffect(int level)
    {
        DoServerEffect(transform.position, level);
    }
    protected void DoServerEffect(Vector3 target, int level) 
    {
        controller.DoEffect(this, target, level); 
    }
    [Client]
    public virtual void OnDoClientEffect(Vector3 target, int level)
    {
        if (effect)
            PlaceEffect(effect, target, level);
    }

    public void DoServerAudio()
    {
        controller.DoAbilityAudio(this);
    }
    [Client]
    public virtual void OnDoClientAudio()
    {
        GetComponent<AudioSource>().Play();
    }

    protected virtual void PlaceEffect(ParticleSystem effect, Vector3 target, int level)
    {
        effect.Play();
    }

    public virtual void UpdateUI(int level)
    {
        cooldownUI?.SetCooldown(nextAbility - Time.time, CalcNextAbility(level));
    }
}
