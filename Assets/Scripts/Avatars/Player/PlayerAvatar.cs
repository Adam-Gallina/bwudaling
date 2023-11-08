using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class PlayerAvatar : AvatarBase
{
    [SyncVar]
    protected NetworkPlayer player;
    [HideInInspector] public PlayerStats stats;

    public Transform NametagTarget;
    public string AvatarName;

    [SerializeField] protected SkinnedMeshRenderer body;
    [SerializeField] protected SkinnedMeshRenderer shirt;
    [SyncVar(hook = nameof(OnBodyColChanged))]
    protected Color bodyCol;
    [SyncVar(hook = nameof(OnShirtChanged))]
    protected string shirtId;

    [Header("Movement")]
    private bool useMouse = false;
    protected Vector3 targetPos;
    [HideInInspector] public float boost;
    private bool boostRecharging = false;
    [SerializeField] protected float turnSpeed = 360;
    [SerializeField] protected float turnSpeedMod;
    [HideInInspector] [SyncVar] public float currSpeed;

    //[Header("Items")]
    [HideInInspector] public ItemBase heldItem;

    [Header("Specials")]
    [SerializeField] protected AbilityBase ability1;
    [SerializeField] protected AbilityBase ability2;
    [SerializeField] protected AbilityBase ability3;
    [SerializeField] protected Transform projectileSpawn;


    //[Header("Debuffs")]
    protected Transform dragTarget;
    protected float dragEnd;
    protected float dragSpeed;
    protected float currSpeedMod;
    protected float speedDebuffEnd;

    [Header("Map Icons")]
    [SerializeField] private GameObject aliveIcon;
    [SerializeField] private GameObject deadIcon;

    [Header("Animations")]
    [SerializeField] private float startWalkAnimSpeed;
    [SerializeField] private float stepWalkAnimSpeed;

    [Header("Effects")]
    [SerializeField] private ParticleSystem splatSystem;
    [SerializeField] private ParticleSystem healedSystem;
    [SerializeField] private AudioSource shieldBreak;
    [SerializeField] private ParticleSystem shatterSystem;
    [SerializeField] private AudioSource deathAudio;
    [SerializeField] private float timeBeforeIdleAnim;
    private float idleStart;
    [SerializeField] private ParticleSystem levelUpSystem;
    [SerializeField] private AudioSource levelUpAudio;
    [SerializeField] private RandomAudio healAudio;

    private List<Collider> currSafeZones = new List<Collider>();

    protected InputController inp;
    protected Animator anim;

    #region Getters/Setters
    public void SetNetworkPlayer(NetworkPlayer player)
    {
        this.player = player;

        if (!player.hasAuthority)
            return;

        stats = player.GetComponent<PlayerStats>();

        CmdSetBodyColor(player.avatarColor);
        CmdSetShirtId(player.shirtTextureId);

        boost = player.abilities.BoostMaxVal;

        GameUI.Instance?.UpdateDisplay();

        player.abilities.OnLevelUp += CmdOnLevelUp;
    }

    [Command]
    private void CmdSetBodyColor(Color col) { bodyCol = col; }
    private void OnBodyColChanged(Color _, Color col)
    {
        body.material.color = col;
        aliveIcon.GetComponent<SpriteRenderer>().color = col;
        deadIcon.GetComponent<SpriteRenderer>().color = col;
    }
    [Command]
    private void CmdSetShirtId(string id) { shirtId = id; }
    private void OnShirtChanged(string _, string id)
    {
        if (AchievmentController.Shirts.ContainsKey(id))
            shirt.material = AchievmentController.Shirts[id].mat;
    }
    #endregion

    protected override void Awake()
    {
        base.Awake();

        anim = GetComponentInChildren<Animator>();

        ability1?.SetController(this);
        ability2?.SetController(this);
        ability3?.SetController(this);
    }

    public override void OnStartAuthority()
    {
        inp = gameObject.AddComponent<InputController>();

        CameraController.Instance.SetTarget(transform);
        PlayerUI.Instance.SetTarget(transform);

        ability1?.LinkUI(((LevelUI)GameUI.Instance).special1Cooldown, ((LevelUI)GameUI.Instance).special1Tooltip);
        ((LevelUI)GameUI.Instance).special1Name = ability1.abilityName;
        ability2?.LinkUI(((LevelUI)GameUI.Instance).special2Cooldown, ((LevelUI)GameUI.Instance).special2Tooltip);
        ((LevelUI)GameUI.Instance).special2Name = ability2.abilityName;
        ability3?.LinkUI(((LevelUI)GameUI.Instance).special3Cooldown, ((LevelUI)GameUI.Instance).special3Tooltip);
        ((LevelUI)GameUI.Instance).special3Name = ability3.abilityName;

        idleStart = Time.time;
    }

    protected override void OnShieldChanged(int _, int shield)
    {
        base.OnShieldChanged(_, shield);

        if (_ > 0 && shield == 0)
        {
            shatterSystem.Play();

            if (hasAuthority)
                shieldBreak.Play();
        }
    }

    [ClientRpc]
    public void RpcStatsHealPlayer() { if (hasAuthority) stats?.AddRescue(); }
    [Command]
    private void CmdPlayHealAudio(PlayerAvatar target)
    {
        RpcPlayHealAudio(target);
    }
    [ClientRpc]
    private void RpcPlayHealAudio(PlayerAvatar target)
    {
        if (BwudalingNetworkManager.Instance.ActivePlayer.avatar == target)
        {
            healAudio.Play();
        }
    }

    [ClientRpc]
    public void RpcStatsHaiwCollected() { if (hasAuthority) stats?.AddHaiw(); }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.layer)
        {
            case Constants.PlayerLayer:
                PlayerAvatar o = other.GetComponent<PlayerAvatar>();
                if (o && o.dead)
                {
                    o.Heal();
                    healAudio.Play();
                    CmdPlayHealAudio(o);
                    RpcStatsHealPlayer();
                }
                break;
            case Constants.SafeAreaLayer:
                currSafeZones.Add(other);
                canDamage = false;
                break;
        }
    }

    [ServerCallback]
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == Constants.SafeAreaLayer)
        {
            currSafeZones.Remove(other);
            canDamage = currSafeZones.Count == 0;
        }
    }

    protected virtual void Update()
    {
        aliveIcon.SetActive(!dead);
        aliveIcon.transform.eulerAngles = Vector3.right * 90;
        deadIcon.SetActive(dead);
        deadIcon.transform.eulerAngles = Vector3.right * 90;

        if (!hasAuthority)
            return;

        CheckInput();

        ability1?.UpdateUI(player.abilities.special1Level);
        ability2?.UpdateUI(player.abilities.special2Level);
        ability3?.UpdateUI(player.abilities.special3Level);

        if (dead)
        {
            if (Time.time < dragEnd)
                transform.Translate(dragSpeed * Time.deltaTime * (dragTarget.position - transform.position).normalized, Space.World);
            return;
        }

        if (!inp.boost || boostRecharging)
            boost += player.abilities.BoostRechargeVal * Time.deltaTime;
        if (boost > player.abilities.BoostMaxVal)
            boost = player.abilities.BoostMaxVal;
        if (boostRecharging && inp.boost.up)
            boostRecharging = false;
        anim.SetBool("Running", (inp.boost.down || inp.boost.held) && !boostRecharging);

        targetPos = GetMovement();
        Vector3 dir = targetPos - transform.position;
        dir.y = 0;
        if (dir != Vector3.zero)
            projectileSpawn.forward = dir;

        rb.velocity = (targetPos - transform.position).normalized * CalcSpeed();
        if (Vector3.Distance(transform.position, targetPos) < rb.velocity.magnitude * Time.deltaTime)
        {
            rb.velocity = Vector3.zero;
            rb.MovePosition(targetPos);
        }
        
        UpdateCurrSpeed(rb.velocity.magnitude > 0 ? rb.velocity.magnitude : CalcSpeed());

        bool walking = rb.velocity.magnitude > 0;
        if (walking)
        {
            Vector3 newVec = targetPos - transform.position;
            newVec.y = 0;
            transform.forward = Vector3.RotateTowards(transform.forward, newVec.normalized, (turnSpeed + player.abilities.speedLevel * turnSpeedMod) * Mathf.Deg2Rad * Time.deltaTime, 0);
            anim.SetBool("Dancing", false);
        }
        anim?.SetBool("Walking", walking);
    }

    private void UpdateCurrSpeed(float newSpeed)
    {
        if (newSpeed == currSpeed) return;
        CmdUpdateCurrSpeed(newSpeed);
    }
    [Command] private void CmdUpdateCurrSpeed(float newSpeed)
    {
        currSpeed = newSpeed;
    }

    #region Controls
    protected virtual void CheckInput()
    {
        if (BwudalingNetworkManager.Instance.DEBUG_AllowKeyCheats)
        {
            if (Input.GetKeyDown(KeyCode.Equals))
            {
                player.abilities.talentPoints += AbilityLevels.TalentPointsPerLevel;
                GameUI.Instance.UpdateDisplay();
            }
            if (Input.GetKeyDown(KeyCode.Minus))
            {
                player.abilities.AddXp(5);
            }
            if (dead && Input.GetKeyDown(KeyCode.Backspace))
            {
                CmdHealTarget(this, 1);
            }
        }

        if (!dead && inp.dance1.down)
        {
            anim.SetBool("Dancing", true);
            anim.SetInteger("Dance", 1);
            idleStart = Time.time;
        }

        if (inp.special1.down && (!dead || ability1.canUseWhileDead))
        {
            ability1.QueueAbility(player.abilities.special1Level);
            ability2.CancelAbility();
            ability3.CancelAbility();
            anim.SetBool("Dancing", false);
            idleStart = Time.time;
        }
        if (inp.special2.down && (!dead || ability2.canUseWhileDead))
        {
            ability1.CancelAbility();
            ability2.QueueAbility(player.abilities.special2Level);
            ability3.CancelAbility();
            anim.SetBool("Dancing", false);
            idleStart = Time.time;
        }
        if (inp.special3.down && (!dead || ability3.canUseWhileDead))
        {
            ability1.CancelAbility();
            ability2.CancelAbility();
            ability3.QueueAbility(player.abilities.special3Level);
            anim.SetBool("Dancing", false);
            idleStart = Time.time;
        }

        anim.SetBool("Idling", Time.time >= idleStart + timeBeforeIdleAnim);
    }

    protected bool CheckSpecial(float timer)
    {
        return Time.time >= timer;
    }

    private Vector3 GetMovement()
    {
        if (inp.left || inp.right || inp.up || inp.down)
            useMouse = false;

        if ((inp.altfire.down || inp.altfire.held))
        {
            useMouse = true;
            Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100, 1 << Constants.GroundLayer);
            idleStart = Time.time;
            return new Vector3(hit.point.x, 0, hit.point.z);
        }
        else if (useMouse)
            return targetPos;

        Vector3 dir = Vector3.zero;

        if (inp.left)
            dir.x -= 1;
        if (inp.right)
            dir.x += 1;

        if (inp.down)
            dir.z -= 1;
        if (inp.up)
            dir.z += 1;

        if (dir != Vector3.zero)
            idleStart = Time.time;

        return transform.position + dir;
    }

    protected virtual float CalcSpeed()
    {
        if (currSpeedMod != 1 && Time.time > speedDebuffEnd)
            currSpeedMod = 1;

        if (inp.boost && !boostRecharging && boost > 0)
        {
            boost -= Time.deltaTime;
            if (boost <= 0)
            {
                boostRecharging = true;
                boost = 0;
            }
        }

        anim.SetFloat("WalkSpeed", startWalkAnimSpeed + player.abilities.speedLevel * stepWalkAnimSpeed);

        float boostMod = 1;
        if (inp.boost && !boostRecharging && boost > 0)
            boostMod = player.abilities.BoostSpeedVal;

        return player.abilities.SpeedVal * boostMod * currSpeedMod;
    }

    public void SetPosition(Vector3 pos, bool updateTargetPos = false)
    {
        transform.position = pos;
        if (updateTargetPos)
            targetPos = transform.position;
    }
    #endregion

    protected override void OnHeal()
    {
        if (anim?.GetBool("Dead") == true)
            healedSystem?.Play();
        anim?.SetBool("Dead", false);
        idleStart = Time.time;
    }

    protected override void OnDeath()
    {
        if (hasAuthority)
            stats?.AddDeath();

        targetPos = transform.position;
        rb.velocity = Vector3.zero;

        ability1.CancelAbility();
        ability2.CancelAbility();
        ability3.CancelAbility();

        if (anim?.GetBool("Dead") == false)
        {
            splatSystem?.Play();

            if (hasAuthority)
            {
                deathAudio.volume = 1;
                deathAudio?.Play();
            }
            else if (GetComponentInChildren<Renderer>().isVisible)
            {
                deathAudio.volume = .75f;
                deathAudio?.Play();
            }
        }
        anim?.SetBool("Dead", true);

        if (isServer)
            GameController.Instance.RpcSendServerBannerMessage($"{player.displayName}{Constants.RandomDeathPhrase}", player.avatarColor, Constants.DeathBannerDuration);

    }

    #region Network Commands
    [Command]
    public void CmdHealTarget(PlayerAvatar target)
    {
        target.Heal();
    }
    [Command]
    public void CmdHealTarget(PlayerAvatar target, int shield)
    {
        target.SetShield(shield);
        target.Heal();
    }

    [ClientRpc]
    public void RpcAddXp(int xp)
    {
        if (!hasAuthority)
            return;
        player.abilities.AddXp(xp);
    }
    [Command]
    private void CmdOnLevelUp() { RpcOnLevelUp(); }
    [ClientRpc]
    private void RpcOnLevelUp()
    {
        if (hasAuthority)
            levelUpAudio.Play();
        levelUpSystem.Play();
    }


    [Server]
    public void DragEffect(Transform target, float duration, float speed)
    {
        RpcDragEffect(target, duration, speed);
    }

    [ClientRpc]
    private void RpcDragEffect(Transform target, float duration, float speed)
    {
        if (!hasAuthority)
            return;

        dragTarget = target;
        dragEnd = Time.time + duration;
        dragSpeed = speed;
    }

    [Command]
    public void CmdApplySpeedMod(float newSpeedMod, float duration)
    {
        RpcApplySpeedMod(newSpeedMod, duration);
    }
    [ClientRpc]
    private void RpcApplySpeedMod(float newSpeedMod, float duration)
    {
        if (!hasAuthority) return;

        speedDebuffEnd = Time.time + duration;

        if (newSpeedMod > currSpeedMod)
            currSpeedMod = newSpeedMod;
    }
    #endregion

    #region Abilities
    public void UseAbility(AbilityBase ability, Vector3 target, int level)
    {
        if (ability == ability1)
            CmdUseAbility(0, target, level);
        else if (ability == ability2)
            CmdUseAbility(1, target, level);
        else if (ability == ability3)
            CmdUseAbility(2, target, level);
        else
            Debug.LogError("Can't use ability " + ability.name, this);
    }

    [Command]
    private void CmdUseAbility(int ability, Vector3 target, int level)
    {
        switch (ability)
        {
            case 0:
                ability1.OnUseServerAbility(target, level);
                break;
            case 1:
                ability2.OnUseServerAbility(target, level);
                break;
            case 2:
                ability3.OnUseServerAbility(target, level);
                break;
        }

        RpcUseAbility(ability, target, level);
    }

    [ClientRpc]
    private void RpcUseAbility(int ability, Vector3 target, int level)
    {
        switch (ability)
        {
            case 0:
                ability1.OnUseClientAbility(target, level);
                break;
            case 1:
                ability2.OnUseClientAbility(target, level);
                break;
            case 2:
                ability3.OnUseClientAbility(target, level);
                break;
        }
    }

    public void DoEffect(AbilityBase ability, Vector3 target, int level)
    {
        if (ability == ability1)
            CmdDoAbilityEffect(0, target, level);
        else if (ability == ability2)
            CmdDoAbilityEffect(1, target, level);
        else if (ability == ability3)
            CmdDoAbilityEffect(2, target, level);
        else
            Debug.LogError("Can't use ability effect " + ability.name, this);
    }
    [Command]
    private void CmdDoAbilityEffect(int ability, Vector3 target, int level)
    {
        RpcDoAbilityEffect(ability, target, level);
    }
    [ClientRpc]
    private void RpcDoAbilityEffect(int ability, Vector3 target, int level)
    {
        switch (ability)
        {
            case 0:
                ability1.OnDoClientEffect(target, level);
                break;
            case 1:
                ability2.OnDoClientEffect(target, level);
                break;
            case 2:
                ability3.OnDoClientEffect(target, level);
                break;
        }
    }

    public void DoAbilityAudio(AbilityBase ability)
    {
        if (ability == ability1)
            CmdDoAbilityAudio(0);
        else if (ability == ability2)
            CmdDoAbilityAudio(1);
        else if (ability == ability3)
            CmdDoAbilityAudio(2);
        else
            Debug.LogError("Can't use ability audio " + ability.name, this);
    }
    [Command]
    private void CmdDoAbilityAudio(int ability)
    {
        RpcDoAbilityAudio(ability);
    }
    [ClientRpc]
    private void RpcDoAbilityAudio(int ability)
    {
        switch (ability)
        {
            case 0:
                ability1.OnDoClientAudio();
                break;
            case 1:
                ability2.OnDoClientAudio();
                break;
            case 2:
                ability3.OnDoClientAudio();
                break;
        }
    }
    #endregion
}