using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainbowString : BossBase
{
    [Header("Movement Anim")]
    [SerializeField] private RangeF moveAngle;
    private float currMoveRadius;
    private Vector3 currMoveCenter;
    private float currAngDir;
    [SerializeField] private float sawFollowDist;
    [SerializeField] private Transform[] segments;

    [Header("Yarnball Attack")]
    [SerializeField] protected BossAttackStats yarnballStats;
    [SerializeField] protected float[] yarnballSpawnDelays;
    [SerializeField] protected int yarnballHaiwSpawnCount;

    [Header("Tangle Attack")]
    [SerializeField] protected BossAttackStats frayStats;

    [Header("Unknown Attack")]
    [SerializeField] protected BossAttackStats tangleStats;

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
        float maxRadianDelta = moveSpeed * Time.deltaTime / currMoveRadius;
        transform.position = Vector3.RotateTowards(transform.position - currMoveCenter, targetPos, maxRadianDelta, 0); ;

        //transform.forward = Vector3.RotateTowards(transform.forward, (targetPos - transform.position).normalized, rotationSpeed * Mathf.Deg2Rad * Time.deltaTime, 0);


        // Update segment positions
    }

    protected override Vector3 GetBossMoveTarget(float distance)
    {
        currMoveRadius = distance;
        currMoveCenter = base.GetBossMoveTarget(distance);
        
        currAngDir = (Random.Range(0, 1) == 0 ? 1 : -1);
        float ang = moveAngle.RandomVal * currAngDir;

        Vector3 toPos = (targetPos - currMoveCenter).normalized;
        Vector3 toTarget = MyMath.RotateAboutY(toPos, ang);

        return currMoveCenter + toTarget;
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

        RpcPlayDeathAudio();
        RpcSetAnimTrigger("Killed");

        yield return base.DeathAnim();
    }
}
