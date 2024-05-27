using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TargetAbility : AbilityBase
{
    protected bool showIndicator = false;
    protected Vector3 currReticlePos;
    protected int currLevel = 0;

    [Header("Target Reticle")]
    [SerializeField] protected TargetReticle reticle;
    [SerializeField] protected AbilityUpgrade range;
    [SerializeField] protected AbilityUpgrade radius;
    [SerializeField] protected bool forceMaxRange;

    [SerializeField] protected ParticleSystem splashEffect;
    [Tooltip("0 to not scale")] [SerializeField] protected float baseScale = 0;

    private void Update()
    {
        if (showIndicator)
        {
            reticle?.SetReticle(true);
            SetIndicator(radius.CalcValue(currLevel), range.CalcValue(currLevel));
        }
        else
        {
            reticle?.SetReticle(false);
        }
    }

    public override void QueueAbility(int level)
    {
        if (!CanUseAbility(level))
            return;

        if (abilityQueued)
            CancelAbility();
        else
        {
            currLevel = level;
            showIndicator = true;
            abilityQueued = true;
        }
    }

    public override void CancelAbility()
    {
        showIndicator = false;

        base.CancelAbility();
    }

    protected void SetIndicator(float size, float range)
    {
        if (!reticle)
        {
            UseAbility(currLevel);
            showIndicator = false;
            currReticlePos = controller.transform.position;
            return;
        }

        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100, 1 << Constants.GroundLayer);
        Vector3 toTarget = new Vector3(hit.point.x, 0, hit.point.z) - transform.position;
        if (forceMaxRange ||
           (range >= 0 && toTarget.magnitude > range))
            toTarget = toTarget.normalized * range;

        currReticlePos = transform.position + toTarget;
        DrawIndicator(size);

        if (InputController.Instance.fire.down)
        {
            UseAbility(currLevel);
            reticle.SetReticle(false);
            showIndicator = false;
        }
    }

    protected virtual void DrawIndicator(float size)
    {
        reticle.SetReticleCenter(currReticlePos);
        reticle.DrawCircle(size);
    }

    protected override void DoEffect(int level)
    {
        DoServerEffect(currReticlePos, level);
    }

    public override void OnDoClientEffect(Vector3 target, int level)
    {
        ParticleSystem.ShapeModule shape;
        if (effect)
        {
            shape = effect.shape;
            shape.radius = radius.CalcValue(level);
        }

        base.OnDoClientEffect(target, level);

        if (!splashEffect) return;

        if (baseScale == 0)
        {
            shape = splashEffect.shape;
            shape.radius = radius.CalcValue(level) - 2.5f;
        }
        else
        {
            float s = baseScale * radius.CalcValue(level) * 2;
            splashEffect.transform.localScale = new Vector3(s, s, s);
        }
        PlaceEffect(splashEffect, target + Vector3.up * .1f, level);
    }

    protected override void PlaceEffect(ParticleSystem effect, Vector3 target, int level)
    {
        effect.transform.parent = null;
        effect.transform.position = target;
        base.PlaceEffect(effect, target, level);
    }
}
