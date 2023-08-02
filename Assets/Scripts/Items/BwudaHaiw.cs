using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BwudaHaiw : ItemBase
{
    [SerializeField] private float followDist;

    [Header("Animation")]
    [SerializeField] private Transform model;
    [SerializeField] private float bobSpeed;
    [SerializeField] private float bobMaxDelta;
    private float currBob;
    private float bobDir = 1;

    [Header("Map Icon")]
    [SerializeField] private Transform icon;
    [SerializeField] private float iconTurnSpeed;
    [SerializeField] private float iconTurnMaxDelta;
    private float currIconRot;
    private int iconDir = 1;

    [ServerCallback]
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        if (other.gameObject.layer == Constants.GroundLayer)
        {
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    private void Update()
    {
        if (isClient)
        {
            currBob += bobDir * bobSpeed * Time.deltaTime;
            if (currBob < -bobMaxDelta || currBob > bobMaxDelta)
            {
                bobDir *= -1;
                currBob = Mathf.Clamp(currBob, -bobMaxDelta, bobMaxDelta);
            }
            model.transform.localPosition = new Vector3(0, currBob, 0);

            currIconRot += iconDir * iconTurnSpeed * Time.deltaTime;
            if (currIconRot < -iconTurnMaxDelta || currIconRot > iconTurnMaxDelta)
            {
                iconDir *= -1;
                currIconRot = Mathf.Clamp(currIconRot, -iconTurnMaxDelta, iconTurnMaxDelta);
            }
            icon.localEulerAngles = new Vector3(90, currIconRot, 0);
        }
        
        if (isServer)
        {
            if (targetPlayer)
                transform.position = targetPlayer.transform.position + (transform.position - targetPlayer.transform.position).normalized * followDist;
        }
    }

    [Server]
    protected override void OnCollect(PlayerAvatar p)
    {
        base.OnCollect(p);

        p.RpcStatsHaiwCollected();
    }
}
