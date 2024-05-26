using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Action = System.Action;

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
    [SerializeField] private float frayEdgeRadius = 110;
    [SerializeField] private float frayMoveSpeed;
    [SerializeField] private RangeI frayCharges;
    [Range(0, 100)]
    [SerializeField] protected int frayHaiwSpawnChance = 30;

    [Header("Tangle Attack")]
    [SerializeField] protected BossAttackStats tangleStats;
    [SerializeField] private float tanglePullDuration = 3;
    [SerializeField] private float tanglePullSpeedMod = .8f;
    [SerializeField] private float tangleExplodeSpeedMod = 1.35f;
    [SerializeField] private float tangleExplodeModDuration = 10;
    [SerializeField] private int tangleSpawnCount = 12;
    [SerializeField] private float tangleAnimRadius = 10;
    [SerializeField] private float tangleAnimMoveSpeed = 24;
    [SerializeField] private RangeI tangleHaiwSpawns;


    [Header("Spawn Anim")]
    [SerializeField] private float spawnAnimSegmentPlaceDelay;
    [SerializeField] private AudioSource placeAudio;

    [Header("Death Anim")]
    [SerializeField] private float jitterMoveSpeed;
    [SerializeField] private float jitterRadius;
    [SerializeField] private float deathAnimFirstSegmentDeath;
    [SerializeField] private float deathAnimSegmentDeathDelay;
    [SerializeField] private float deathAnimCamMoveSpeed;
    [SerializeField] private ParticleSystem segmentDeathPrefab;

    private void OnDrawGizmos()
    {
        if (moveCurves.Count == 0)
            return;

        /*Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, minMoveDist);
        Gizmos.DrawWireSphere(transform.position, maxMoveDist);*/

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(moveCurves[0].start + Vector3.up * 6, moveCurves[0].start + moveCurves[0].startConstraint + Vector3.up * 6);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(moveCurves[0].end + Vector3.up * 6, moveCurves[0].end + moveCurves[0].endConstraint + Vector3.up * 6);

        Gizmos.color = Color.green;
        Vector3 lastPos = moveCurves[0].start;
        for (int i = 1; i < bezierSamples; i++)
        {
            Vector3 pos = moveCurves[0].Sample((float)i / bezierSamples);
            Gizmos.DrawLine(lastPos + Vector3.up * 5, pos + Vector3.up * 5);
            lastPos = pos;
        }

        Gizmos.color = Color.yellow;
        for (int j = 1; j < moveCurves.Count; j++) {
            lastPos = moveCurves[j].start;
            for (int i = 1; i < bezierSamples; i++)
            {
                Vector3 pos = moveCurves[j].Sample((float)i / bezierSamples);
                Gizmos.DrawLine(lastPos + Vector3.up * 5, pos + Vector3.up * 5);
                lastPos = pos;
            }
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
        //segments[0].position = transform.position + Vector3.up * segments[0].position.y;

        Vector3[] segmentPositions = new Vector3[segments.Length - 1];

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

            //segments[i].position = moveCurves[currCurve].Sample(animT) + Vector3.up * segments[i].position.y;
            segmentPositions[i - 1] = moveCurves[currCurve].Sample(animT);
        }

        RpcUpdateSaws(segmentPositions);

        // Clean up any unused curves
        if (currCurve < moveCurves.Count - 1)
            moveCurves.RemoveAt(moveCurves.Count - 1);
    }

    [ClientRpc]
    private void RpcUpdateSaws(Vector3[] segmentPositions)
    {
        for (int i = 1; i < segments.Length; i++)
        {
            segments[i].position = segmentPositions[i - 1] + Vector3.up * segments[i].position.y;
        }
    }

    protected override Vector3 GetBossMoveTarget(float distance)
    {
        if (moveCurves.Count == 0 || Vector3.Distance(transform.position, moveCurves[0].end) < targetPosAccuracy || lastBezierT == 1)
            GetNewMovementCurve(base.GetBossMoveTarget(distance));

        return moveCurves[0].Sample(1);
    }

    private void GetNewMovementCurve(Vector3 target, Vector3? targetControl = null, float controlSize = 0)
    {
        if (controlSize <= 0)
            controlSize = bezierControlSize;
        if (!targetControl.HasValue)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            targetControl = new Vector3(dir.x, 0, dir.y) * controlSize;
        }

        Vector3 s = transform.position;
        Vector3 sc = transform.forward * controlSize;
        moveCurves.Insert(0, new Bezier(s, sc, target, targetControl.Value));

        lastBezierT = 0;

        moveCurves[0].ApproximateLength(bezierSamples);
    }

    #region Attacks
    private IEnumerator MoveThroughCurve(Action onMove = null)
    {
        while (lastBezierT < 1)
        {
            DoBossMovement();

            if (onMove != null)
                onMove.Invoke();

            yield return new WaitForEndOfFrame();
        }
    }

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
        attacking = true;
        canMove = false;
        float lastMoveSpeed = moveSpeed;

        yield return MoveThroughCurve();
        Vector3 currDir = (transform.position - MapController.Instance.mapCenter).normalized;
        Vector3 t = MyMath.RotateAboutY(currDir, Random.Range(30, 60)) * frayEdgeRadius * (Random.Range(0, 2) == 0 ? 1 : -1);
        GetNewMovementCurve(MapController.Instance.mapCenter + t, (t - transform.position).normalized);
        yield return MoveThroughCurve();

        moveSpeed = frayMoveSpeed;
        for (int i = frayCharges.RandomVal; i > 0; i--)
        {
            // Move to new target start
            currDir = (transform.position - MapController.Instance.mapCenter).normalized;
            float ang = Random.Range(30, 60) * (Random.Range(0, 2) == 0 ? 1 : -1);
            t = MyMath.RotateAboutY(currDir, ang) * frayEdgeRadius;
            GetNewMovementCurve(MapController.Instance.mapCenter + t, (MapController.Instance.mapCenter + t - transform.position).normalized);

            yield return MoveThroughCurve();

            // Charge thru map
            currDir = (MapController.Instance.mapCenter - transform.position).normalized;
            ang = Random.Range(-30, 30);
            t = MyMath.RotateAboutY(currDir, ang) * frayEdgeRadius;
            GetNewMovementCurve(MapController.Instance.mapCenter + t, currDir);

            // Spawn haiw at halfway point
            bool spawnedHaiw = false;
            yield return MoveThroughCurve(() =>
            {
                if (!spawnedHaiw && lastBezierT >= .5f)
                {
                    spawnedHaiw = true;
                    if (Random.Range(0, 100) <= frayHaiwSpawnChance)
                    {
                        SpawnHaiw();
                    }
                }
            });
        }

        Vector3 toCenter = (MapController.Instance.mapCenter - transform.position).normalized;
        toCenter = MyMath.RotateAboutY(toCenter, Random.Range(-30, 30));
        GetNewMovementCurve(transform.position + toCenter * frayEdgeRadius * .75f);
        targetPos = moveCurves[0].Sample(1);

        attacking = false;
        canMove = true;
        nextAttack = Time.time + Random.Range(minTimeBetweenAttacks, maxTimeBetweenAttacks);
        moveSpeed = lastMoveSpeed;
    }

    protected override void DoAttack3()
    {
        StartCoroutine(TangleAttack());
    }

    private IEnumerator TangleAttack()
    {
        attacking = true;
        canMove = false;

        yield return MoveThroughCurve();
        GetNewMovementCurve(MapController.Instance.mapCenter, (MapController.Instance.mapCenter - transform.position).normalized);
        yield return MoveThroughCurve();

        BasicSaw[] saws = MapController.Instance.GetComponentsInChildren<BasicSaw>();
        foreach (BasicSaw saw in saws)
        {
            if (saw == null) continue;

            saw.SetDirection(transform.position - saw.transform.position);
            saw.ApplySpeedMod(tanglePullSpeedMod, tanglePullDuration * 1.25f, true);
        }

        float speed = moveSpeed;
        moveSpeed = tangleAnimMoveSpeed;

        float end = Time.time + tanglePullDuration;
        while (Time.time < end)
        {
            Vector2 d = Random.insideUnitCircle * tangleAnimRadius;
            Vector3 pos = MapController.Instance.mapCenter + new Vector3(d.x, 0, d.y);
            GetNewMovementCurve(pos, (pos - transform.position).normalized, tangleAnimRadius / 2);

            yield return MoveThroughCurve();
        }

        moveSpeed = speed;

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

    private bool receivedCb = false;
    [Server]
    public override IEnumerator SpawnAnim()
    {
        RpcStartAnim();

        yield return new WaitForSeconds(1);

        BossAnimations.Callback1 += OnServerAnimCallback;
        RpcSetAnimTrigger("Spawn");

        yield return new WaitUntil(() => receivedCb);

        yield return base.SpawnAnim();
    }
    private void OnServerAnimCallback()
    {
        receivedCb = true;
        BossAnimations.Callback1 -= OnServerAnimCallback;
    }

    [ClientRpc]
    private void RpcStartAnim()
    {
        CameraController.Instance.SetTarget(MapController.Instance.transform, 1);
        CameraController.Instance.FocusOnPoint(MapController.Instance.mapCenter);
        CameraController.Instance.SetZoom(12);

        BossAnimations.Callback1 += OnAnimCallback;
    }
    private void OnAnimCallback()
    {
        BossAnimations.Callback1 -= OnAnimCallback;
        StartCoroutine(Anim());
    }
    private IEnumerator Anim()
    {
        GetComponent<NetworkAnimator>().enabled = false;
        GetComponentInChildren<Animator>().enabled = false;

        Vector3 s = segments[segments.Length - 1].position;
        Vector3 e = transform.position;
        s.y = e.y = 0;
        moveCurves.Add(new Bezier(s, Vector3.right * 35 + Vector3.forward * .8f * 35,
                                  e, Vector3.right * .8f * 35 + Vector3.back * 35));
        moveCurves[0].ApproximateLength(bezierSamples);

        Vector3[] targetSegmentPos = new Vector3[segments.Length];
        for (int i = 0; i < segments.Length; i++)
            targetSegmentPos[i] = moveCurves[0].Sample(1 - ((float)i / (segments.Length-1)));

        Vector3[] lastSegmentPos = new Vector3[segments.Length];
        for (int i = 0; i < segments.Length; i++)
            lastSegmentPos[i] = segments[i].position;

        float next = Time.time + spawnAnimSegmentPlaceDelay;
        int placed = 0;
        while (placed < segments.Length)
        {
            for (int i = placed; i < segments.Length; i++)
            {
                Vector2 d = Random.insideUnitCircle * jitterRadius;
                Vector3 pos = lastSegmentPos[i] + new Vector3(d.x, 0, d.y);

                segments[i].position = segments[i].position + (pos - segments[i].position).normalized * jitterMoveSpeed * Time.deltaTime;
                segments[i].localEulerAngles = new Vector3(0, segments[i].localEulerAngles.y, 0);
            }

            if (Time.time >= next)
            {
                segments[placed].position = targetSegmentPos[placed] + Vector3.up * 1.75f;
                placed++;
                placeAudio.Play();
                next = Time.time + spawnAnimSegmentPlaceDelay;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    [Server]
    public override IEnumerator DeathAnim()
    {
        yield return new WaitForSeconds(1);

        canMove = false;
        attacking = true;

        RpcStartDeath();

        yield return base.DeathAnim();
    }

    [ClientRpc]
    private void RpcStartDeath() { StartCoroutine(Death()); }
    private IEnumerator Death()
    {
        CameraController.Instance.SetZoom(8);

        Vector3[] lastSegmentPos = new Vector3[segments.Length];
        for (int i = 0; i < segments.Length; i++)
            lastSegmentPos[i] = segments[i].position;

        float next = Time.time + deathAnimFirstSegmentDeath;
        int destroyed = 0;
        float camT = lastBezierT;
        float camDt = deathAnimCamMoveSpeed * Time.deltaTime / moveCurves[0].length;
        int currCamCurve = 0;
        float camMoveStart = Time.time + deathAnimFirstSegmentDeath;

        while (destroyed < segments.Length)
        {
            for (int i = destroyed; i < segments.Length; i++)
            {
                Vector2 d = Random.insideUnitCircle * jitterRadius;
                Vector3 pos = lastSegmentPos[i] + new Vector3(d.x, 0, d.y);

                segments[i].position = segments[i].position + (pos - segments[i].position).normalized * jitterMoveSpeed * Time.deltaTime;
            }

            if (Time.time >= next)
            {
                ParticleSystem ps = Instantiate(segmentDeathPrefab, segments[destroyed].position, Quaternion.identity);
                ps.gameObject.SetActive(true);
                ps.Play();

                Destroy(segments[destroyed].gameObject);
                destroyed++;
                next = Time.time + deathAnimSegmentDeathDelay;
            }

            if (Time.time >= camMoveStart)
            {
                camT -= camDt;

                if (camT < 0)
                {
                    currCamCurve++;
                    if (currCamCurve >= moveCurves.Count)
                    {
                        currCamCurve--;
                    }
                    else
                    {
                        float remainingDist = deathAnimCamMoveSpeed - (camT + camDt) * moveCurves[currCamCurve - 1].length;
                        camT = 1 - (remainingDist / moveCurves[currCamCurve].length);
                        camDt = deathAnimCamMoveSpeed * Time.deltaTime / moveCurves[currCamCurve].length;
                    }
                }

                CameraController.Instance.FocusOnPoint(moveCurves[currCamCurve].Sample(camT));
            }

            yield return new WaitForEndOfFrame();
        }
    }
}
