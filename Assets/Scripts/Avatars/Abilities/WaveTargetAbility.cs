using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WaveTargetAbility : TargetAbility
{
    [SerializeField] protected float angle;
    [SerializeField] private float innerRadius = 0.75f;

    protected override void DrawIndicator(float size)
    {
        reticle.SetReticleCenter(transform.position);
        reticle.DrawArc(currReticlePos - transform.position, angle, size, innerRadius);
    }
}
