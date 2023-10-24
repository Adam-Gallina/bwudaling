using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FetchProjectile : Projectile
{
    private float fetchDur;
    private float fetchSpe;

    [SerializeField] private RangeF SpinSpeed;
    private float spinSpeed;

    [SerializeField] private Transform rotTarget;

    private void Start()
    {
        spinSpeed = SpinSpeed.RandomVal * Mathf.Sign(Random.Range(-1, 1));
    }

    protected void Update()
    {
        rotTarget.localEulerAngles = new Vector3(0, rotTarget.localEulerAngles.y + spinSpeed * Time.deltaTime, 0);
    }

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
