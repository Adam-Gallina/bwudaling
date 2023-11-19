using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : NetworkBehaviour
{
    protected AvatarBase spawner;

    [SerializeField] protected LayerMask targetLayer;

    //[SerializeField] protected GameObject hitEffect;

    [HideInInspector] public UnityEvent HitTarget;

    [SerializeField] protected bool restrictTargetCollisionsToServer = true;

    protected float speed = 250;

    protected Rigidbody rb;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    [Server]
    public virtual void SetSpeed(float speed)
    {
        this.speed = speed;
        SetDirection(transform.forward);
    }

    [Server]
    public virtual void SetDirection(Vector3 direction)
    {
        rb.velocity = direction.normalized * speed;
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        if (restrictTargetCollisionsToServer && !isServer)
            return;

        if (spawner && other.GetComponentInParent<AvatarBase>() == spawner)
            return;

        if (((1 << other.gameObject.layer) & targetLayer.value) > 0)
        {
            OnHitTarget(other);
        }

        if (!isServer)
            return;

        if (other.gameObject.layer == Constants.EnvironmentLayer)
        {
            OnHitWall(other);
        }
    }

    [Server]
    public void SetSource(AvatarBase spawner) { SetSource(spawner, targetLayer); }
    [Server]
    public void SetSource(AvatarBase spawner, LayerMask targetLayer)
    {
        this.spawner = spawner;
        this.targetLayer = targetLayer;
    }

    protected virtual void OnHitTarget(Collider other)
    {
        if (restrictTargetCollisionsToServer && !isServer)
            return;

        if (isServer)
            DestroyObject();
        HitTarget?.Invoke();
    }

    [Server]
    protected virtual void OnHitWall(Collider other)
    {
        DestroyObject();
    }

    [Server]
    public virtual void DestroyObject(bool playDeathAnim=true)
    {
        NetworkServer.Destroy(gameObject);
    }
}
