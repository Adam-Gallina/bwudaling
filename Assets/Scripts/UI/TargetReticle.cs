using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetReticle : MonoBehaviour
{
    [SerializeField] protected Transform reticle;
    [SerializeField] protected LineRenderer reticleLR;
    [SerializeField] protected int maxLRpoints = 180;

    private float lastRadius;

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
        if (radius == lastRadius)
            return;
        lastRadius = radius;

        Vector2 v = Vector2.up * radius;
        float ang = 360f / maxLRpoints;

        for (int i = 0; i < maxLRpoints; i++)
        {
            reticleLR.SetPosition(i, new Vector3(v.x, 0, v.y));
            v = MyMath.Rotate(v, ang);
        }
    }
}
