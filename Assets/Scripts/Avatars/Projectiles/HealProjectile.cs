using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class HealProjectile : RicochetProjectile
{
    [SerializeField] private RangeF SpinSpeed;
    private float spinSpeed;

    [SerializeField] private Transform rotTarget;

    private void Start()
    {
        spinSpeed = SpinSpeed.RandomVal * Mathf.Sign(Random.Range(-1, 1));
    }

    protected override void Update()
    {
        base.Update();

        rotTarget.localEulerAngles = new Vector3(0, rotTarget.localEulerAngles.y + spinSpeed * Time.deltaTime, 0);
    }

    [Server]
    protected override void OnHitTarget(Collider other)
    {
        PlayerAvatar player = other.GetComponentInParent<PlayerAvatar>();

        if (player && player.dead)
        {
            player.PlayerHeal((PlayerAvatar)spawner);
            ((PlayerAvatar)spawner).RpcPlayHealAudio(player);
            base.OnHitTarget(other);
        }
    }
}
