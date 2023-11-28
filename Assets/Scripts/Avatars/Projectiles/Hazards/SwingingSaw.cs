using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class SwingingSaw : IncorporealSaw
{
    [SerializeField] protected RangeF travelRange;
    protected Vector3 endPos;
    protected Vector3 dirToEnd;

    [Header("Smoothing")]
    [SerializeField] protected float smoothingRange;
    [SerializeField] protected float minSpeedMod;
    protected bool smoothing = false;
    protected float prevSpeed;

    public override void SetSpawnLocation(Vector3 spawnLocation)
    {
        base.SetSpawnLocation(spawnLocation);
        endPos = GetNewEndPos();
        dirToEnd = (endPos - transform.position).normalized;
    }

    [Server]
    public override void SetDirection(Vector3 direction)
    {
        // Allow other sources to change direction of saw
        if (direction != transform.forward)
        {
            float dist = travelRange.RandomVal;
            spawnPos = transform.position - direction.normalized * dist / 2;
            endPos = transform.position + direction.normalized * dist / 2;
            dirToEnd = (endPos - transform.position).normalized;
        }

        direction = endPos - transform.position;

        base.SetDirection(direction);
    }

    private new void Update()
    {
        base.Update();

        if (!isServer)
            return;

        float t = (transform.position.x - spawnPos.x) / (endPos.x - spawnPos.x);

        if (t >= 1)
        {
            spawnPos = transform.position;
            endPos = GetNewEndPos();
            dirToEnd = (endPos - transform.position).normalized;
            t = 0;
        }

        float t2 = 2 * Mathf.Abs(t - 0.5f); //0>1, -.5>.5, 0>|.5|, 0>|1|

        float mod = 1;
        if (t2 > smoothingRange)
        {
            float t3 = (1 - t2) / (1 - smoothingRange);
            mod = minSpeedMod + (1 - minSpeedMod) * t3;
        }

        rb.velocity = dirToEnd * speed * mod * currSpeedMod;
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

        return transform.position + MyMath.RotateAboutY((origin - transform.position).normalized, Random.Range(-30, 30)) * travelRange.RandomVal;
    }
}
