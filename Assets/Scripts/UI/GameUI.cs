using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;

    [SerializeField] protected NametagUI nametagPrefab;
    [SerializeField] private Transform nametagParent;
    [SerializeField] private Color defaultBannerCol;

    [HideInInspector] public bool start = false;

    protected virtual void Awake()
    {
        Instance = this;
    }

    protected virtual void Start()
    {
        start = true;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public virtual void UpdateDisplay()
    {

    }

    public virtual NametagUI SpawnNametag()
    {
        if (!nametagPrefab)
            return null;
        return Instantiate(nametagPrefab, nametagParent);
    }

    public virtual void AddNametag(NametagUI nametag)
    {

    }

    public virtual void RemoveNametag(NametagUI nametag)
    {

    }

    public void SetBannerText(string text, float duration = 0) { SetBannerText(text, defaultBannerCol, duration); }
    public virtual void SetBannerText(string text, Color col, float duration = 0)
    {
        Debug.LogWarning($"No Banner set up (text: {text})");
    }

    public virtual void SetBossHealthTarget(BossBase boss)
    {
        Debug.LogWarning($"No Healthbar set up (boss: {boss.name})");
    }

    #region Button Callbacks
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
    #endregion
}
