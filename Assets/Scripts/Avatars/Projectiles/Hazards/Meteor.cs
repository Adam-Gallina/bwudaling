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

        RpcSetVisibility(false);
        yield return new WaitForSeconds(spawnDelay);
        RpcSetVisibility(true);

        float start = Time.time;
        while (Time.time < start + spawnDuration) 
        {
            float t = (Time.time - start) / spawnDuration;
            transform.position = spawnPos + Vector3.up * spawnHeight * (1 - t);

            yield return null;
        }

        transform.position = spawnPos;

        SpawnPayload();
        RpcOnLand();

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

    }
}
