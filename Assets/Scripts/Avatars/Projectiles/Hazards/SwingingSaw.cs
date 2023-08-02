using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class SwingingSaw : IncorporealSaw
{
    [SerializeField] protected float minRange;
    [SerializeField] protected float maxRange;
    protected Vector3 endPos;

    [Header("Smoothing")]
    [SerializeField] protected float smoothingRange;
    [SerializeField] protected float minSpeedMod;
    protected bool smoothing = false;
    protected float prevSpeed;

    protected override void Awake()
    {
        base.Awake();

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
            SetSpeed(prevSpeed * mod);
            smoothing = false;
        }
        else
        {
            SetSpeed(prevSpeed);
        }
        
        SetDirection(endPos - transform.position);
    }

    private Vector3 GetNewEndPos()
    {
        Vector2 dir = Random.insideUnitCircle.normalized;
        Vector3 pos = transform.position + new Vector3(dir.x, 0, dir.y) * Random.Range(minRange, maxRange);

        if (Vector3.Distance(MapController.Instance.mapCenter, pos) < MapController.Instance.hazardRange)
            return pos;

        return (MapController.Instance.mapCenter - transform.position).normalized * Random.Range(minRange, maxRange);
    }
}
