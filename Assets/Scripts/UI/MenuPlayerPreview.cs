using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPlayerPreview : MonoBehaviour
{
    //[SerializeField] private GameObject wuvaHat;
    [SerializeField] private GameObject dogieHat;
    [SerializeField] private GameObject piestHat;
    [SerializeField] private GameObject bwudaHat;

    [SerializeField] private SkinnedMeshRenderer body;

    private Animator anim;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        anim.SetBool("Dancing", true);
        anim.SetInteger("Dance", 1);
    }

    public void SetClass(int c)
    {
        switch (c)
        {
            case 0:
                //wuvaHat.SetActive(true); 
                dogieHat.SetActive(false);
                piestHat.SetActive(false);
                bwudaHat.SetActive(false);
                break;
            case 1:
                //wuvaHat.SetActive(false); 
                dogieHat.SetActive(true);
                piestHat.SetActive(false);
                bwudaHat.SetActive(false);
                break;
            case 2:
                //wuvaHat.SetActive(false); 
                dogieHat.SetActive(false);
                piestHat.SetActive(true);
                bwudaHat.SetActive(false);
                break;
            case 3:
                //wuvaHat.SetActive(false); 
                dogieHat.SetActive(false);
                piestHat.SetActive(false);
                bwudaHat.SetActive(true);
                break;
        }
    }

    public void SetColor(Color col)
    {
        body.material.color = col;
    }
}
