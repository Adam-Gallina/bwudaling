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
    [SerializeField] private TargetReticle reticle;
    [SerializeField] protected AbilityUpgrade range;
    [SerializeField] protected AbilityUpgrade radius;

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
        if (level == -1)
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
            return;
        }

        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100, 1 << Constants.GroundLayer);
        Vector3 toTarget = new Vector3(hit.point.x, 0, hit.point.z) - transform.position;
        if (range > 0 && toTarget.magnitude > range)
            toTarget = toTarget.normalized * range;

        currReticlePos = transform.position + toTarget;
        reticle.SetReticleCenter(currReticlePos);
        reticle.DrawCircle(size);

        if (InputController.Instance.fire.down)
        {
            UseAbility(currLevel);
            reticle.SetReticle(false);
            showIndicator = false;
        }
    }
}
