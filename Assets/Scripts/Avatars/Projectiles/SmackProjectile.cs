using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmackProjectile : RicochetProjectile
{
    [SerializeField] private RangeF spinSpeed;

    private void Start()
    {
        rb.AddTorque(new Vector3(0, Mathf.Deg2Rad * spinSpeed.RandomVal, 0) * Mathf.Sign(Random.Range(-1, 1)), ForceMode.VelocityChange);
    }

    [Server]
    protected override void OnHitTarget(Collider other)
    {
        BasicSaw s = other.GetComponentInParent<BasicSaw>();
        if (s)
        {
            NetworkServer.Destroy(s.gameObject);
        }

        base.OnHitTarget(other);
    }
}
