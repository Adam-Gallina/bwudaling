using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class MenuPlayerPreview : MonoBehaviour
{
    //[SerializeField] private string wuvaHatName;
    [SerializeField] private string dogieHatName = "Hat_Dogie";
    [SerializeField] private string piestHatName = "Hat_Piest";
    [SerializeField] private string bwudaHatName = "Hat_Bwuda";

    [SerializeField] private SkinnedMeshRenderer shirtBody;
    [SerializeField] private SkinnedMeshRenderer shirt;

    [SerializeField] private SkinnedMeshRenderer danceBody;
    [SerializeField] private SkinnedMeshRenderer danceShirt;

    [SerializeField] private float spinSpeed = 180;

    private Animator anim;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        //anim.SetBool("Walking", true);
        anim.SetBool("Dancing", true);
        anim.SetInteger("Dance", 1);

        Debug.Log(shirtBody.transform.parent);
        Debug.Log(shirtBody.transform.parent.Find(dogieHatName));
    }

    float rot = 0;
    private void Update()
    {
        rot += spinSpeed * Time.deltaTime;
        transform.GetChild(0).localEulerAngles = Vector3.up * rot;
    }

    public void SetClass(AvatarClass c)
    {
        foreach (Transform t in new Transform[] { shirtBody.transform.parent, danceBody.transform.parent }) {
            switch (c)
            {
                case AvatarClass.Wuva:
                    //t.Find(wuvaHatName).gameObject.SetActive(true);
                    t.Find(dogieHatName).gameObject.SetActive(false);
                    t.Find(piestHatName).gameObject.SetActive(false);
                    t.Find(bwudaHatName).gameObject.SetActive(false);
                    break;
                case AvatarClass.Dogie:
                    //t.Find(wuvaHatName).gameObject.SetActive(false); 
                    t.Find(dogieHatName).gameObject.SetActive(true);
                    t.Find(piestHatName).gameObject.SetActive(false);
                    t.Find(bwudaHatName).gameObject.SetActive(false);
                    break;
                case AvatarClass.Piest:
                    //t.Find(wuvaHatName).gameObject.SetActive(false); 
                    t.Find(dogieHatName).gameObject.SetActive(false);
                    t.Find(piestHatName).gameObject.SetActive(true);
                    t.Find(bwudaHatName).gameObject.SetActive(false);
                    break;
                case AvatarClass.Bwuda:
                    //t.Find(wuvaHatName).gameObject.SetActive(false); 
                    t.Find(dogieHatName).gameObject.SetActive(false);
                    t.Find(piestHatName).gameObject.SetActive(false);
                    t.Find(bwudaHatName).gameObject.SetActive(true);
                    break;
            }
        }
    }

    public void SetColor(Color col)
    {
        shirtBody.material.color = col;
        danceBody.material.color = col;
    }

    public void SetMaterial(Material mat)
    {
        shirt.material = mat;
        danceShirt.material = mat;
    }


    public void SetDance(int dance)
    {
        anim.SetInteger("Dance", dance);
    }
}
