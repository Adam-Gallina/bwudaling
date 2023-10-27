 using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Twenty : BossBase
{
    [Header("Wave Attack")]
    [SerializeField] protected BossAttackStats waveStats;
    [SerializeField] protected int sawWaveCount;
    [SerializeField] protected float sawSpreadAng;
    [SerializeField] protected float turnSpeed;
    [SerializeField] protected float aimTime;
    [SerializeField] protected float sawLaunchDelay;
    [SerializeField] protected RangeI haiwSpawns;


    [Header("Meteor Shower")]
    [SerializeField] protected BossAttackStats meteorStats;
    [SerializeField] protected RangeI meteorCount;
    [SerializeField] protected int chanceToTargetPlayer;
    [SerializeField] protected float maxDistFromPlayer;
    [SerializeField] protected int chanceForRandomDrop;
    [SerializeField] protected RangeF meteorRange;
    [SerializeField] protected RangeF timeBetweenMeteors;


    protected override void CheckBossMovement()
    {
        anim.SetBool("Walking", canMove);

        if (canMove)
        {
            base.CheckBossMovement();
        }
    }

    protected override void FillAttackBucket()
    {
        for (int i = 0; i < waveStats.bucketCount; i++)
            attackBucket.Add(BossAttack.Attack1);
        for (int i = 0; i < meteorStats.bucketCount; i++)
            attackBucket.Add(BossAttack.Attack2);
    }

    protected override void DoAttack1()
    {
        StartCoroutine(WaveAttack());
    }

    protected IEnumerator WaveAttack()
    {
        attacking = true;
        canMove = false;

        Transform target = GetClosestValidPlayer();

        transform.forward = target.position - transform.position;

        float end = Time.time + aimTime;
        while (Time.time < end)
        {
            transform.forward = Vector3.RotateTowards(transform.forward, (target.position - transform.position).normalized, turnSpeed * Time.deltaTime * Mathf.Deg2Rad, 0);
            yield return new WaitForEndOfFrame();
        }

        RpcSetAnimTrigger("Attack");
        yield return new WaitForSeconds(1);

        float da = sawSpreadAng / (sawWaveCount * 2);
        Vector3 dir = transform.forward;
        Vector2 nextDir1 = MyMath.Rotate(new Vector2(dir.x, dir.z), -sawSpreadAng / 2);
        Vector2 nextDir2 = MyMath.Rotate(new Vector2(dir.x, dir.z), sawSpreadAng / 2);

        for (int _ = 0; _ < haiwSpawns.RandomVal; _++)
        {
            SpawnHaiw();
        }

        for (int _ = 0; _ < sawWaveCount; _++)
        {
            SpawnSaw(waveStats.hazardPrefab, transform.position, new Vector3(nextDir1.x, 0, nextDir1.y), waveStats.hazardSpeedMod);
            SpawnSaw(waveStats.hazardPrefab, transform.position, new Vector3(nextDir2.x, 0, nextDir2.y), waveStats.hazardSpeedMod);

            nextDir1 = MyMath.Rotate(nextDir1, da);
            nextDir2 = MyMath.Rotate(nextDir2, -da);

            yield return new WaitForSeconds(sawLaunchDelay);
        }


        attacking = false;
        canMove = true;
        nextAttack = Time.time + Random.Range(minTimeBetweenAttacks, maxTimeBetweenAttacks);
    }

    protected override void DoAttack2()
    {
        StartCoroutine(MeteorAttack());
    }

    protected IEnumerator MeteorAttack()
    {
        attacking = true;
        canMove = false;

        RpcSetAnimTrigger("Attack2");
        yield return new WaitForSeconds(1);

        for (int _ = meteorCount.RandomVal; _ > 0; _--)
        {
            Vector3 spawnPos;

            int n = Random.Range(0, chanceToTargetPlayer + chanceForRandomDrop);
            if (n < chanceToTargetPlayer)
            {
                Vector2 dir = Random.insideUnitCircle * Random.Range(0, maxDistFromPlayer);
                spawnPos = GetRandomValidPlayer().position + new Vector3(dir.x, 0, dir.y);
            }
            else
            {
                Vector2 c = Random.insideUnitCircle;
                spawnPos = transform.position + new Vector3(c.x, 0, c.y) * meteorRange.RandomVal;
            }

            SpawnSaw(meteorStats.hazardPrefab, spawnPos, Vector3.zero, meteorStats.hazardSpeedMod);

            yield return new WaitForSeconds(timeBetweenMeteors.RandomVal);
        }

        attacking = false;
        canMove = true;
        nextAttack = Time.time + Random.Range(minTimeBetweenAttacks, maxTimeBetweenAttacks);
    }

    protected override void DoAttack3()
    {
        throw new System.NotImplementedException();
    }


    [Server]
    public override IEnumerator SpawnAnim()
    {
        yield return new WaitForSeconds(1);

        RpcSetAnimTrigger("Spawn");

        yield return new WaitForSeconds(.5f);

        yield return base.SpawnAnim();
    }

    [Server]
    public override IEnumerator DeathAnim()
    {
        yield return new WaitForSeconds(1);

        canMove = false;

        RpcSetAnimTrigger("Killed");

        yield return base.DeathAnim();
    }
}
