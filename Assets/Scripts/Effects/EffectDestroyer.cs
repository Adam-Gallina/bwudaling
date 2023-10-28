using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectDestroyer : MonoBehaviour
{
    private bool spawned = false;   

    void Update()
    {
        if (GetComponent<ParticleSystem>().isPlaying)
            spawned = true;
        if (spawned && !GetComponent<ParticleSystem>().isPlaying)
            Destroy(gameObject);
    }
}
