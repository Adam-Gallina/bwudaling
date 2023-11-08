using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
    public static BackgroundMusic Instance;

    [SerializeField] private AudioClip startClip;

    [SerializeField] private AudioClip[] music;

    private AudioSource source;

    private void Awake()
    {
        Instance = this;

        source = GetComponent<AudioSource>();
    }

    public void PlayStartClip()
    {
        source.clip = startClip;
        source.Play();
    }

    public void Play()
    {
        Debug.Log("Background music started...");
    }

    public void Stop()
    {

    }
}
