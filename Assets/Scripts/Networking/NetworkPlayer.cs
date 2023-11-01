using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class NetworkPlayer : NetworkBehaviour
{
    [SyncVar]
    public AvatarClass gameAvatarClass;
    [SyncVar]
    [HideInInspector] public int avatarLevel;
    [SyncVar]
    public PlayerAvatar avatar;
    protected NametagUI currNametag;
    [SyncVar]
    public PlayerStatValues currPlayerStats;

    [SyncVar(hook = nameof(OnDisplayNameChanged))]
    public string displayName = "Unnamed Player";
    [SyncVar(hook = nameof(OnAvatarColorChanged))]
    public Color avatarColor = Color.white;
    [SyncVar(hook = nameof(OnAvatarShirtChanged))]
    public string shirtTextureId = AchievmentController.DefaultShirtId;
    [SyncVar(hook = nameof(OnReadyChanged))]
    public bool IsReady = false;

    public bool IsLeader;

    public Abilities abilities;

    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);

        BwudalingNetworkManager.Instance.Players.Add(this);

        SpawnGameUI();
    }

    public override void OnStartAuthority()
    {
        SetAvatarColor(ColorSelect.Instance.GetNextAvailableColor());
    }

    [Client]
    public void SetAvatar(AvatarClass avatar)
    {
        abilities = AbilityLevels.LoadAbilities(avatar.ToString());
        abilities.OnAddXp += SaveAbilities;
        abilities.OnLevelUp += SaveAbilities;
        abilities.OnUpgrade += SaveAbilities;

        CmdSetAvatar(avatar, abilities.level);
    }

    [Command]
    private void CmdSetAvatar(AvatarClass avatar, int level)
    {
        gameAvatarClass = avatar;
        avatarLevel = level;
    }

    [Client]
    public void SetAvatarColor(Color col)
    {
        if (hasAuthority)
            CmdSetAvatarColor(col);
    }

    [Command]
    private void CmdSetAvatarColor(Color col)
    {
        avatarColor = col;
    }

    [Client]
    public void SetAvatarShirt(string shirtId)
    {
        if (hasAuthority)
            CmdSetShirtColor(shirtId);
    }
    [Command]
    private void CmdSetShirtColor(string shirtId)
    {
        shirtTextureId = shirtId;
    }

    [Client]
    private void SaveAbilities()
    {
        if (abilities == null)
            return;

        AbilityLevels.SaveAbilities(gameAvatarClass.ToString(), abilities);
    }

    public override void OnStopClient()
    {
        BwudalingNetworkManager.Instance.Players.Remove(this);

        if (currNametag)
        {
            Destroy(currNametag.gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += SpawnGameUI;

        if (abilities != null)
        {
            abilities.OnAddXp += SaveAbilities;
            abilities.OnLevelUp += SaveAbilities;
            abilities.OnUpgrade += SaveAbilities;
        }
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= SpawnGameUI;

        if (abilities != null)
        {
            abilities.OnAddXp -= SaveAbilities;
            abilities.OnLevelUp -= SaveAbilities;
            abilities.OnUpgrade -= SaveAbilities;
        }
    }

    private void SpawnGameUI(Scene oldScene, Scene newScene)
    {
        SpawnGameUI();
    }
    private void SpawnGameUI()
    {
        StartCoroutine(WaitForMapController());
    }

    private IEnumerator WaitForMapController()
    {
        if (!GameUI.Instance)
            yield return new WaitUntil(() => { return GameUI.Instance && GameUI.Instance.start; });

        currNametag = GameUI.Instance?.SpawnNametag();
        currNametag?.SetLinkedPlayer(this);
    }

    #region Getters/Setters
    [Command]
    public void CmdSetDisplayName(string displayName)
    {
        this.displayName = displayName;
    }
    
    public bool CanReady()
    {
        return gameAvatarClass != AvatarClass.None;
    }
    [Command]
    public void CmdSetIsReady(bool isReady)
    {
        if (!CanReady())
            return;

        IsReady = isReady;

        BwudalingNetworkManager.Instance.NotifyPlayersOfReadyState();
    }

    private void OnDisplayNameChanged(string _, string newval)
    {
        OnPlayerInfoChanged();
    }
    private void OnAvatarColorChanged(Color o, Color n)
    {
        ColorSelect.Instance?.PlayerSelectedColor(o, n);
        OnPlayerInfoChanged();
    }
    private void OnAvatarShirtChanged(string _, string newval)
    {
        OnPlayerInfoChanged();
    }
    private void OnReadyChanged(bool _, bool newval)
    {
        OnPlayerInfoChanged();
    }

    private void OnPlayerInfoChanged()
    {
        GameUI.Instance.UpdateDisplay();
    }

    [ClientRpc] public void RpcGetPlayerStats() 
    { 
        if (!hasAuthority) return;
        CmdSetPlayerStats(GetComponent<PlayerStats>().currStats); 
    }
    [Command] public void CmdSetPlayerStats(PlayerStatValues stats)
    {
        currPlayerStats = stats;
    }
    #endregion

    #region Avatar
    [ClientRpc]
    public void RpcOnAvatarSpawned(GameObject avatarObj)
    {
        avatar = avatarObj.GetComponent<PlayerAvatar>();

        //if (!hasAuthority) return;

        avatar.name = displayName;

        avatar.SetNetworkPlayer(this);
    }
    #endregion
}