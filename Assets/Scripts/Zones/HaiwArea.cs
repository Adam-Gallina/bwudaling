using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using static UnityEngine.GraphicsBuffer;

public class HaiwArea : SafeArea
{
    [HideInInspector] public float chargeLevel = 0;
    [SerializeField] protected float damageSpeed;
    [SerializeField] protected float chargePerHaiw;

    [SerializeField] protected int deliveryXp = 25;

    [SerializeField] protected BossDamageAnim damageAnim;

    [Header("Haiw Deposit Anim")]
    [SerializeField] private AnimationCurve depositAnim;
    [SerializeField] private float depositAnimTime;
    [SerializeField] private Transform depositAnimTarget;

    [ServerCallback]
    private void Update()
    {
        if (BwudalingNetworkManager.Instance.DEBUG_AllowKeyCheats && Input.GetKeyDown(KeyCode.Delete))
            chargeLevel += chargePerHaiw;

        if (chargeLevel > 0)
        {
            chargeLevel -= Time.deltaTime;
            ((BossGameController)GameController.Instance).spawnedBoss.currHealth -= damageSpeed * Time.deltaTime;
        }
        else if (chargeLevel < 0)
            chargeLevel = 0;


        damageAnim.SetAttacking(chargeLevel > 0);
    }

    [Server]
    protected override void ServerOnTouchPlayer(PlayerAvatar target)
    {
        if (target.heldItem)
        {
            if (target.heldItem.itemType == ItemType.BwudaHaiw)
            {
                StartCoroutine(OnCollectHaiw(target.heldItem));
                target.RpcAddXp(MapController.Instance.safeZoneXp, false);
            }
        }
    }

    [Server]
    protected IEnumerator OnCollectHaiw(ItemBase haiw)
    {
        haiw.Drop();
        haiw.transform.parent = null;

        Vector3 startPos = haiw.transform.position;
        float start = Time.time;

        while (Time.time - start < depositAnimTime)
        {
            float t = (Time.time - start) / depositAnimTime;

            Vector3 pos = startPos + (depositAnimTarget.position - startPos) * depositAnim.Evaluate(t);

            haiw.transform.position = pos;

            yield return new WaitForEndOfFrame();
        }

        chargeLevel += chargePerHaiw;
        NetworkServer.Destroy(haiw.gameObject);
    }
}
