using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class BossBase : NetworkBehaviour
{
    protected enum BossAttack { Attack1, Attack2, Attack3 };

    public float maxHealth;
    [SyncVar]
    [HideInInspector] public float currHealth;
    [SerializeField] protected float spawnAnimDuration;
    [SerializeField] protected float deathAnimDuration;

    protected bool spawned = false;
    protected bool canMove = false;
    protected bool attacking = false;

    [Header("Haiw Spawning")]
    [SerializeField] protected ItemBase haiwPrefab;
    [SerializeField] protected RangeF haiwSpawnVelocity;
    [SerializeField] protected float haiwSpawnVelocityY;
    [SerializeField] protected Transform haiwSpawnPoint;

    [Header("Random Movement")]
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float maxMoveDist;
    [SerializeField] protected float moveBuffer;
    [SerializeField] protected float targetPosAccuracy = .1f;
    protected Vector3 targetPos;
    [SerializeField] protected float rotationSpeed;

    [Header("Combat")]
    [SerializeField] protected float minTimeBetweenAttacks;
    [SerializeField] protected float maxTimeBetweenAttacks;
    [SerializeField] protected float startAttackDelayMod = 2;
    protected float nextAttack;

    protected List<BossAttack> attackBucket = new List<BossAttack>();

    protected Animator anim;

    protected virtual void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == Constants.PlayerLayer)
        {
            PlayerAvatar p = other.gameObject.GetComponent<PlayerAvatar>();
            if (p)
                p.Damage();
        }
    }

    [Server]
    public virtual IEnumerator SpawnAnim()
    {
        float healthStep = maxHealth / spawnAnimDuration;
        while (currHealth < maxHealth)
        {
            currHealth += healthStep * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        currHealth = maxHealth;

        OnSpawn();
    }

    protected virtual void OnSpawn()
    {
        spawned = true;
        canMove = true;
        targetPos = GetBossMoveTarget(Random.Range(0, maxMoveDist));
        nextAttack = Time.time + Random.Range(minTimeBetweenAttacks, maxTimeBetweenAttacks) * startAttackDelayMod;
    }

    [ServerCallback]
    private void Update()
    {
        if (spawned)
        {
            CheckBossMovement();
            CheckBossAttacks();

            if (currHealth <= 0)
            {
                currHealth = 0;
                GameController.Instance.HandlePlayerWin(null);
            }
        }
    }

    protected bool IsValidMovePos(Vector3 pos) 
    {
        return !Physics.Raycast(transform.position, (pos - transform.position), Vector3.Distance(transform.position, pos) + moveBuffer, 1 << Constants.HazardBoundaryLayer | 1 << Constants.EnvironmentLayer);
    }
    protected virtual void CheckBossMovement()
    {
        if (Vector3.Distance(transform.position, targetPos) < targetPosAccuracy)
        {
            targetPos = GetBossMoveTarget(Random.Range(0, maxMoveDist));
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        transform.forward = Vector3.RotateTowards(transform.forward, (targetPos - transform.position).normalized, rotationSpeed * Mathf.Deg2Rad * Time.deltaTime, 0);
    }
    protected virtual Vector3 GetBossMoveTarget(float distance)
    {
        Vector2 randPos = Random.insideUnitCircle * distance;
        Vector3 pos;

        for (int _ = 0; _ < 4; _++)
        {
            pos = transform.position + new Vector3(randPos.x, 0, randPos.y);
            if (IsValidMovePos(pos))
                return pos;
            randPos = MyMath.Rotate(randPos, 90);
        }

        Debug.LogWarning(name + " couldn't find a valid move direction :(");
        return transform.position;
    }

    protected abstract void FillAttackBucket();
    protected virtual void CheckBossAttacks()
    {
        if (Time.time < nextAttack)
            return;

        nextAttack = Time.time + Random.Range(minTimeBetweenAttacks, maxTimeBetweenAttacks);

        if (attackBucket.Count == 0)
            FillAttackBucket();

        int i = Random.Range(0, attackBucket.Count);
        BossAttack attack = attackBucket[i];
        attackBucket.RemoveAt(i);

        switch (attack)
        {
            case BossAttack.Attack1: DoAttack1(); break;
            case BossAttack.Attack2: DoAttack2(); break;
            case BossAttack.Attack3: DoAttack3(); break;
        }
    }

    protected abstract void DoAttack1();
    protected abstract void DoAttack2();
    protected abstract void DoAttack3();




    #region Haiw Spawning
    [Server]
    protected void SpawnHaiw()
    {
        SpawnHaiw(GetHaiwSpawnDir());
    }
    [Server] 
    protected void SpawnHaiw(Vector3 targetDir)
    {
        ItemBase haiw = Instantiate(haiwPrefab, haiwSpawnPoint.position, Quaternion.identity);
        NetworkServer.Spawn(haiw.gameObject);

        Vector2 v = GetHaiwSpawnVelocity();
        haiw.GetComponent<Rigidbody>().velocity = new Vector3(targetDir.x * v.x, targetDir.y, targetDir.z * v.x) + Vector3.up * v.y;

        //RpcOnSpawnHaiw();
    }

    [Server]
    protected virtual Vector3 GetHaiwSpawnDir()
    {
        Vector2 p = Random.insideUnitCircle;
        return new Vector3(p.x, 0, p.y);
    }

    [Server]
    protected virtual Vector2 GetHaiwSpawnVelocity()
    {
        return new Vector2(haiwSpawnVelocity.RandomVal, haiwSpawnVelocityY);
    }

    [ClientRpc]
    protected virtual void RpcOnSpawnHaiw()
    {

    }
    #endregion

    [Server]
    protected BasicSaw SpawnSaw(BasicSaw prefab, Vector3 pos, Vector3 dir, float speedMod = 1)
    {
        BasicSaw saw = Instantiate(prefab, pos, Quaternion.identity);
        NetworkServer.Spawn(saw.gameObject);
        saw.SetSpawnLocation(pos);
        saw.SetSpeed(MapController.Instance.hazardSpeed * speedMod);
        saw.SetDirection(dir);

        RpcSpawnSaw(pos, dir);

        return saw;
    }
    [ClientRpc]
    protected void RpcSpawnSaw(Vector3 pos, Vector3 dir)
    {

    }


    [Server]
    public virtual IEnumerator DeathAnim()
    {
        yield return new WaitForSeconds(deathAnimDuration);
    }
    [ClientRpc]
    protected void RpcSetAnimTrigger(string trigger)
    {
        anim.SetTrigger(trigger);
    }
}

[System.Serializable]
public struct BossAttackStats
{
    public BasicSaw hazardPrefab;
    public float hazardSpeedMod;
    public int bucketCount;
}