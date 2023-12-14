using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BossGameController : BasicGameController
{
    //[SerializeField] protected BossBase bossPrefab;
    [SerializeField] protected float bossSpawnTime;
    [SyncVar(hook=nameof(OnSpawnBoss))]
    [HideInInspector] public BossBase spawnedBoss;

    [Server]
    protected override IEnumerator BeforeStartMessages()
    {
        BossBase boss = Instantiate(((BossMapController)MapController.Instance).bossPrefab, GameObject.Find("Boss Spawn Pos").transform.position, GameObject.Find("Boss Spawn Pos").transform.rotation);
        NetworkServer.Spawn(boss.gameObject);
        spawnedBoss = boss;

        yield return StartCoroutine(spawnedBoss.SpawnAnim());

        RpcSetPlayerCamera();
    }

    private void OnSpawnBoss(BossBase _, BossBase boss)
    {
        GameUI.Instance.SetBossHealthTarget(boss);
        CameraController.Instance.SetTarget(boss.transform, 1);
        CameraController.Instance.FocusOnPoint(boss.transform.position + Vector3.forward * 3);
    }

    [Server]
    protected override IEnumerator EndSequence(NetworkPlayer p)
    {
        RpcSetCamera(spawnedBoss, 1);

        RpcSendServerBannerMessage(spawnedBoss.name.Remove(spawnedBoss.name.Length - 7) + " was defeated!", 0);

        spawnedBoss.StopAllCoroutines();
        yield return StartCoroutine(spawnedBoss.DeathAnim());

        RpcSendServerBannerMessage(string.Empty, 0);
        yield return null;

        BwudalingNetworkManager.Instance.NextMap();
    }
}
