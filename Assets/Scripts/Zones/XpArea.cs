using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using System;

public class XpArea : SafeArea
{
    [SerializeField] protected ParticleSystem reachEffect;
    [SerializeField] protected AudioSource reachAudio;

    protected Dictionary<PlayerAvatar, int> rewarded = new Dictionary<PlayerAvatar, int>();
    [SerializeField] protected float rewardDist = 35;

    [Client]
    protected override void ClientOnTouchPlayer(PlayerAvatar target)
    {
        if (IsNewPlayer(target) && target.hasAuthority)
        {
            reachEffect?.Play();
            reachAudio?.Play();
        }
    }

    [Server]
    protected override void ServerOnTouchPlayer(PlayerAvatar target)
    {
        if (IsNewPlayer(target))
        {
            target.RpcAddXp(MapController.Instance.safeZoneXp);

            List<PlayerAvatar> eligible = new List<PlayerAvatar>();

            // Check for players close to/within safe zone
            Collider[] nearby = Physics.OverlapBox(transform.position, new Vector3(rewardDist, rewardDist, rewardDist) / 2, transform.rotation, 1 << Constants.PlayerLayer);
            foreach (Collider c in nearby)
            {
                PlayerAvatar p = c.GetComponent<PlayerAvatar>();
                if (p == null) continue;

                // Make sure no wall exists between potential helper and safe zone
                if (Physics.Raycast(transform.position + Vector3.up * .25f, c.ClosestPoint(transform.position) - transform.position, Vector3.Distance(transform.position, c.ClosestPoint(transform.position)), 1 << Constants.EnvironmentLayer))
                    continue;

                eligible.Add(p);
            }

            foreach (PlayerAvatar p in eligible)
            {
                ServerAssignReward(p, eligible.Count - 1);
            }

            // Check for players that revived the target
            foreach (PlayerAvatar p in target.GetLastHeals())
            {
                if (eligible.Contains(p)) continue;

                ServerAssignReward(p, 1);
            }
            target.ResetLastHeals();
        }
    }

    [Server]
    private void ServerAssignReward(PlayerAvatar target, int count)
    {
        if (!rewarded.ContainsKey(target))
            rewarded.Add(target, 0);

        int c = Math.Min(MapController.Instance.maxFriendlyReward, count);
        c -= rewarded[target];

        if (c <= 0) return;

        rewarded[target] += c;
        int friendlyReward = Mathf.RoundToInt(MapController.Instance.safeZoneXp * MapController.Instance.friendlyXpMod);
        target.RpcAddXp(friendlyReward * c);
    }
}
