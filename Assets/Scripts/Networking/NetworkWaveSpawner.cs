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

        Vector3 forward = transform.forward * spawnOffset / 2;
        Vector3 back = transform.forward * -spawnOffset / 2;
        Vector3 left = transform.right * spawnHeight / 2;
        Vector3 right = transform.right * -spawnHeight / 2;

        Gizmos.DrawLine(transform.position + forward + right, transform.position + forward + left);
        Gizmos.DrawLine(transform.position + back + left, transform.position + forward + left);
        Gizmos.DrawLine(transform.position + back + left, transform.position + back + right);
        Gizmos.DrawLine(transform.position + forward + right, transform.position + back + right);
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