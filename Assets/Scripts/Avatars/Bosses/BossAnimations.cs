using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAnimations : MonoBehaviour
{
    [SerializeField] private ParticleSystem system1;
    [SerializeField] private ParticleSystem system2;
    [SerializeField] private ParticleSystem system3;
    [SerializeField] private GameObject obj1;

    public static event Action Callback1;

    public void StartSystem1() {
        if (!system1)
            return;
        system1.Play(); 
        system1.gameObject.SetActive(true); 
    }
    public void StopSystem1() {
        if (!system1)
            return;
        system1.Stop(); 
        //system1.gameObject.SetActive(false); 
    }

    public void StartSystem2() {
        if (!system2)
            return;
        system2.Play();
        system2.gameObject.SetActive(true); 
    }
    public void StopSystem2() {
        if (!system2)
            return;
        system2.Stop();
        //system2.gameObject.SetActive(false); 
    }

    public void StartSystem3()
    {
        if (!system3)
            return;
        system3.Play();
        system3.gameObject.SetActive(true);
    }
    public void StopSystem3()
    {
        if (!system3)
            return;
        system3.Stop();
        //system3.gameObject.SetActive(false); 
    }

    public void ShowObj1()
    {
        if (!obj1)
            return;
        obj1.SetActive(true);
    }
    public void HideObj1()
    {
        if (!obj1)
            return;
        obj1.SetActive(false);
    }

    public void SetCallback1()
    {
        Callback1?.Invoke();
    }
}
