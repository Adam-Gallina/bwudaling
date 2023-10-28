using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AvatarBase : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnDeadChanged))]
    [HideInInspector] public bool dead;

    [SyncVar(hook = nameof(OnShieldChanged))]
    [HideInInspector] public int shield;
    [SerializeField] protected float shieldDur;
    [SerializeField] protected GameObject shieldModel;
    protected float nextHit;
    public bool canDamage { get; protected set; } = true;
    [SyncVar]
    protected bool invulnerable = false;


    protected Rigidbody rb;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnDeadChanged(bool _, bool dead)
    {
        if (dead)
            OnDeath();
        else
            OnHeal();
    }

    protected virtual void OnShieldChanged(int _, int shield)
    {
        shieldModel.SetActive(shield > 0);
    }

    [Command]
    public void SetInvulnerable(bool invulnerable) { this.invulnerable = invulnerable; }

    [Server]
    public virtual void SetShield(int shieldCount)
    {
        if (shieldCount > shield)
            shield = shieldCount;
    }

    [Server]
    public virtual void Heal()
    {
        if (dead)
            dead = false;
    }

    protected virtual void OnHeal()
    {

    }

    [Server]
    public virtual void Damage()
    {
        if (dead)
            return;

        if (!canDamage || invulnerable)
            return;

        if (Time.time <= nextHit)
            return;

        if (shield > 0)
        {
            shield--;
            nextHit = Time.time + shieldDur;
            return;
        }

        dead = true;
    }

    protected virtual void OnDeath()
    {

    }
}
