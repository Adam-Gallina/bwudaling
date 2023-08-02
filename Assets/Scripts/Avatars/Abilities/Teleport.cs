using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class Teleport : TargetAbility
{
    protected override void OnUseAbility(int level)
    {
        if (!BwudalingNetworkManager.Instance.DEBUG_TpWalls)
        {
            Collider[] colls = Physics.OverlapSphere(currReticlePos, 0.1f, 1 << Constants.TeleportAreaLayer);
            if (colls.Length == 0)
                return;
            foreach (Collider c in colls)
            {
                TeleportArea tp = c.GetComponent<TeleportArea>();
                if (tp && !tp.validTp)
                    return;
            }
        }

        controller.stats?.AddAbility();

        controller.SetPosition(currReticlePos, true);
    }
}
