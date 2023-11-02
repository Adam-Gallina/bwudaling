using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Rendering.VirtualTexturing;

public class BasicSaw : RicochetProjectile
{    
    protected Vector3 spawnPos;
    protected Vector3 origin;
    protected float maxRange = -1;

    [SerializeField] protected float spinSpeed = 720;
    protected int spinDir;

    protected float size = 1;
    [SyncVar(hook = nameof(OnSizeChanged))]
    protected float currSize = 1;
    protected float sizeDebuffEnd;

    protected float currSpeedMod;
    protected float speedDebuffEnd;

    [Header("Effects")]
    [SerializeField] protected Transform model;
    [SerializeField] protected ParticleSystem sparkPrefab;
    [SerializeField] protected float sparkRadius;
    [SerializeField] protected float sparkHeight;
    private List<ParticleSystem> spawnedSparks = new List<ParticleSystem>();
    private Dictionary<Collider, ParticleSystem> activeCollisions = new Dictionary<Collider, ParticleSystem>();
    [SerializeField] private float sparkDespawwnTime = 0.15f;
    [SerializeField] private ParticleSystem deathPrefab;
    [SerializeField] private AudioSource sawWhine;
    [SerializeField] private float distForWhine;
    [SerializeField] private RandomAudio sawCrash;


    protected override void Awake()
    {
        base.Awake();

        size = currSize = transform.localScale.x;
        spinDir = Random.Range(0, 2) == 0 ? 1 : -1;
    }

    [Server]
    public virtual void SetSpawnLocation(Vector3 spawnLocation)
    {
        spawnPos = spawnLocation;
    }

    [Server]
    public virtual void SetOriginLocation(Vector3 origin, float maxRange = -1)
    {
        this.origin = origin;
        this.maxRange = maxRange;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (sparkPrefab == null) return;
        else if (activeCollisions.ContainsKey(other)) return;
        else if (((1 << other.gameObject.layer) & (freeRicochetLayers.value | costRicochetLayers.value)) == 0) 
            return;

        ParticleSystem system = null;
        if (spawnedSparks.Count > 0)
        {
            system = spawnedSparks[0];
            spawnedSparks.RemoveAt(0);
        }
        else
            system = Instantiate(sparkPrefab, transform);

        Vector3 dir = (other.ClosestPoint(transform.position) - transform.position).normalized;
        system.transform.position = transform.position + dir * sparkRadius + Vector3.up * sparkHeight;
        system.transform.forward = dir;
        system.Play();

        activeCollisions.Add(other, system);
    }

    protected override void OnTriggerStay(Collider other)
    {
        base.OnTriggerStay(other);

        if (!activeCollisions.ContainsKey(other))
            return;

        Vector3 dir = (other.ClosestPoint(transform.position) - transform.position).normalized;
        activeCollisions[other].transform.position = transform.position + dir * sparkRadius + Vector3.up * sparkHeight;
        activeCollisions[other].transform.forward = dir;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!activeCollisions.ContainsKey(other))
            return;
        StartCoroutine(StopSparks(activeCollisions[other]));
        activeCollisions.Remove(other);
    }

    private IEnumerator StopSparks(ParticleSystem sparks)
    {
        yield return new WaitForSeconds(sparkDespawwnTime);

        sparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        yield return new WaitForSeconds(sparks.main.startLifetime.Evaluate(1));
        spawnedSparks.Add(sparks);
    }

    protected override void Update()
    {
        model.localEulerAngles = new Vector3(0, model.localEulerAngles.y + spinSpeed * spinDir * Time.deltaTime, 0);

        if (sawWhine)
        {
            float dist = Vector3.Distance(transform.position, BwudalingNetworkManager.Instance.ActivePlayer.avatar.transform.position);
            if (dist <= distForWhine)
            {
                sawWhine.volume = 1 - ((dist - 2) / (distForWhine - 2));
                if (!sawWhine.isPlaying)
                    sawWhine.Play();
            }
            else if (sawWhine.isPlaying)
                sawWhine.Stop();
        }

        if (!isServer)
            return;

        if (currSize != size && Time.time > sizeDebuffEnd
            && Physics.OverlapSphere(transform.position, currSize, 1 << Constants.EnvironmentLayer | 1 << Constants.HazardBoundaryLayer | 1 << Constants.HazardLayer).Length == 1)
                currSize = size;
        if (currSpeedMod != 1 && Time.time > speedDebuffEnd)
            currSpeedMod = 1;

        rb.velocity = currVelocity.normalized * speed * currSpeedMod;
    }

    [Server]
    protected override void OnHitTarget(Collider other)
    {
        AvatarBase target = other.gameObject.GetComponentInParent<AvatarBase>();
        if (target)
        {
            target.Damage();
        }
    }

    [Server]
    public void ApplySizeMod(float newSizeMod, float duration)
    {
        sizeDebuffEnd = Time.time + duration;

        if (size * newSizeMod < currSize)
            currSize = size * newSizeMod;
            
    }
    
    private void OnSizeChanged(float _, float newSize)
    {
        transform.localScale = new Vector3(newSize, newSize, newSize);
    }

    [Server]
    public void ApplySpeedMod(float newSpeedMod, float duration)
    {
        speedDebuffEnd = Time.time + duration;

        if (newSpeedMod < currSpeedMod)
            currSpeedMod = newSpeedMod;
    }

    [ClientRpc]
    protected override void RpcOnRicochet()
    {
        if (sawCrash)
            sawCrash.Play();
    }

    [Server]
    public override void DestroyObject(bool playDeathAnim=true)
    {
        OnDestroyObject(playDeathAnim);
    }

    [ClientRpc]
    protected virtual void OnDestroyObject(bool playDeathAnim)
    {
        if (deathPrefab && playDeathAnim)
        {
            ParticleSystem ps = Instantiate(deathPrefab, transform.position, Quaternion.identity);
            ps.gameObject.SetActive(true);
            ps.Play();
        }

        if (isServer)
            NetworkServer.Destroy(gameObject);
    }
}
