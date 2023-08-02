using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkSpiralSpawner : NetworkEnemySpawnPos
{
    [SerializeField] protected int amountPerSpawn;
    [SerializeField] protected float spawnRate;
    [SerializeField] protected float minTurnAng;
    [SerializeField] protected float maxTurnAng;
    protected float nextSpawn;
    protected float nextSpawnAng;

    [ServerCallback]
    private void Update()
    {
        if (Time.time > nextSpawn)
        {
            nextSpawn = Time.time + spawnRate;
            nextSpawnAng += Random.Range(minTurnAng, maxTurnAng);

            for (int i = 0; i < amountPerSpawn; i++)
                SpawnEnemy(false);
        }
    }

    private float lastSpawnRot;
    public override Vector3 GetSpawnRot()
    {
        lastSpawnRot += 360 / amountPerSpawn;
        return new Vector3(0, nextSpawnAng + lastSpawnRot, 0);
    }
}
