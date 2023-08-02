using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Dogie : PlayerAvatar
{
    [SerializeField] private Transform bulletSpawn;

    [Header("Fetch")]
    [SerializeField] private Projectile fetchBullet;
    [SerializeField] private float fetchBulletSpeed = 7.5f;
    [SerializeField] private AbilityUpgrade fetchDuration;
    [SerializeField] private AbilityUpgrade fetchSpeed;
    private PlayerAvatar fetchTarget;
    private float fetchEnd;
    //This is dumb. Find a better way
    private float fetchDur;
    private float fetchSpe;


    // Fetch
    protected  void DoSpecial1(int level)
    {
        //if (CheckSpecial(nextSpecial1) || BwudalingNetworkManager.Instance.DEBUG_IgnoreCooldown)
        {
            stats?.AddAbility();

            //nextSpecial1 = Time.time + special1Cooldown;

            CmdSetFetchVals(level);
            CmdSpawnFetchBullet(level);
        }
    }

    [Command]
    private void CmdSpawnFetchBullet(int level)
    {
        //FireProjectiles(fetchBullet, bulletSpawn.position, bulletSpawn.eulerAngles, 1, 0, level, OnSpawnFetchBullet);

        RpcOnFireFetchBullet();
    }

    [Command]
    private void CmdSetFetchVals(int level)
    {
        fetchDur = fetchDuration.CalcValue(level);
        fetchSpe = fetchSpeed.CalcValue(level);
    }

    [Server]
    private void OnSpawnFetchBullet(Projectile b, int level)
    {
        b.SetSpeed(fetchBulletSpeed);
        b.SetSource(this, 1 << Constants.PlayerLayer);
    }

    [ClientRpc]
    private void RpcOnFireFetchBullet()
    {

    }

    [Server]
    public void SetFetchTarget(PlayerAvatar target)
    {
        target.DragEffect(transform, fetchDur, fetchSpe);
    }

    // Dig
}
