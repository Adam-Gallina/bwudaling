using Mirror;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] protected float[] yarnballSpawnDelays;
    [SerializeField] protected int yarnballHaiwSpawnCount;

    [Header("Tangle Attack")]
    [SerializeField] protected BossAttackStats frayStats;

    [Header("Unknown Attack")]
    [SerializeField] protected BossAttackStats tangleStats;

    /*private void OnDrawGizmos()
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
    }*/

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
        // If no curve exists yet, step any remaining points towards front
        /*for (; i < segments.Length; i++)
        {
            if (Vector3.Distance(segments[i - 1].position, segments[i].position) > segmentFollowDist)
                segments[i].position = segments[i - 1].position + (segments[i].position - segments[i - 1].position).normalized * segmentFollowDist;
        }*/

        // Clean up any unused curves
        if (currCurve < moveCurves.Count - 1)
            moveCurves.RemoveAt(moveCurves.Count - 1);
    }


    protected override Vector3 GetBossMoveTarget(float distance)
    {
        if (moveCurves.Count == 0 || Vector3.Distance(transform.position, moveCurves[0].end) < targetPosAccuracy || lastBezierT == 1)
        {
            Vector3 s = transform.position;
            Vector3 sc = transform.forward * bezierControlSize;
            Vector3 e = base.GetBossMoveTarget(distance);
            Vector3 ec = MyMath.RotateAboutY((s - e).normalized, Random.Range(-45, 45)) * bezierControlSize;
            moveCurves.Insert(0, new Bezier(s, sc, e, ec));

            lastBezierT = 0;

            moveCurves[0].ApproximateLength(bezierSamples);
        }

        return moveCurves[0].Sample(1);
    }

    #region Attacks
    protected override void DoAttack1()
    {
        StartCoroutine(YarnballAttack());
    }
    private IEnumerator YarnballAttack()
    {
        yield return null;
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
        yield return null;
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
