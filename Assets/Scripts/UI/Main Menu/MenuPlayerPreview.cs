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

    [SerializeField] private SkinnedMeshRenderer shirt;

    [SerializeField] private float spinSpeed = 180;

    private Animator anim;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        //anim.SetBool("Walking", true);
        //anim.SetBool("Dancing", true);
        //anim.SetInteger("Dance", 1);
    }

    float rot = 0;
    private void Update()
    {
        rot += spinSpeed * Time.deltaTime;
        transform.GetChild(0).localEulerAngles = Vector3.up * rot;
    }

    public void SetClass(AvatarClass c)
    {
        switch (c)
        {
            case AvatarClass.Wuva:
                //wuvaHat.SetActive(true); 
                dogieHat.SetActive(false);
                piestHat.SetActive(false);
                bwudaHat.SetActive(false);
                break;
            case AvatarClass.Dogie:
                //wuvaHat.SetActive(false); 
                dogieHat.SetActive(true);
                piestHat.SetActive(false);
                bwudaHat.SetActive(false);
                break;
            case AvatarClass.Piest:
                //wuvaHat.SetActive(false); 
                dogieHat.SetActive(false);
                piestHat.SetActive(true);
                bwudaHat.SetActive(false);
                break;
            case AvatarClass.Bwuda:
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

    public void SetMaterial(Material mat)
    {
        shirt.material = mat;
    }
}
