using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncorporealSaw : BasicSaw
{
    protected override void Update()
    {
        if (Vector3.Distance(transform.position, MapController.Instance.mapCenter) > MapController.Instance.hazardRange)
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
