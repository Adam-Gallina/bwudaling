using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RicochetProjectile : Projectile
{
    [SerializeField] protected LayerMask costRicochetLayers;
    [SerializeField] protected LayerMask freeRicochetLayers;
    public int maxBounces;
    [HideInInspector] public int bounces;

    protected Vector3 currVelocity;

    public override void OnStartServer()
    {
        bounces = maxBounces;
    }

    public override void SetDirection(Vector3 direction)
    {
        base.SetDirection(direction);
        currVelocity = direction.normalized * speed;
    }

    [Server]
    public void SetMaxBounces(int bounces)
    {
        if (bounces > -1)
            this.bounces = bounces - (maxBounces - this.bounces);
        maxBounces = bounces;
    }

    [ServerCallback]
    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.collider.gameObject.layer) & (costRicochetLayers.value | freeRicochetLayers.value)) > 0)
        {
            currVelocity = Vector3.Reflect(currVelocity, collision.contacts[0].normal);
            rb.velocity = currVelocity.normalized * speed;

            if (((1 << collision.collider.gameObject.layer) & costRicochetLayers.value) > 0)
                OnHitWall(collision.GetContact(0).otherCollider);
        }
    }

    [ServerCallback]
    protected virtual void Update()
    {
        if (currVelocity != Vector3.zero)
            rb.velocity = currVelocity.normalized * speed;
    }

    [ServerCallback]
    private void LateUpdate()
    {
        if (rb.velocity != Vector3.zero)
            currVelocity = rb.velocity;
    }

    protected override void OnHitWall(Collider other)
    {
        if (maxBounces == -1)
            return;

        bounces -= 1;
        if (bounces < 0)
            base.OnHitWall(other);
    }
}
