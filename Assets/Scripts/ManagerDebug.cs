using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerDebug : MonoBehaviour
{
    public static ManagerDebug Instance;

    public NetworkManager kcpManager;
    public GameObject kcpOptions;

    public NetworkManager steamManager;
    public bool DEBUG_useKcpManager = true;

    void Awake()
    {
        Instance = this;

        if (DEBUG_useKcpManager)
        {
            kcpManager.gameObject.SetActive(true);
            kcpOptions.SetActive(true);
        }
        else
        {
            steamManager.gameObject.SetActive(true);
            kcpOptions.gameObject.SetActive(false);
        }
    }
}
