using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigWed : BossBase
{
    [Header("Spicy Attack")]
    [SerializeField] protected BossAttack spicyStats;
    [SerializeField] private float[] spawnDelays;
    [SerializeField] private float sawSpreadAng;
    [SerializeField] protected float turnSpeed;
    [SerializeField] private float aimTime;
    [SerializeField] private int spicyHaiwSpawnCount;

    [Header("Whirl Attack")]
    [SerializeField] protected BossAttack whirlStats;
    [SerializeField] protected RangeF whirlSpeed;
    [SerializeField] protected float chargeDuration;
    [SerializeField] protected RangeF whirlDuration;
    [SerializeField] protected RangeF timeBetweenSpawns;
    [SerializeField] protected int spawnCount = 3;
    [SerializeField] private int whirlHaiwSpawnCount;

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
            int n = Random.Range(0, spicyStats.chance + whirlStats.chance);

            if (n < spicyStats.chance)
                StartCoroutine(SpicyAttack());
            else if (n < whirlStats.chance + spicyStats.chance)
                StartCoroutine(WhirlAttack());
        }
    }

    private IEnumerator SpicyAttack()
    {
        attacking = true;
        canMove = false;

        Transform target = BwudalingNetworkManager.Instance.Players[0].avatar.transform;
        float dist = Mathf.Infinity;
        foreach (NetworkPlayer p in BwudalingNetworkManager.Instance.Players)
        {
            if (p.avatar.dead)
                continue;

            float d = Vector3.Distance(transform.position, p.avatar.transform.position);
            if (d < dist)
            {
                target = p.avatar.transform;
                dist = d;
            }
        }

        RpcSetAnimTrigger("Attack");

        transform.forward = target.position - transform.position;

        float end = Time.time + aimTime;
        while (Time.time < end)
        {
            transform.forward = Vector3.RotateTowards(transform.forward, (target.position - transform.position).normalized, turnSpeed * Time.deltaTime * Mathf.Deg2Rad, 0);
            yield return new WaitForEndOfFrame();
        }

        int haiwDelta = spawnDelays.Length / spicyHaiwSpawnCount;
        for (int i = 0; i < spawnDelays.Length; i++)
        {
            yield return new WaitForSeconds(spawnDelays[i]);
            Vector3 dir = MyMath.RotateAboutY(transform.forward, Random.Range(-sawSpreadAng, sawSpreadAng));
            SpawnSaw(spicyStats.hazardPrefab, transform.position, dir, spicyStats.hazardSpeedMod);

            if (i % haiwDelta == 0)
                SpawnHaiw();
        }

        attacking = false;
        canMove = true;
        nextAttack = Time.time + Random.Range(minTimeBetweenAttacks, maxTimeBetweenAttacks);
    }

    protected IEnumerator WhirlAttack()
    {
        attacking = true;
        canMove = false;

        yield return new WaitForSeconds(0.5f);

        anim.SetBool("Attack2", true);

        float startTime = Time.time;
        
        while (Time.time - startTime < chargeDuration)
        {
            float t = (Time.time - startTime) / chargeDuration;
            
            Vector3 ang = transform.localEulerAngles;
            ang.y += whirlSpeed.PercentVal(t) * Time.deltaTime;
            transform.localEulerAngles = ang;

            yield return new WaitForEndOfFrame();
        }

        float endTime = Time.time + whirlDuration.RandomVal;
        float nextSpawn = Time.time + timeBetweenSpawns.RandomVal;
        float da = 360f / spawnCount;

        float haiwDelta = (endTime - Time.time) / whirlHaiwSpawnCount;
        float nextHair = Time.time + haiwDelta;

        while (Time.time < endTime)
        {
            if (Time.time >= nextSpawn)
            {
                nextSpawn = Time.time + timeBetweenSpawns.RandomVal;

                Vector3 dir = transform.forward;
                for (int i = 0; i < spawnCount; i++)
                {
                    SpawnSaw(whirlStats.hazardPrefab, transform.position, dir, whirlStats.hazardSpeedMod);
                    dir = MyMath.RotateAboutY(dir, da);
                }
            }

            if (Time.time > nextHair)
            {
                nextHair = Time.time + haiwDelta;
                SpawnHaiw();
            }

            Vector3 ang = transform.localEulerAngles;
            ang.y += whirlSpeed.maxVal * Time.deltaTime;
            transform.localEulerAngles = ang;

            yield return new WaitForEndOfFrame();
        }


        startTime = Time.time;

        while (Time.time - startTime < chargeDuration)
        {
            float t = 1 - ((Time.time - startTime) / chargeDuration);

            Vector3 ang = transform.localEulerAngles;
            ang.y += whirlSpeed.PercentVal(t) * Time.deltaTime;
            transform.localEulerAngles = ang;

            yield return new WaitForEndOfFrame();
        }

        anim.SetBool("Attack2", false);

        attacking = false;
        canMove = true;
        nextAttack = Time.time + Random.Range(minTimeBetweenAttacks, maxTimeBetweenAttacks);
    }

    [Server]
    public override IEnumerator SpawnAnim()
    {
        /*yield return new WaitForSeconds(1);

        RpcSetAnimTrigger("Spawn");

        yield return new WaitForSeconds(.5f);*/

        yield return base.SpawnAnim();
    }

    [Server]
    public override IEnumerator DeathAnim()
    {
        /*yield return new WaitForSeconds(1);

        canMove = false;

        RpcSetAnimTrigger("Killed");*/

        yield return base.DeathAnim();
    }
}
