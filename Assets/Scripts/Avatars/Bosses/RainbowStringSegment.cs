using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainbowStringSegment : MonoBehaviour
{
    [SerializeField] private float spinSpeed = 120;

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == Constants.PlayerLayer)
        {
            PlayerAvatar p = other.gameObject.GetComponent<PlayerAvatar>();
            if (p && !BwudalingNetworkManager.Instance.DEBUG_Invulnerable)
                p.Damage();
        }
    }

    private void Update()
    {;
        transform.localEulerAngles += Vector3.up * spinSpeed * Time.deltaTime;
    }
}
