using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ShrinkProjectile : Projectile
{
    [HideInInspector] public float sizeDebuff;
    [HideInInspector] public float debuffDuration;

    [SerializeField] private RangeF spinSpeed;

    private void Start()
    {
        rb.AddTorque(new Vector3(0, Mathf.Deg2Rad * spinSpeed.RandomVal, 0) * Mathf.Sign(Random.Range(-1, 1)), ForceMode.VelocityChange);
    }

    [Server]
    protected override void OnHitTarget(Collider other)
    {
        other.GetComponentInParent<BasicSaw>()?.ApplySizeMod(sizeDebuff, debuffDuration);

        base.OnHitTarget(other);
    }
}
