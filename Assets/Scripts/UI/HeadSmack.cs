using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadSmack : MonoBehaviour
{
    public void PlaySmack() { GetComponent<AudioSource>().Play(); }
}
