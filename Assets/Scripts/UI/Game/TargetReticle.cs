using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetReticle : MonoBehaviour
{
    [SerializeField] protected Transform reticle;
    [SerializeField] protected LineRenderer reticleLR;
    [SerializeField] protected int maxLRpoints = 180;

    private float lastOuterRadius;
    private float lastInnerRadius;
    private float lastAngle;

    private void Start()
    {
        reticleLR.positionCount = maxLRpoints;
        SetReticle(false);
    }

    public void SetReticle(bool show)
    {
        reticleLR.enabled = show;
    }

    public void SetReticleCenter(Vector3 center)
    {
        reticle.position = center;
    }

    public void DrawCircle(float radius)
    {
        if (radius == lastOuterRadius)
            return;
        lastOuterRadius = radius;
        reticleLR.positionCount = maxLRpoints;

        Vector2 v = Vector2.up * radius;
        float ang = 360f / maxLRpoints;

        for (int i = 0; i < maxLRpoints; i++)
        {
            reticleLR.SetPosition(i, new Vector3(v.x, 0, v.y));
            v = MyMath.Rotate(v, ang);
        }
    }

    public void DrawArc(Vector3 dir, float angle, float radius, float innerRadius)
    {
        transform.forward = dir;

        if (radius == lastOuterRadius && innerRadius == lastInnerRadius && angle == lastAngle) 
            return;
        lastOuterRadius = radius;
        lastInnerRadius = innerRadius;
        lastAngle = angle;

        Vector2 v = MyMath.Rotate(Vector2.up * radius, -angle / 2);
        float ang = 360f / maxLRpoints;

        int p = Mathf.RoundToInt(angle / ang + 0.5f);
        reticleLR.positionCount = p * 2;

        for (int i = 0; i < p; i++)
        {
            reticleLR.SetPosition(i, new Vector3(v.x, 0, v.y));
            v = MyMath.Rotate(v, ang);
        }

        v = MyMath.Rotate(v.normalized * innerRadius, -ang);

        for (int i = p; i < p * 2; i++)
        {
            reticleLR.SetPosition(i, new Vector3(v.x, 0, v.y));
            v = MyMath.Rotate(v, -ang);
        }

    }
}
