using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkEnemySpawnRegion : NetworkEnemySpawnPos
{
    [SerializeField] protected Vector2 spawnBounds;
    [SerializeField] protected int spawnCount;
    [SerializeField] protected int spawnTries = 10;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.grey;
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnBounds.x, 0, spawnBounds.y) * 2);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnBounds.x - spawnSize, 0, spawnBounds.y - spawnSize) * 2);
    }

    [Server]
    public override bool SpawnEnemy(bool validatePos = true)
    {
        int spawned = 0;
        for (int i = 0; i < spawnCount; i++)
        {
            for (int t = 0; t < spawnCount; t++)
                if (base.SpawnEnemy(spawnTries > 0))
                {
                    spawned++;
                    break;
                }
        }

        return spawned > 0;
    }

    public override Vector3 GetSpawnPos()
    {
        return transform.position + new Vector3(Random.Range(-spawnBounds.x + spawnSize, spawnBounds.x - spawnSize), 0, Random.Range(-spawnBounds.y + spawnSize, spawnBounds.y - spawnSize));
    }
}
