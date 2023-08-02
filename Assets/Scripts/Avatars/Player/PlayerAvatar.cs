using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public abstract class PlayerAvatar : AvatarBase
{
    [SyncVar]
    protected NetworkPlayer player;
    protected PlayerStats stats;

    public Transform NametagTarget;
    public string AvatarName;

    [SerializeField] protected SkinnedMeshRenderer body;
    [SyncVar(hook = nameof(OnBodyColChanged))]
    protected Color bodyCol;

    [Header("Movement")]
    [SerializeField] private float distToTarget = 0.1f;
    private bool useMouse = false;
    protected Vector3 targetPos;
    [HideInInspector] public float boost;
    [SerializeField] protected float turnSpeed = 360;
    [SerializeField] protected float turnSpeedMod;

    //[Header("Items")]
    [HideInInspector] public ItemBase heldItem;

    [Header("Specials")]
    [SerializeField] private string special1Name;
    [SerializeField] private string special1Tooltip;
    [SerializeField] protected float special1Cooldown;
    [SerializeField] private Sprite special1Icon;
    protected float nextSpecial1 = 0;
    [SerializeField] private string special2Name;
    [SerializeField] private string special2Tooltip;
    [SerializeField] protected float special2Cooldown;
    [SerializeField] private Sprite special2Icon;
    protected float nextSpecial2 = 0;
    [SerializeField] private string special3Name;
    [SerializeField] private string special3Tooltip;
    [SerializeField] protected float special3Cooldown;
    [SerializeField] private Sprite special3Icon;
    protected float nextSpecial3 = 0;
    protected bool showingIndicator = false;
    protected Vector3 currReticlePos;

    //[Header("Debuffs")]
    protected Transform dragTarget;
    protected float dragEnd;
    protected float dragSpeed;

    [Header("Map Icons")]
    [SerializeField] private GameObject aliveIcon;
    [SerializeField] private GameObject deadIcon;

    [Header("Animations")]
    [SerializeField] private float startWalkAnimSpeed;
    [SerializeField] private float stepWalkAnimSpeed;

    protected InputController inp;
    protected Animator anim;
    protected TargetReticle reticle;

    #region Getters/Setters
    public void SetNetworkPlayer(NetworkPlayer player)
    {
        this.player = player;

        if (!player.hasAuthority)
            return;

        stats = player.GetComponent<PlayerStats>();

        CmdSetBodyColor(player.avatarColor);

        player.abilities.special1Name = special1Name;
        player.abilities.special1Image = special1Icon;
        player.abilities.special1Tooltip = special1Tooltip;
        player.abilities.special2Name = special2Name;
        player.abilities.special2Image = special2Icon;
        player.abilities.special2Tooltip = special2Tooltip;
        player.abilities.special3Name = special3Name;
        player.abilities.special3Image = special3Icon;
        player.abilities.special3Tooltip = special3Tooltip;

        boost = player.abilities.BoostMaxVal;

        GameUI.Instance?.UpdateDisplay();
    }

    [Command]
    private void CmdSetBodyColor(Color col) { bodyCol = col; }
    private void OnBodyColChanged(Color _, Color col)
    {
        body.material.color = col;
        aliveIcon.GetComponent<SpriteRenderer>().color = col;
        deadIcon.GetComponent<SpriteRenderer>().color = col;
    }
    #endregion

    protected override void Awake()
    {
        base.Awake();

        anim = GetComponentInChildren<Animator>();
        reticle = GetComponentInChildren<TargetReticle>();
    }

    public override void OnStartAuthority()
    {
        inp = gameObject.AddComponent<InputController>();

        CameraController.Instance.SetTarget(transform);
    }

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
                    RpcStatsHealPlayer();
                }
                break;
            case Constants.SafeAreaLayer:
                canDamage = false;
                break;
        }
    }

    [ClientRpc]
    protected void RpcStatsHealPlayer() { if (hasAuthority) stats?.AddRescue(); }

    [ClientRpc]
    public void RpcStatsHaiwCollected() { if (hasAuthority) stats?.AddHaiw(); }

    [ServerCallback]
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == Constants.SafeAreaLayer)
            canDamage = true;
    }

    protected virtual void Update()
    {
        aliveIcon.SetActive(!dead);
        deadIcon.SetActive(dead);

        if (!hasAuthority)
            return;

        if (dead && Input.GetKeyDown(KeyCode.Backspace) && BwudalingNetworkManager.Instance.DEBUG_AllowKeyCheats)
        {
            CmdHealTarget(this, 1);
        }

        if (dead)
        {
            if (Time.time < dragEnd)
            {
                transform.Translate(dragSpeed * Time.deltaTime * (dragTarget.position - transform.position).normalized, Space.World);
            }

            return;
        }

        CheckInput();

        ((LevelUI)GameUI.Instance).special1Cooldown.SetCooldown(nextSpecial1 - Time.time, special1Cooldown);
        ((LevelUI)GameUI.Instance).special2Cooldown.SetCooldown(nextSpecial2 - Time.time, special2Cooldown);
        ((LevelUI)GameUI.Instance).special3Cooldown.SetCooldown(nextSpecial3 - Time.time, special3Cooldown);

        if (!inp.boost)
            boost += player.abilities.BoostRechargeVal * Time.deltaTime;
        if (boost > player.abilities.BoostMaxVal)
            boost = player.abilities.BoostMaxVal;

        targetPos = GetMovement();

        //transform.position = Vector3.MoveTowards(transform.position, targetPos, CalcSpeed() * Time.deltaTime);
        //bool walking = Vector3.Distance(transform.position, targetPos) > distToTarget;

        rb.velocity = (targetPos - transform.position).normalized * CalcSpeed();
        if (Vector3.Distance(transform.position, targetPos) < rb.velocity.magnitude * Time.deltaTime)
        {
            rb.velocity = Vector3.zero;
            rb.MovePosition(targetPos);
        }

        bool walking = rb.velocity.magnitude > 0;
        if (walking)
        {
            Vector3 newVec = targetPos - transform.position;
            newVec.y = 0;
            transform.forward = Vector3.RotateTowards(transform.forward, newVec.normalized, (turnSpeed + player.abilities.speedLevel * turnSpeedMod) * Mathf.Deg2Rad * Time.deltaTime, 0);
        }
        anim?.SetBool("Walking", walking);
    }

    #region Controls
    protected virtual void CheckInput()
    {
        if (inp.special1.down && player.abilities.special1Level != -1)
            DoSpecial1(player.abilities.special1Level);

        if (inp.special2.down && player.abilities.special2Level != -1)
            DoSpecial2(player.abilities.special2Level);

        if (inp.special3.down && player.abilities.special3Level != -1)
            DoSpecial3(player.abilities.special3Level);

        if (BwudalingNetworkManager.Instance.DEBUG_AllowKeyCheats)
        {
            if (Input.GetKeyDown(KeyCode.Equals)) {
                player.abilities.talentPoints += AbilityLevels.TalentPointsPerLevel;
                GameUI.Instance.UpdateDisplay();
            }
            if (Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                Debug.Log($"{player.abilities.special1Level} {player.abilities.special2Level} {player.abilities.special3Level}");
            }
        }

    }

    protected bool CheckSpecial(float timer)
    {
        return Time.time >= timer;
    }

    protected abstract void DoSpecial1(int level);
    protected abstract void DoSpecial2(int level);
    protected abstract void DoSpecial3(int level);

    private Vector3 GetMovement()
    {
        if (inp.left || inp.right || inp.up || inp.down)
            useMouse = false;

        if ((inp.altfire.down || inp.altfire.held) && !showingIndicator)
        {
            useMouse = true;
            Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100, 1 << Constants.GroundLayer);
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

        return transform.position + dir;
    }

    protected virtual float CalcSpeed()
    {
        if (inp.boost && boost > 0)
            boost -= Time.deltaTime;

        anim.SetFloat("WalkSpeed", startWalkAnimSpeed + player.abilities.speedLevel * stepWalkAnimSpeed);

        return player.abilities.SpeedVal * (inp.boost && boost > 0 ? player.abilities.BoostSpeedVal : 1);
    }
    #endregion

    protected override void OnHeal()
    {
        anim?.SetBool("Dead", false);
    }

    protected override void OnDeath()
    {
        if (hasAuthority)
            stats?.AddDeath();

        targetPos = transform.position;
        rb.velocity = Vector3.zero;

        anim?.SetBool("Dead", true);

        if (isServer)
            GameController.Instance.RpcSendServerBannerMessage($"{player.displayName} {Constants.RandomDeathPhrase}", Constants.DeathBannerDuration);

    }

    #region Network Commands
    [Command]
    protected void CmdHealTarget(PlayerAvatar target)
    {
        target.Heal();
    }
    [Command]
    protected void CmdHealTarget(PlayerAvatar target, int shield)
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
    #endregion

    #region Helper Functions
    protected bool CheckShowIndicator()
    {
        if (inp.altfire.up)
            return false;

        return true;
    }
    protected void ShowIndicator(bool show, float size, float range, Action useCallback)
    {
        showingIndicator = show;
        reticle.SetReticle(show);
        if (!show)
            return;

        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100, 1 << Constants.GroundLayer);
        Vector3 toTarget = new Vector3(hit.point.x, 0, hit.point.z) - transform.position;
        if (range > 0 && toTarget.magnitude > range)
            toTarget = toTarget.normalized * range;

        currReticlePos = transform.position + toTarget;
        reticle.SetReticleCenter(currReticlePos);
        reticle.DrawCircle(size);

        if (inp.fire)
        {
            useCallback?.Invoke();
            reticle.SetReticle(false);
        }
    }

    [Server]
    protected void FireProjectiles(Projectile prefab, Vector3 spawnPos, Vector3 spawnRot, int count, float spread, int level, Action<Projectile, int> spawnCallback)
    {
        float startAng = count > 1 ? -spread / 2 : 0;
        float angStep = count > 1 ? spread / (count - 1) : 0;

        for (int i = 0; i < count; i++)
        {
            Projectile b = Instantiate(prefab, spawnPos, Quaternion.Euler(new Vector3(0, spawnRot.y + startAng + angStep * i, 0)));
            NetworkServer.Spawn(b.gameObject);

            spawnCallback?.Invoke(b, level);
        }
    }
    #endregion
}