using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RainbowString : BossBase
{
    [Header("Movement Anim")]
    [SerializeField] private float bezierControlSize = 15;
    [SerializeField] private int bezierSamples = 20;
    float lastBezierT;
    [SerializeField] private float segmentFollowDist;
    [SerializeField] private Transform[] segments;
    private List<Bezier> moveCurves = new List<Bezier>();

    [Header("Yarnball Attack")]
    [SerializeField] protected BossAttackStats yarnballStats;
    [SerializeField] private RangeI yarnballBursts;
    [SerializeField] private int yarnballBurstSpawns = 3;
    [SerializeField] private float yarnballSpawnMargin = .15f;
    [SerializeField] private float yarnballBurstDelay = 1.25f;
    [Range(0, 100)]
    [SerializeField] protected int yarnballHaiwSpawnChance = 30;

    [Header("Fray Attack")]
    [SerializeField] protected BossAttackStats frayStats;

    [Header("Tangle Attack")]
    [SerializeField] protected BossAttackStats tangleStats;
    [SerializeField] private float tanglePullDuration = 3;
    [SerializeField] private float tanglePullSpeedMod = .8f;
    [SerializeField] private float tangleExplodeSpeedMod = 1.35f;
    [SerializeField] private float tangleExplodeModDuration = 10;
    [SerializeField] private int tangleSpawnCount = 12;
    [SerializeField] private RangeI tangleHaiwSpawns;

    private void OnDrawGizmos()
    {
        if (moveCurves.Count == 0)
            return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, minMoveDist);
        Gizmos.DrawWireSphere(transform.position, maxMoveDist);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(moveCurves[0].start + Vector3.up * 6, moveCurves[0].start + moveCurves[0].startConstraint + Vector3.up * 6);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(moveCurves[0].end + Vector3.up * 6, moveCurves[0].end + moveCurves[0].endConstraint + Vector3.up * 6);

        Gizmos.color = Color.yellow;
        Vector3 lastPos = moveCurves[0].start;
        for (int i = 1; i < bezierSamples; i++)
        {
            Vector3 pos = moveCurves[0].Sample((float)i / bezierSamples);
            Gizmos.DrawLine(lastPos + Vector3.up * 5, pos + Vector3.up * 5);
            lastPos = pos;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        foreach (Transform s in segments)
        {
            s.parent = transform.parent;
            s.position = transform.position;
        }
    }

    protected override void FillAttackBucket()
    {
        for (int i = 0; i < yarnballStats.bucketCount; i++)
            attackBucket.Add(BossAttack.Attack1);
        for (int i = 0; i < frayStats.bucketCount; i++)
            attackBucket.Add(BossAttack.Attack2);
        for (int i = 0; i < tangleStats.bucketCount; i++)
            attackBucket.Add(BossAttack.Attack3);
    }

    protected override void DoBossMovement()
    {
        Vector3 lastPos = transform.position;
        float dt = moveSpeed * Time.deltaTime / moveCurves[0].length;
        lastBezierT += dt;
        if (lastBezierT >= 1)
            lastBezierT = 1;
        transform.position = moveCurves[0].Sample(lastBezierT);
        
        if (lastPos != transform.position)
            transform.forward = transform.position - lastPos;

        // Update animation
        segments[0].position = transform.position;
        float animDt = segmentFollowDist / moveCurves[0].length;
        float animT = lastBezierT;
        int currCurve = 0;
        int i;
        for (i = 1; i < segments.Length; i++)
        {
            animT -= animDt;
            if (animT < 0)
            {
                currCurve++;
                if (currCurve >= moveCurves.Count)
                    break;

                float remainingDist = segmentFollowDist - (animT + animDt) * moveCurves[currCurve - 1].length;
                animT = 1 - (remainingDist / moveCurves[currCurve].length);
                animDt = segmentFollowDist / moveCurves[currCurve].length;
            }

            segments[i].position = moveCurves[currCurve].Sample(animT);
        }

        // Clean up any unused curves
        if (currCurve < moveCurves.Count - 1)
            moveCurves.RemoveAt(moveCurves.Count - 1);
    }

    protected override Vector3 GetBossMoveTarget(float distance)
    {
        if (moveCurves.Count == 0 || Vector3.Distance(transform.position, moveCurves[0].end) < targetPosAccuracy || lastBezierT == 1)
            GetNewMovementCurve(base.GetBossMoveTarget(distance));

        return moveCurves[0].Sample(1);
    }

    private void GetNewMovementCurve(Vector3 target)
    {
        Vector3 s = transform.position;
        Vector3 sc = transform.forward * bezierControlSize;
        Vector3 e = target;
        Vector3 ec = MyMath.RotateAboutY((s - e).normalized, Random.Range(-45, 45)) * bezierControlSize;
        moveCurves.Insert(0, new Bezier(s, sc, e, ec));

        lastBezierT = 0;

        moveCurves[0].ApproximateLength(bezierSamples);
    }

    #region Attacks
    protected override void DoAttack1()
    {
        StartCoroutine(YarnballAttack());
    }
    private IEnumerator YarnballAttack()
    {
        attacking = true;

        for (int i = yarnballBursts.RandomVal; i >= 0; i--)
        {
            for (int j = 0; j < yarnballBurstSpawns; j++)
            {
                Transform target = GetRandomPlayer(false);
                SpawnSaw(yarnballStats.hazardPrefab, transform.position, target.position - transform.position, yarnballStats.hazardSpeedMod);

                if (Random.Range(0, 100) < yarnballHaiwSpawnChance)
                    SpawnHaiw();

                yield return new WaitForSeconds(yarnballSpawnMargin);
            }

            yield return new WaitForSeconds(yarnballBurstDelay);
        }

        attacking = false;
        nextAttack = Time.time + Random.Range(minTimeBetweenAttacks, maxTimeBetweenAttacks);
    }

    protected override void DoAttack2()
    {
        StartCoroutine(FrayAttack());
    }
    protected IEnumerator FrayAttack()
    {
        yield return null;
    }

    protected override void DoAttack3()
    {
        StartCoroutine(TangleAttack());
    }

    private IEnumerator TangleAttack()
    {
        attacking = true;
        canMove = false;

        GetNewMovementCurve(MapController.Instance.mapCenter);
        if (moveCurves.Count > 1)
        {
            moveCurves[1].MoveEnd(transform.position, transform.forward * bezierControlSize);
            moveCurves[1].ApproximateLength(bezierSamples);
        }

        while (lastBezierT < 1)
        {
            DoBossMovement();
            yield return new WaitForEndOfFrame();
        }

        BasicSaw[] saws = MapController.Instance.GetComponentsInChildren<BasicSaw>();
        foreach (BasicSaw saw in saws)
        {
            if (saw == null) continue;

            saw.SetDirection(transform.position - saw.transform.position);
            saw.ApplySpeedMod(tanglePullSpeedMod, tanglePullDuration);
        }

        float start = Time.time;
        while (Time.time < start + tanglePullDuration)
        {
            // Circle animation

            yield return new WaitForEndOfFrame();
        }

        foreach (BasicSaw saw in saws)
        {
            if (saw == null) continue;

            saw.SetDirection(saw.transform.position - transform.position);
            saw.ApplySpeedMod(tangleExplodeSpeedMod, tangleExplodeModDuration, true);
        }

        // Spawn a bunch of saws in a circle
        Vector3 dir = transform.forward;
        float da = 360 / tangleSpawnCount;
        for (int i = 0; i < tangleSpawnCount; i++)
        {
            SpawnSaw(tangleStats.hazardPrefab, transform.position, dir, tangleStats.hazardSpeedMod);
            dir = MyMath.RotateAboutY(dir, da);
        }

        for (int i = tangleHaiwSpawns.RandomVal; i >= 0; i--)
            SpawnHaiw();
        
        targetPos = GetBossMoveTarget(Random.Range(minMoveDist, maxMoveDist));

        attacking = false;
        canMove = true;
        nextAttack = Time.time + Random.Range(minTimeBetweenAttacks, maxTimeBetweenAttacks);
    }
    #endregion


    [Server]
    public override IEnumerator SpawnAnim()
    {
        RpcStartAnim();

        yield return base.SpawnAnim();
    }

    [ClientRpc]
    private void RpcStartAnim() { StartCoroutine(Anim()); }
    private IEnumerator Anim()
    {
        yield return null;
    }

    [Server]
    public override IEnumerator DeathAnim()
    {
        yield return new WaitForSeconds(1);

        canMove = false;

        //RpcPlayDeathAudio();
        //RpcSetAnimTrigger("Killed");

        yield return base.DeathAnim();
    }
}
