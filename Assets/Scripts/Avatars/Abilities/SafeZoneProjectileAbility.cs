using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeZoneProjectileAbility : ProjectileSpawnAbility
{
    [Header("Safe Zone Projectile")]
    [SerializeField] private AbilityUpgrade zoneDiameter;
    [SerializeField] private AbilityUpgrade zoneDuration;
    [SerializeField] private float projectileDespawnTime = 1;
    [SerializeField] private float projectileDespawnOffset = -10;

    [Server]
    protected override void OnSpawnProjectile(Projectile b, int level)
    {
        base.OnSpawnProjectile(b, level);

        float s = zoneDiameter.CalcValue(level);
        b.transform.localScale = new Vector3(s, s, s);

        StartCoroutine(DespawnProjectile(b.gameObject, level));
    }

    [Server]
    private IEnumerator DespawnProjectile(GameObject b, int level)
    {
        yield return new WaitForSeconds(zoneDuration.CalcValue(level));

        float start = Time.time;
        float startOffset = b.transform.GetChild(0).localPosition.y;
        while (Time.time < start + projectileDespawnTime)
        {
            float t = (Time.time - start) / projectileDespawnTime;
            b.transform.GetChild(0).localPosition = new Vector3(0, startOffset + projectileDespawnOffset * t, 0);

            yield return new WaitForEndOfFrame();
        }

        // Allow collisions to update
        b.transform.position = new Vector3(0, -100, 0);
        yield return new WaitForEndOfFrame();
        NetworkServer.Destroy(b);
    }


    protected override float CalcNextAbility(int level)
    {
        return base.CalcNextAbility(level) + zoneDuration.CalcValue(level);
    }

    public override void UpdateUI(int level)
    {
        float actualCooldown = abilityCooldown.CalcValue(level);
        cooldownUI.SetCooldown(nextAbility - Time.time, actualCooldown);
    }
}
