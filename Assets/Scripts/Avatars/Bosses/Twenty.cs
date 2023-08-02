using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Twenty : BossBase
{
    [Header("Wave Attack")]
    [SerializeField] protected BossAttack waveStats;
    [SerializeField] protected int sawWaveCount;
    [SerializeField] protected float sawSpreadAng;
    [SerializeField] protected float turnSpeed;
    [SerializeField] protected float aimTime;
    [SerializeField] protected float sawLaunchDelay;
    [SerializeField] protected RangeI haiwSpawns;


    [Header("Meteor Shower")]
    [SerializeField] protected BossAttack meteorStats;
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

    protected override void CheckBossAttacks()
    {
        if (Time.time > nextAttack && !attacking)
        {
            int n = Random.Range(0, waveStats.chance + meteorStats.chance);

            if (n < waveStats.chance)
                StartCoroutine(WaveAttack());
            else if (n < meteorStats.chance + waveStats.chance)
                StartCoroutine(MeteorAttack());
        }
    }

    protected IEnumerator WaveAttack()
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

        transform.forward = target.position - transform.position;

        float end = Time.time + aimTime;
        while (Time.time < end)
        {
            transform.forward = Vector3.RotateTowards(transform.forward, (target.position - transform.position).normalized, turnSpeed * Time.deltaTime * Mathf.Deg2Rad, 0);
            yield return new WaitForEndOfFrame();
        }

        anim.SetTrigger("Attack");
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

    protected IEnumerator MeteorAttack()
    {
        attacking = true;
        canMove = false;

        anim.SetTrigger("Attack2");
        yield return new WaitForSeconds(1);

        for (int _ = meteorCount.RandomVal; _ > 0; _--)
        {
            Vector2 c = Random.insideUnitCircle;
            Vector3 spawnPos = transform.position + new Vector3(c.x, 0, c.y) * meteorRange.RandomVal;

            int n = Random.Range(0, chanceToTargetPlayer + chanceForRandomDrop);
            if (n < chanceToTargetPlayer)
            {
                int tries = BwudalingNetworkManager.Instance.Players.Count;
                int p = Random.Range(0, BwudalingNetworkManager.Instance.Players.Count);

                while (tries-- > 0)
                {
                    PlayerAvatar pa = BwudalingNetworkManager.Instance.Players[p].avatar;
                    if (!pa.dead && pa.canDamage)
                    {
                        Vector2 dir = Random.insideUnitCircle * Random.Range(0, maxDistFromPlayer);
                        spawnPos = BwudalingNetworkManager.Instance.Players[p].avatar.transform.position + new Vector3(dir.x, 0, dir.y);
                        break;
                    }

                    p = (p + 1) % BwudalingNetworkManager.Instance.Players.Count;
                }
            }

            SpawnSaw(meteorStats.hazardPrefab, spawnPos, Vector3.zero, meteorStats.hazardSpeedMod);

            yield return new WaitForSeconds(timeBetweenMeteors.RandomVal);
        }

        attacking = false;
        canMove = true;
        nextAttack = Time.time + Random.Range(minTimeBetweenAttacks, maxTimeBetweenAttacks);
    }


    [Server]
    public override IEnumerator DeathAnim()
    {
        yield return new WaitForSeconds(1);

        canMove = false;

        anim.SetTrigger("Killed");

        yield return base.DeathAnim();
    }
}
