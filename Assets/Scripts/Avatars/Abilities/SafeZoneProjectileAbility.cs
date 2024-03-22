using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeZoneProjectileAbility : ProjectileSpawnAbility
{
    [Header("Safe Zone Projectile")]
    [SerializeField] private AbilityUpgrade zoneDiameter;
    [SerializeField] private AbilityUpgrade zoneDuration;
    [SerializeField] private AbilityUpgrade zoneTravelDist;
    [SerializeField] private float minSpeed;
    [SerializeField] private float projectileDespawnTime = 1;
    [SerializeField] private float projectileDespawnOffset = -10;

    [Server]
    protected override void OnSpawnProjectile(Projectile b, int level)
    {
        base.OnSpawnProjectile(b, level);

        float s = zoneDiameter.CalcValue(level);
        b.transform.localScale = new Vector3(s, b.transform.localScale.y, s);
        Vector3 pos = b.transform.position;
        pos.y = 0;
        b.transform.position = pos;

        StartCoroutine(MoveProjectile(b, level));
    }

    [Server]
    private IEnumerator MoveProjectile(Projectile b, int level)
    {
        float zoneStart = Time.time;
        float calcSpeedMod = controller.currSpeed + speedMod - minSpeed;

        float dist = 0;
        Vector3 lastPos = b.transform.position;
        do {
            dist += Vector3.Distance(b.transform.position, lastPos);

            float t = dist / zoneTravelDist.CalcValue(level);

            b.SetSpeed(minSpeed + calcSpeedMod * (1 - t));

            lastPos = b.transform.position;

            yield return new WaitForEndOfFrame();

        } while (dist < zoneTravelDist.CalcValue(level) && Time.time < zoneStart + zoneDuration.CalcValue(level));

        b.SetSpeed(0);

        yield return new WaitForSeconds(zoneDuration.CalcValue(level) - (Time.time - zoneStart));

        float start = Time.time;
        Vector3 startPos = b.transform.position;
        while (Time.time < start + projectileDespawnTime)
        {
            float t = (Time.time - start) / projectileDespawnTime;
            b.transform.position = startPos + Vector3.up * projectileDespawnOffset * t;

            yield return new WaitForEndOfFrame();
        }

        // Allow collisions to update
        b.transform.position = new Vector3(0, -100, 0);
        yield return new WaitForEndOfFrame();
        NetworkServer.Destroy(b.gameObject);
    }


    protected override float CalcNextAbility(int level)
    {
        return base.CalcNextAbility(level) + zoneDuration.CalcValue(level);
    }

    public override void UpdateUI(int level)
    {
        float next = nextAbility - Time.time;
        float actualCooldown = abilityCooldown.CalcValue(level);

        if (next < actualCooldown)
            cooldownUI.SetCooldown(next, actualCooldown);
        else
            cooldownUI.SetActive(next - actualCooldown);
    }
}
