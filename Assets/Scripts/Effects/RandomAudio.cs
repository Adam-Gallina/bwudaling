using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomAudio : MonoBehaviour
{
    [SerializeField] private AudioClip[] clips;

    [SerializeField] private bool PlayOnAwake = true;

    [SerializeField] private float requiredDistToPlayer;

    [SerializeField] private AudioSource source;
    [SerializeField] private AudioSource nonPlayerSource;

    private void Awake()
    {
        if (source == null)
            source = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (PlayOnAwake)
            Play();

        if (nonPlayerSource != null)
        {
            NetworkBehaviour behaviour = GetComponentInParent<NetworkBehaviour>();
            if (!behaviour.hasAuthority)
                source = nonPlayerSource;
        }
    }

    public void Play()
    {
        float dist = requiredDistToPlayer > 0 ? Vector3.Distance(transform.position, BwudalingNetworkManager.Instance.ActivePlayer.avatar.transform.position) : -1;
        if (clips.Length > 0 && dist < requiredDistToPlayer)
        {
            source.clip = clips[Random.Range(0, clips.Length)];
            source.Play();
        }
    }
}
