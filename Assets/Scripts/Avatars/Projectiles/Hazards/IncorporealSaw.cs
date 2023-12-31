using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncorporealSaw : BasicSaw
{
    [SerializeField] private bool destroyWhenOOB = true;

    protected override void Update()
    {
        base.Update();

        if (!isServer)
            return;

        if (maxRange > -1 && destroyWhenOOB && Vector3.Distance(transform.position, origin) > maxRange)
        {
            DestroyObject(false);
            return;
        }
    }

    protected override void OnHitWall(Collider other)
    {
    }
}
