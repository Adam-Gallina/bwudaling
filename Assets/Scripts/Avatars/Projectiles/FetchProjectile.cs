using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class FetchProjectile : Projectile
{
    private float fetchDur;
    private float fetchSpe;

    public void SetFetchVals(float duration, float speed)
    {
        fetchDur = duration;
        fetchSpe = speed;
    }

    [Server]    
    protected override void OnHitTarget(Collider other)
    {
        PlayerAvatar p = other.GetComponentInParent<PlayerAvatar>();

        if (p && p.dead)
        {
            p.DragEffect(spawner.transform, fetchDur, fetchSpe);

            base.OnHitTarget(other);
        }
    }
}
