using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BasicSaw : RicochetProjectile
{
    [SerializeField] protected Transform model;

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

    protected bool canHitPlayer = true;

    protected override void Awake()
    {
        base.Awake();

        size = currSize = model.localScale.x;
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

    protected override void Update()
    {
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y + spinSpeed * spinDir * Time.deltaTime, 0);

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
        if (!canHitPlayer)
            return;

        AvatarBase target = other.gameObject.GetComponentInParent<AvatarBase>();
        if (target)
            target.Damage();
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
        model.localScale = new Vector3(newSize, newSize, newSize);
    }

    [Server]
    public void ApplySpeedMod(float newSpeedMod, float duration)
    {
        speedDebuffEnd = Time.time + duration;

        if (newSpeedMod < currSpeedMod)
            currSpeedMod = newSpeedMod;
    }
}
