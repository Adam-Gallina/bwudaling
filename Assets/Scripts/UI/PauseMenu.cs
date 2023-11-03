using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        anim.SetBool("IsHost", BwudalingNetworkManager.Instance.ActivePlayer.IsLeader);
    }

    public void LeaveLobbyPressed()
    {
        switch (BwudalingNetworkManager.Instance.mode)
        {
            case Mirror.NetworkManagerMode.ClientOnly:
                BwudalingNetworkManager.Instance.StopClient();
                break;
            case Mirror.NetworkManagerMode.Host:
                BwudalingNetworkManager.Instance.StopHost();
                break;
            default:
                Debug.LogError("Idk what happened but probably ur trying to make a server now so that's pretty cool");
                break;
        }
    }
    public void RestartLevel()
    {
        if (BwudalingNetworkManager.Instance.ActivePlayer.IsLeader)
            BwudalingNetworkManager.Instance.RestartMap();
    }
    public void ReturnToLobby()
    {
        if (BwudalingNetworkManager.Instance.ActivePlayer.IsLeader)
            BwudalingNetworkManager.Instance.ReturnToLobby();
    }
}
