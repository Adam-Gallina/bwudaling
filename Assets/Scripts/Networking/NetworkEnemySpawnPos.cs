using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkEnemySpawnPos : MonoBehaviour
{
    public bool spawn = true;
    public NetworkIdentity enemyPrefab;
    [SerializeField] protected float spawnSize;

    [SerializeField] protected float hazardSpeed;
    [SerializeField] protected float hazardRange;
    [SerializeField] protected bool useSpawnerSpeed;
    [SerializeField] protected bool useSpawnerOrigin;
    [SerializeField] protected bool useSpawnerRange;

    [SerializeField] private GameObject model;

    private void Awake()
    {
        if (model)
            model.SetActive(false);
    }

    [Server]
    public virtual bool SpawnEnemy(bool validatePos = true)
    {
        return SpawnEnemy(GetSpawnPos(), GetSpawnRot(), validatePos);
    }
    [Server]
    public virtual bool SpawnEnemy(Vector3 pos, Vector3 dir, bool validatePos = true)
    {
        if (validatePos)
            if (!ValidateSpawnPos(pos))
                return false;

        GameObject newEnemy = Instantiate(enemyPrefab.gameObject, pos, Quaternion.Euler(dir));
        NetworkServer.Spawn(newEnemy);

        BasicSaw newSaw = newEnemy.GetComponent<BasicSaw>();
        newSaw.SetSpeed(useSpawnerSpeed ? hazardSpeed : MapController.Instance.hazardSpeed);
        newSaw.SetOriginLocation(
            useSpawnerOrigin ? transform.position : MapController.Instance.mapCenter, 
            useSpawnerRange ? hazardRange : MapController.Instance.hazardRange);
        newSaw.SetSpawnLocation(pos);

        return true;
    }

    protected virtual bool ValidateSpawnPos(Vector3 pos)
    {
        return Physics.OverlapSphere(pos, spawnSize / 2, 1 << Constants.HazardLayer).Length == 0;
    }

    public virtual Vector3 GetSpawnPos()
    {
        return transform.position;
    }

    public virtual Vector3 GetSpawnRot()
    {
        return new Vector3(0, Random.Range(0, 360), 0);
    }
}
