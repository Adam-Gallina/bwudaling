using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SiwyCwab : BossBase
{
    [Header("Charge Attack")]
    [SerializeField] protected BossAttack chargeStats;
    [SerializeField] protected float distBetweenChargeSpawns = 7;
    [SerializeField] protected float chargeSawSpawnOffset = 1;
    [SerializeField] protected float chargeSpeed;
    [SerializeField] protected float minChargeDist;
    [SerializeField] protected float maxChargeDist;

    [Header("Stomp Attack")]
    [SerializeField] protected BossAttack stompStats;
    [SerializeField] protected float timeBetweenStomps;
    [SerializeField] protected int stompWaves;
    [SerializeField] protected int sawsPerWave;
    [SerializeField] protected float stompWaveAngle;
    [SerializeField] protected Transform[] stompSpawnPoints;
    [SerializeField] protected int minHaiwSpawns;
    [SerializeField] protected int maxHaiwSpawns;

    protected override void CheckBossMovement()
    {
        anim.SetBool("Walking", canMove);

        if (canMove)
        {
            base.CheckBossMovement();
        }
    }

    protected override void CheckBossAttacks()
    {
        if (Time.time > nextAttack && !attacking)
        {
            int n = Random.Range(0, chargeStats.chance + stompStats.chance);

            if (n < chargeStats.chance)
                StartCoroutine(ChargeAttack());
            else if (n < stompStats.chance + chargeStats.chance)
                StartCoroutine(StompAttack());
        }
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

