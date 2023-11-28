using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerDebug : MonoBehaviour
{
    public static ManagerDebug Instance;

    public GameObject kcpOptions;

    public BwudalingNetworkManager networkManager;

    public Transport kcpTransport;
    public Transport steamTransport;
    public bool DEBUG_useKcpManager = true;

    public Transport transport { get; private set; }

    void Awake()
    {
        kcpOptions.SetActive(DEBUG_useKcpManager);

        if (Instance != null)
        {
            networkManager.transform.parent = null;
            networkManager.gameObject.SetActive(true);

            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        transport = DEBUG_useKcpManager ? kcpTransport : steamTransport;
        transport.gameObject.SetActive(true);

        networkManager.transform.parent = null;
        networkManager.gameObject.SetActive(true);
    }
}
