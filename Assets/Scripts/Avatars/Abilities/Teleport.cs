using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using Mirror;

public class Teleport : TargetAbility
{
    protected override bool OnUseAbility(int level)
    {
        Collider[] colls = Physics.OverlapSphere(currReticlePos, 0.1f, 1 << Constants.EnvironmentLayer);
        if (colls.Length > 0)
            return false;

        if (!BwudalingNetworkManager.Instance.DEBUG_TpWalls)
        {
            colls = Physics.OverlapSphere(currReticlePos, 0.1f, 1 << Constants.TeleportAreaLayer);
            if (colls.Length == 0)
                return false;
            foreach (Collider c in colls)
            {
                TeleportArea tp = c.GetComponent<TeleportArea>();
                if (tp && !tp.validTp)
                    return false;
            }
        }

        controller.stats?.AddAbility();

        DoServerEffect(transform.position, level);
        controller.SetPosition(currReticlePos, true);

        return true;
    }

    protected override void PlaceEffect(ParticleSystem effect, Vector3 target, int level)
    {
        ParticleSystem e = Instantiate(effect, target, Quaternion.identity);
        base.PlaceEffect(e, target, level);
    }
}
