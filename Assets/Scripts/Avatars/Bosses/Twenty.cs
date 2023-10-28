 using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    [Header("Arc Attack")]
    [SerializeField] private BossAttackStats arcStats;
    [SerializeField] private RangeI arcCount;
    [SerializeField] protected float arcDuration;
    [SerializeField] protected float startArcDelay;
    [SerializeField] protected float arcDelay;
    [SerializeField] protected float maxArcOffset;
    [SerializeField] protected float arcSpawnOffset;
    [SerializeField] protected float arcSawDelay;
    [SerializeField] [Range(0, 100)] protected int haiwChance;

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
        for (int i = 0; i < arcStats.bucketCount; i++)
            attackBucket.Add(BossAttack.Attack3);
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
        StartCoroutine(ArcAttack());
    }
    private IEnumerator ArcAttack()
    {
        attacking = true;

        targetPos = MapController.Instance.mapCenter;
        yield return new WaitUntil(() => Vector3.Distance(transform.position, targetPos) < targetPosAccuracy);

        canMove = false;

        yield return new WaitForSeconds(startArcDelay);

        int count = arcCount.RandomVal;
        float remaining = count * arcDelay + arcDuration;
        for (int i = 0; i < count; i++)
        {
            float offset = Random.Range(-maxArcOffset, maxArcOffset);
            if (i % 2 == 0)
            {
                float dir = Mathf.Sign(Random.Range(-1, 1));
                StartCoroutine(ArcController(
                    MapController.Instance.mapCenter + Vector3.forward * offset + Vector3.right * arcSpawnOffset * -dir,
                    Vector3.right * dir,
                    remaining
                ));
            }
            else
            {
                StartCoroutine(ArcController(
                    MapController.Instance.mapCenter + Vector3.right * offset - Vector3.forward * arcSpawnOffset,
                    Vector3.forward,
                    remaining
                ));
            }

            if (Random.Range(0, 100) < haiwChance)
                SpawnHaiw();

            yield return new WaitForSeconds(arcDelay);
            remaining -= arcDelay;
        }

        yield return new WaitForSeconds(remaining);

        canMove = true;
        attacking = false;
        nextAttack = Time.time + Random.Range(minTimeBetweenAttacks, maxTimeBetweenAttacks);
    }
    private IEnumerator ArcController(Vector3 origin, Vector3 direction, float duration)
    {
        float end = Time.time + duration;

        while (Time.time < end)
        {
            SpawnSaw(arcStats.hazardPrefab, origin, direction, arcStats.hazardSpeedMod);
            yield return new WaitForSeconds(arcSawDelay);
        }
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
