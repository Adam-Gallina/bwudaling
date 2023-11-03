using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Net.Sockets;
using static UnityEngine.GraphicsBuffer;

public class SiwyCwab : BossBase
{
    [Header("Charge Attack")]
    [SerializeField] protected BossAttackStats chargeStats;
    [SerializeField] protected float distBetweenChargeSpawns = 7;
    [SerializeField] protected float chargeSawSpawnOffset = 1;
    [SerializeField] protected float chargeSpeed;
    [SerializeField] protected float minChargeDist;
    [SerializeField] protected float maxChargeDist;

    [Header("Stomp Attack")]
    [SerializeField] protected BossAttackStats stompStats;
    [SerializeField] protected float timeBetweenStomps;
    [SerializeField] protected int stompWaves;
    [SerializeField] protected int sawsPerWave;
    [SerializeField] protected float stompWaveAngle;
    [SerializeField] protected Transform[] stompSpawnPoints;
    [SerializeField] protected int minHaiwSpawns;
    [SerializeField] protected int maxHaiwSpawns;

    [Header("Leap Attack")]
    [SerializeField] protected BossAttackStats leapStats;
    [SerializeField] protected RangeI leapCount;
    [SerializeField] protected float leapDelay;
    [SerializeField] protected float leapTurnSpeed;
    [SerializeField] protected float leapTime;
    [SerializeField] protected RangeF leapDist;
    [SerializeField] protected RangeI leapSaws;
    [SerializeField][Range(0,100)] protected int leapHaiwChance;

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
        for (int i = 0; i < chargeStats.bucketCount; i++)
            attackBucket.Add(BossAttack.Attack1);
        for (int i = 0; i < stompStats.bucketCount; i++)
            attackBucket.Add(BossAttack.Attack2);
        for (int i = 0; i < leapStats.bucketCount; i++)
            attackBucket.Add(BossAttack.Attack3);
    }

    protected override void DoAttack1()
    {
        StartCoroutine(ChargeAttack());
    }
    private IEnumerator ChargeAttack()
    {
        attacking = true;
        canMove = false;

        Vector3 tPos = GetBossMoveTarget(Random.Range(minChargeDist, maxChargeDist));
        transform.forward = tPos - transform.position;

        Vector3 lastSpawnPos = (tPos - transform.position).normalized * -distBetweenChargeSpawns;

        yield return new WaitForSeconds(.5f);
        anim.SetBool("Running", true);
        yield return new WaitForSeconds(.5f);


        while (Vector3.Distance(transform.position, tPos) > targetPosAccuracy)
        {
            transform.position = Vector3.MoveTowards(transform.position, tPos, chargeSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, lastSpawnPos) > distBetweenChargeSpawns)
            {
                lastSpawnPos = transform.position;

                SpawnHaiw(-transform.forward);
                SpawnSaw(chargeStats.hazardPrefab, transform.position + transform.right * chargeSawSpawnOffset, -transform.right, chargeStats.hazardSpeedMod);
                SpawnSaw(chargeStats.hazardPrefab, transform.position - transform.right * chargeSawSpawnOffset, transform.right, chargeStats.hazardSpeedMod);
            }

            yield return new WaitForEndOfFrame();
        }

        anim.SetBool("Running", false);

        yield return new WaitForSeconds(1);

        attacking = false;
        canMove = true;
        nextAttack = Time.time + Random.Range(minTimeBetweenAttacks, maxTimeBetweenAttacks);
    }

    protected override void DoAttack2()
    {
        StartCoroutine(StompAttack());
    }
    protected IEnumerator StompAttack()
    {
        attacking = true;
        canMove = false;

        yield return new WaitForSeconds(1);

        for (int i = 0; i < stompWaves; i++)
        {
            float da = stompWaveAngle / sawsPerWave;
            Vector3 dir = stompSpawnPoints[i % stompSpawnPoints.Length].forward;
            Vector2 nextDir = MyMath.Rotate(new Vector2(dir.x, dir.z), -stompWaveAngle / 2);

            for (int s = 0; s < sawsPerWave; s++)
            {
                SpawnSaw(stompStats.hazardPrefab, stompSpawnPoints[i % stompSpawnPoints.Length].position, new Vector3(nextDir.x, 0, nextDir.y), stompStats.hazardSpeedMod);

                nextDir = MyMath.Rotate(nextDir, da);
            }

            for (int _ = 0; _ < Random.Range(minHaiwSpawns, maxHaiwSpawns); _++)
                SpawnHaiw();

            RpcSetAnimTrigger("Attack");

            yield return new WaitForSeconds(timeBetweenStomps);
        }

        attacking = false;
        canMove = true;
        nextAttack = Time.time + Random.Range(minTimeBetweenAttacks, maxTimeBetweenAttacks);
    }

    protected override void DoAttack3()
    {
        StartCoroutine(LeapAttack());
    }
    protected IEnumerator LeapAttack()
    {
        attacking = true;
        canMove = false;

        anim.SetBool("Jump", true);
        for (int _ = leapCount.RandomVal; _ > 0; _--)
        {
            Vector3 target = GetBossMoveTarget(leapDist.RandomVal);
            if (target == transform.position)
            {
                yield return new WaitForSeconds(leapDelay);
            }
            else 
            {
                float sa = transform.eulerAngles.y;
                float ang = Vector3.SignedAngle(transform.forward, target - transform.position, Vector3.up);

                float turnEnd = Time.time + leapDelay;
                while (Time.time < turnEnd)
                {
                    float t = 1 - ((turnEnd - Time.time) / leapDelay);
                    transform.eulerAngles = new Vector3(0, sa + ang * t, 0);
                    yield return new WaitForEndOfFrame();
                }
            }

            anim.SetBool("JumpUp", true);
            yield return new WaitForSeconds(.35f);
            GetComponent<BoxCollider>().enabled = false;

            Vector3 start = transform.position;
            float end = Time.time + leapTime;
            while (Time.time < end)
            {
                float t = 1 - ((end - Time.time) / leapTime);
                transform.position = start + (target - start) * t;

                if (end - Time.time < 0.5f && anim.GetBool("JumpUp"))
                    anim.SetBool("JumpUp", false);

                yield return new WaitForEndOfFrame();
            }

            GetComponent<BoxCollider>().enabled = true;

            if (Random.Range(0, 100) < leapHaiwChance)
                SpawnHaiw();

            Vector3 dir = transform.forward;
            int spawnCount = leapSaws.RandomVal;
            float da = 360 / spawnCount;
            for (int i = 0; i < spawnCount; i++)
            {
                SpawnSaw(leapStats.hazardPrefab, transform.position, dir, leapStats.hazardSpeedMod);
                dir = MyMath.RotateAboutY(dir, da);
            }
        }

        anim.SetBool("Jump", false);
        attacking = false;
        canMove = true;
        nextAttack = Time.time + Random.Range(minTimeBetweenAttacks, maxTimeBetweenAttacks);
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

