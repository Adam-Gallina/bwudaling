using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class SwingingSaw : IncorporealSaw
{
    [SerializeField] protected RangeF travelRange;
    protected Vector3 endPos;

    [Header("Smoothing")]
    [SerializeField] protected float smoothingRange;
    [SerializeField] protected float minSpeedMod;
    protected bool smoothing = false;
    protected float prevSpeed;

    public override void SetSpawnLocation(Vector3 spawnLocation)
    {
        base.SetSpawnLocation(spawnLocation);
        endPos = GetNewEndPos();
    }

    public override void SetSpeed(float speed)
    {
        base.SetSpeed(speed);

        if (smoothing)
            return;

        prevSpeed = speed;
    }

    private new void Update()
    {
        base.Update();

        float t = (transform.position.x - spawnPos.x) / (endPos.x - spawnPos.x);

        if (t >= 1)
        {
            spawnPos = endPos;
            endPos = GetNewEndPos();
            t = 0;
        }

        float t2 = 2 * Mathf.Abs(t - 0.5f); //0>1, -.5>.5, 0>.5
        
        if (t2 > smoothingRange)
        {
            smoothing = true;
            float t3 = (1 - t2) / (1 - smoothingRange);
            float mod = minSpeedMod + (1 - minSpeedMod) * t3;
            SetSpeed(prevSpeed * mod * currSpeedMod);
            smoothing = false;
        }
        else
        {
            SetSpeed(prevSpeed * currSpeedMod);
        }
        
        SetDirection(endPos - transform.position);
    }

    private Vector3 GetNewEndPos()
    {
        Vector2 dir = Random.insideUnitCircle.normalized * travelRange.RandomVal;

        int i = 0;
        while (i++ < 4)
        {
            Vector3 pos = transform.position + new Vector3(dir.x, 0, dir.y);
            if (maxRange > -1 && Vector3.Distance(origin, pos) < maxRange)
                return pos;

            dir = MyMath.Rotate(dir, 90);
        }

        return transform.position + (origin - transform.position).normalized * travelRange.RandomVal;
    }
}
