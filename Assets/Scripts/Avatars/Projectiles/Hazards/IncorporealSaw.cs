using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncorporealSaw : BasicSaw
{
    [SerializeField] private bool destroyWhenOOB = true;

    protected override void Update()
    {
        if (maxRange > -1 && destroyWhenOOB && Vector3.Distance(transform.position, origin) > maxRange)
        {
            DestroyObject();
            return;
        }

        base.Update();
    }

    protected override void OnHitWall(Collider other)
    {
    }
}
