using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Meteor : BasicSaw
{
    [Header("Spawns")]
    [SerializeField] protected BasicSaw spawnedSawPrefab;
    [SerializeField] protected int spawnCount;
    [SerializeField] protected BwudaHaiw haiwPrefab;

    [Header("Animation")]
    [SerializeField] protected float spawnDelay;
    [SerializeField] protected float spawnDuration;
    [SerializeField] protected float spawnHeight;
    [SerializeField] protected ParticleSystem spawnAnim;

    public override void SetSpawnLocation(Vector3 spawnLocation)
    {
        base.SetSpawnLocation(spawnLocation);

        StartCoroutine(SpawnAnim());
    }

    [Server]
    protected void SetVisbility(bool visible)
    {
        RpcSetVisibility(visible);
    }

    [ClientRpc]
    protected void RpcSetVisibility(bool visible)
    {
        model.gameObject.SetActive(visible);
    }

    [Server]
    protected virtual IEnumerator SpawnAnim()
    {
        transform.position = spawnPos + Vector3.up * spawnHeight;

        RpcSetVisibility(true);
        yield return new WaitForSeconds(spawnDelay);

        RpcOnLand();
        spawnAnim.Play();

        yield return new WaitForSeconds(spawnDuration);

        SpawnPayload();

        yield return new WaitUntil(() => !spawnAnim.isPlaying);
        ParticleSystem ps = model.gameObject.GetComponent<ParticleSystem>();
        ps.Stop();
        yield return new WaitForSeconds(ps.main.startLifetime.constantMax);

        DestroyObject();

        yield return null;
    }

    protected virtual void SpawnPayload()
    {
        if (spawnedSawPrefab)
        {
            float da = 360 / spawnCount;
            Vector2 nextDir = MyMath.Rotate(new Vector2(transform.forward.x, transform.forward.z), Random.Range(0, da));

            for (int i = 0; i < spawnCount; i++)
            {
                BasicSaw saw = Instantiate(spawnedSawPrefab, spawnPos, Quaternion.identity);
                NetworkServer.Spawn(saw.gameObject);
                saw.SetSpawnLocation(spawnPos);
                saw.SetSpeed(speed);
                saw.SetDirection(new Vector3(nextDir.x, 0, nextDir.y));
                saw.SetOriginLocation(MapController.Instance.mapCenter, MapController.Instance.hazardRange);

                nextDir = MyMath.Rotate(nextDir, da);
            }
        }

        if (haiwPrefab)
        {
            BwudaHaiw h = Instantiate(haiwPrefab, spawnPos, Quaternion.identity);
            NetworkServer.Spawn(h.gameObject);
        }
            
    }

    [ClientRpc]
    protected void RpcOnLand()
    {
        if (!isServer)
        {
            spawnAnim.Play();

            ParticleSystem ps = model.gameObject.GetComponent<ParticleSystem>();
            Invoke(nameof(ps.Stop), spawnDuration);
        }
    }
}
