using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;

    [SerializeField] protected NametagUI nametagPrefab;
    [SerializeField] protected Transform nametagParent;
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

    public void ToggleOptionsMenu()
    {
        OptionsMenu.Instance.ToggleOptionsMenu();
    }
}
