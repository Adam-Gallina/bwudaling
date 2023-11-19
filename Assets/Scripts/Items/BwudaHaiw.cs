using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Unity.VisualScripting;

public class BwudaHaiw : ItemBase
{
    [SerializeField] private float followDist;

    [Header("Animation")]
    [SerializeField] private Transform model;
    [SerializeField] private float bobSpeed;
    [SerializeField] private float rotSpeed;
    [SerializeField] private float bobMaxDelta;
    private float currBob;
    private float bobDir = 1;
    private float currRot;

    [Header("Map Icon")]
    [SerializeField] private Transform icon;
    [SerializeField] private float iconTurnSpeed;
    [SerializeField] private float iconTurnMaxDelta;
    private float currIconRot;
    private int iconDir = 1;

    [Header("Audio")]
    [SerializeField] private AudioClip grabClip;
    [SerializeField] private AudioClip depositClip;
    private AudioSource source;

    private void Awake()
    {
        currRot = Random.Range(0, 359);

        source = GetComponent<AudioSource>();
    }

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
            currRot += rotSpeed * Time.deltaTime;
            if (currBob < -bobMaxDelta || currBob > bobMaxDelta)
            {
                bobDir *= -1;
                currBob = Mathf.Clamp(currBob, -bobMaxDelta, bobMaxDelta);
            }
            model.transform.localPosition = new Vector3(0, currBob, 0);
            model.transform.localEulerAngles = new Vector3(0, currRot, 0);

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

        RpcPlayAudio(p, 0);
    }

    [Server]
    public override void Drop()
    {
        if (targetPlayer)
            RpcPlayAudio(targetPlayer, 1);

        base.Drop();
    }

    [ClientRpc]
    private void RpcPlayAudio(PlayerAvatar p, int clip) 
    { 
        if (BwudalingNetworkManager.Instance.ActivePlayer.avatar == p)
        {
            switch (clip)
            {
                case -1:
                    source.Stop();
                    return;
                case 0:
                    source.clip = grabClip;
                    break;
                case 1:
                    source.clip = depositClip;
                    break;
                default:
                    Debug.LogError("Could not play requested clip");
                    return;
            }

            source.Play();
        }
    }
}
