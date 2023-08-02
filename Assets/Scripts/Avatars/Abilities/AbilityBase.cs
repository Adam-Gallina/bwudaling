using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityBase : MonoBehaviour
{
    protected AvatarBase controller;

    [SerializeField] protected string aname;

    private int val;

    public void SetController(AvatarBase controller)
    {
        this.controller = controller;
        Debug.Log(controller.name+ ": " + aname);
    }

    public  void SetVal(int val)
    {
        this.val = val;
    }

    public void PrintVal() { Debug.Log(val); }
}
