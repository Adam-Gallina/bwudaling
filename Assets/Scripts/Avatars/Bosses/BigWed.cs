using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigWed : BossBase
{
    [Header("Spicy Attack")]
    [SerializeField] protected BossAttackStats spicyStats;
    [SerializeField] private float[] spawnDelays;
    [SerializeField] private float maxSawSpreadAng;
    [SerializeField] protected float turnSpeed;
    [SerializeField] private float turnTime;
    [SerializeField] private int spicyHaiwSpawnCount;

    [Header("Whirl Attack")]
    [SerializeField] protected BossAttackStats whirlStats;
    [SerializeField] protected RangeF whirlSpeed;
    [SerializeField] protected float chargeDuration;
    [SerializeField] protected RangeF whirlDuration;
    [SerializeField] protected RangeF timeBetweenSpawns;
    [SerializeField] protected int spawnCount = 3;
    [SerializeField] private int whirlHaiwSpawnCount;

    [Header("Overheat Attack")]
    [SerializeField] protected BossAttackStats overheatStats;
    [SerializeField] protected int sawWaveCount;
    [SerializeField] protected int sawsPerWave;
    [SerializeField] protected float waveDelay;
    [SerializeField] protected float sawSpreadAng;
    [SerializeField] protected float aimSpeed;
    [SerializeField] protected float aimTime;
    [SerializeField] protected float sawLaunchDelay;
    [SerializeField][Range(0, 100)] protected int haiwSpawnChancePerWave;

    [Header("Spawn Anim")]
    [SerializeField] private Transform model;
    [SerializeField] private ParticleSystem flames;
    [SerializeField] private Vector3 modelOffset;

    protected override void Awake()
    {
        base.Awake();

        model.gameObject.SetActive(false);
    }

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
        for (int i = 0; i < spicyStats.bucketCount; i++)
            attackBucket.Add(BossAttack.Attack1);
        for (int i = 0; i < whirlStats.bucketCount; i++)
            attackBucket.Add(BossAttack.Attack2);
        for (int i = 0; i < overheatStats.bucketCount; i++)
            attackBucket.Add(BossAttack.Attack3);
    }

    protected override void DoAttack1()
    {
        StartCoroutine(SpicyAttack());
    }
    private IEnumerator SpicyAttack()
    {
        attacking = true;
        canMove = false;

        Transform target = GetClosestValidPlayer();

        RpcSetAnimTrigger("Attack");

        transform.forward = target.position - transform.position;

        float end = Time.time + turnTime;
        while (Time.time < end)
        {
            float ang = -Vector3.SignedAngle(transform.forward, target.position - transform.position, Vector3.up);
            float a = Mathf.Min(Mathf.Sign(ang) * turnSpeed * Time.deltaTime, ang);
            if (a != 0)
                transform.forward = MyMath.RotateAboutY(transform.forward, a);

            yield return new WaitForEndOfFrame();
        }

        int haiwDelta = spawnDelays.Length / spicyHaiwSpawnCount;
        for (int i = 0; i < spawnDelays.Length; i++)
        {
            yield return new WaitForSeconds(spawnDelays[i]);
            Vector3 dir = MyMath.RotateAboutY(transform.forward, Random.Range(-maxSawSpreadAng, maxSawSpreadAng));
            SpawnSaw(spicyStats.hazardPrefab, transform.position, dir, spicyStats.hazardSpeedMod);

            if (i % haiwDelta == 0)
                SpawnHaiw();
        }

        attacking = false;
        canMove = true;
        nextAttack = Time.time + Random.Range(minTimeBetweenAttacks, maxTimeBetweenAttacks);
    }

    protected override void DoAttack2()
    {
        StartCoroutine(WhirlAttack());
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

    protected override void DoAttack3()
    {
        StartCoroutine(Overheat());
    }

    private IEnumerator Overheat()
    {
        attacking = true;
        canMove = false;

        Transform target = GetClosestValidPlayer();

        transform.forward = target.position - transform.position;

        anim.SetBool("Attack3", true);

        float nextSpawn = Time.time + turnTime;
        float attackEnd = nextSpawn + sawWaveCount * waveDelay + sawsPerWave * sawLaunchDelay;
        float da = sawSpreadAng / (sawWaveCount * 2);
        while (Time.time < attackEnd)
        {
            float ang = -Vector3.SignedAngle(transform.forward, target.position - transform.position, Vector3.up);
            float a = Mathf.Min(Mathf.Sign(ang) * turnSpeed * Time.deltaTime, ang);
            if (a != 0)
                transform.forward = MyMath.RotateAboutY(transform.forward, a);

            if (Time.time >= nextSpawn)
            {
                int d = (int)Mathf.Sign(Random.Range(-1, 1));
                Vector2 dir = MyMath.Rotate(new Vector2(transform.forward.x, transform.forward.z), d * maxSawSpreadAng / 2);

                if (Random.Range(0, 100) < haiwSpawnChancePerWave)
                    SpawnHaiw();

                for (int _ = 0; _ < sawsPerWave; _++)
                {
                    SpawnSaw(overheatStats.hazardPrefab, transform.position, new Vector3(dir.x, 0, dir.y), overheatStats.hazardSpeedMod);
                    dir = MyMath.Rotate(dir, -d * da);

                    yield return new WaitForSeconds(sawLaunchDelay);
                }

                nextSpawn = Time.time + waveDelay;
            }

            yield return new WaitForEndOfFrame();
        }

        anim.SetBool("Attack3", false);
        attacking = false;
        canMove = true;
        nextAttack = Time.time + Random.Range(minTimeBetweenAttacks, maxTimeBetweenAttacks);
    }


    [Server]
    public override IEnumerator SpawnAnim()
    {
        StartAnim();

        yield return base.SpawnAnim();
    }

    [ClientRpc] 
    private void StartAnim() { StartCoroutine(Anim()); }
    private IEnumerator Anim()
    {
        model.gameObject.SetActive(true);
        flames.Play();
        CameraController.Instance.FocusOnPoint(model.position + Vector3.forward * 3);
        CameraController.Instance.SetZoom(3);

        float startTime = Time.time;
        while (Time.time < startTime + spawnAnimDuration - 2)
        {
            float t = (Time.time - startTime) / (spawnAnimDuration - 2);

            model.localPosition = modelOffset * (1 - t);

            yield return new WaitForEndOfFrame();
        }
        //currHealth = maxHealth;

        flames.Stop();
        model.localPosition = Vector3.zero;
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
