using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class HaiwArea : SafeArea
{
    [HideInInspector] public float chargeLevel = 0;
    [SerializeField] protected float damageSpeed;
    [SerializeField] protected float chargePerHaiw;

    [SerializeField] protected int deliveryXp = 25;

    [SerializeField] protected BossDamageAnim damageAnim;

    [ServerCallback]
    private void Update()
    {
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
                chargeLevel += chargePerHaiw;

                NetworkServer.Destroy(target.heldItem.gameObject);
                target.heldItem = null;

                target.RpcAddXp(MapController.Instance.safeZoneXp);
            }
        }
    }
}
