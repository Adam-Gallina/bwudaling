using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkWaveSpawner : NetworkSpiralSpawner
{
    [SerializeField] protected float spawnHeight;
    [SerializeField] protected float spawnOffset;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 3);
        
        Gizmos.DrawWireCube(transform.position, transform.forward * spawnOffset + transform.right * spawnHeight);
    }

    public override Vector3 GetSpawnPos()
    {
        return transform.position + transform.right * Random.Range(-spawnHeight / 2, spawnHeight / 2) + transform.forward * Random.Range(-spawnOffset / 2, spawnOffset / 2);
    }

    public override Vector3 GetSpawnRot()
    {
        return transform.localEulerAngles;
    }
}