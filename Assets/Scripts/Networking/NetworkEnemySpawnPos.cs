using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkEnemySpawnPos : MonoBehaviour
{
    public bool spawn = true;
    public NetworkIdentity enemyPrefab;
    [SerializeField] protected float spawnSize;

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

        newEnemy.GetComponent<BasicSaw>().SetSpeed(MapController.Instance.hazardSpeed);
        //newEnemy.GetComponent<BasicSaw>().SetMaxBounces(-1);
        newEnemy.GetComponent<BasicSaw>().SetSpawnLocation(pos);

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
