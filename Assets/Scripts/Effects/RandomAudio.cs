using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomAudio : MonoBehaviour
{
    [SerializeField] private AudioClip[] clips;

    [SerializeField] private bool PlayOnAwake = true;

    private void Start()
    {
        if (PlayOnAwake)
            Play();
    }

    public void Play()
    {
        GetComponent<AudioSource>().clip = clips[Random.Range(0, clips.Length)];
        GetComponent<AudioSource>().Play();
    }
}
