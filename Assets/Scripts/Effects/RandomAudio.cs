using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomAudio : MonoBehaviour
{
    [SerializeField] private AudioClip[] clips;

    [SerializeField] private bool PlayOnAwake = true;

    [SerializeField] private float requiredDistToPlayer;

    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (PlayOnAwake)
            Play();
    }

    public void Play()
    {
        float dist = Vector3.Distance(transform.position, BwudalingNetworkManager.Instance.ActivePlayer.avatar.transform.position);
        if (clips.Length > 0 && dist < requiredDistToPlayer)
        {
            source.clip = clips[Random.Range(0, clips.Length)];
            source.Play();
        }
    }

}
