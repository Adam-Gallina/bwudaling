using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class LevelUI : GameUI
{
    [Header("Player Stats")]
    [SerializeField] private TMPro.TMP_Text levelText;
    [SerializeField] private GameObject talentsBackground;
    [SerializeField] private TMPro.TMP_Text talentsText;
    [SerializeField] private Slider xpSlider;
    [SerializeField] private TMPro.TMP_Text xpText;

    [Header("Player Upgrades")]
    [SerializeField] private TMPro.TMP_Text speedText;
    [SerializeField] private TMPro.TMP_Text boostSpeedText;
    [SerializeField] private TMPro.TMP_Text boostMaxText;
    [SerializeField] private TMPro.TMP_Text boostRechargeText;
    public Button special1Btn;
    [HideInInspector] public string special1Name;
    public Button special2Btn;
    [HideInInspector] public string special2Name;
    public Button special3Btn;
    [HideInInspector] public string special3Name;
    [SerializeField] private RectTransform tooltipObj;
    [SerializeField] private TMPro.TMP_Text tooltipText;
    [SerializeField] private GameObject tooltipUpgradesObj;
    [SerializeField] private TMPro.TMP_Text tooltipUpgradesText;
    [SerializeField] private Sprite unlockSprite;
    [SerializeField] private Sprite upgradeSprite;

    [Header("Player Skills")]
    public AbilityCooldown special1Cooldown;
    public TooltipController special1Tooltip;
    public AbilityCooldown special2Cooldown;
    public TooltipController special2Tooltip;
    public AbilityCooldown special3Cooldown;
    public TooltipController special3Tooltip;

    [Header("Banner")]
    [SerializeField] private GameObject bannerParent;
    [SerializeField] private TMPro.TMP_Text bannerText;
    [SerializeField] private TMPro.TMP_Text bannerTextBkgd;

    [Header("Healthbar")]
    [SerializeField] private GameObject healthbarParent;
    [SerializeField] private TMPro.TMP_Text healthbarName;
    [SerializeField] private TMPro.TMP_Text healthbarNameOutline;
    [SerializeField] private Slider healthbar;
    private BossBase healthbarTarget;

    [Header("Pause Menu")]
    [SerializeField] private GameObject hostMenu;
    [SerializeField] private GameObject clientMenu;

    private NetworkPlayer activePlayer;

    protected override void Start()
    {
        base.Start();

        NetworkPlayer p = BwudalingNetworkManager.Instance.ActivePlayer;
        if (p != null)
        {
            activePlayer = p;
            activePlayer.abilities.OnAddXp += UpdateDisplay;
            activePlayer.abilities.OnLevelUp += UpdateDisplay;
            activePlayer.abilities.OnUpgrade += UpdateDisplay;
        }
    }

    private void OnEnable()
    {
        if (activePlayer)
        {
            activePlayer.abilities.OnAddXp += UpdateDisplay;
            activePlayer.abilities.OnLevelUp += UpdateDisplay;
            activePlayer.abilities.OnUpgrade += UpdateDisplay;
        }
    }

    private void OnDisable()
    {
        if (activePlayer)
        {
            activePlayer.abilities.OnAddXp -= UpdateDisplay;
            activePlayer.abilities.OnLevelUp -= UpdateDisplay;
            activePlayer.abilities.OnUpgrade -= UpdateDisplay;
        }
    }

    private bool lastTarget = false, updated;
    private void Update()
    {
        if (activePlayer.avatar)
        {
            //shieldText.text = "Shield: " + activePlayer.avatar.shield;
            PlayerUI.Instance.SetStaminaBar(activePlayer.avatar.boost < activePlayer.abilities.BoostMaxVal);
            PlayerUI.Instance.SetStaminaBarFillAmount(activePlayer.avatar.boost / activePlayer.abilities.BoostMaxVal);
        }

        if (healthbarTarget)
        {
            SetHealthbarFill(healthbarTarget.currHealth / healthbarTarget.maxHealth);
        }

        if (InputController.Instance)
        {
            updated = lastTarget == InputController.Instance.UsingTargetKeys;
            if (!updated)
            {
                lastTarget = InputController.Instance.UsingTargetKeys;
                special1Cooldown.UpdateKeySet(lastTarget);
                special2Cooldown.UpdateKeySet(lastTarget);
                special3Cooldown.UpdateKeySet(lastTarget);
            }
        }
    }

    public override void UpdateDisplay()
    {
        if (activePlayer.avatar)
            levelText.text = "Lvl " + (activePlayer.abilities.level + 1) + " " + activePlayer.avatar.AvatarName;
        talentsBackground.SetActive(activePlayer.abilities.talentPoints > 0);
        talentsText.text =  activePlayer.abilities.talentPoints + " Smakins";
        xpSlider.value = (float)activePlayer.abilities.currXp / activePlayer.abilities.nextXp;
        xpText.text = activePlayer.abilities.currXp + " / " + activePlayer.abilities.nextXp;

        speedText.text = "Speed: lvl " + activePlayer.abilities.speedLevel;
        boostSpeedText.text = "Boost: lvl " + activePlayer.abilities.boostSpeedLevel;
        boostMaxText.text = "Duration: lvl " + activePlayer.abilities.boostMaxLevel;
        //boostRechargeText.text = "Recharge: lvl " + activePlayer.abilities.boostRechargeLevel;

        string LevelText(int level)
        {
            if (level < AbilityLevels.SpecialAbilityMax)
                return level > -1 ? "lvl " + (level + 1) : "";
            return "lvl Max";
        }

        special1Cooldown.gameObject.SetActive(activePlayer.abilities.special1Level > -1);
        ((Image)special1Btn.targetGraphic).sprite = activePlayer.abilities.special1Level > -1 ? upgradeSprite : unlockSprite;
        special1Tooltip.tooltipLevel = LevelText(activePlayer.abilities.special1Level);
        special1Btn.interactable = activePlayer.abilities.special1Level < AbilityLevels.SpecialAbilityMax;

        special2Cooldown.gameObject.SetActive(activePlayer.abilities.special2Level > -1);
        ((Image)special2Btn.targetGraphic).sprite = activePlayer.abilities.special2Level > -1 ? upgradeSprite : unlockSprite;
        special2Tooltip.tooltipLevel = LevelText(activePlayer.abilities.special2Level);
        special2Btn.interactable = activePlayer.abilities.special2Level < AbilityLevels.SpecialAbilityMax;

        special3Cooldown.gameObject.SetActive(activePlayer.abilities.special3Level > -1);
        ((Image)special3Btn.targetGraphic).sprite = activePlayer.abilities.special3Level > -1 ? upgradeSprite : unlockSprite;
        special3Tooltip.tooltipLevel = LevelText(activePlayer.abilities.special3Level);
        special3Btn.interactable = activePlayer.abilities.special3Level < AbilityLevels.SpecialAbilityMax;
    }

    public void ShowTooltip(Vector3 pos, string title, string text, string[] upgrades = null)
    {
        tooltipObj.gameObject.SetActive(true);
        tooltipObj.position = pos;
        tooltipText.text = title + "\n" + text;

        if (upgrades != null && upgrades.Length > 0)
        {
            tooltipUpgradesObj.SetActive(true);
            tooltipUpgradesText.text = "Upgrades:";
            foreach (string s in upgrades)
                tooltipUpgradesText.text += "\n" + s;
        }
        else
            tooltipUpgradesObj.SetActive(false);
    }
    public void ShowTooltip(Vector3 pos, string text, string[] upgrades = null)
    {
        tooltipObj.gameObject.SetActive(true);
        tooltipObj.position = pos;
        tooltipText.text = text;

        if (upgrades != null && upgrades.Length > 0)
        {
            tooltipUpgradesObj.SetActive(true);
            tooltipUpgradesText.text = "Upgrades:";
            foreach (string s in upgrades)
                tooltipUpgradesText.text += "\n" + s;
        }
        else
            tooltipUpgradesObj.SetActive(false);
    }
    public void HideTooltip()
    {
        tooltipObj.gameObject.SetActive(false);
    }

    #region Banner
    protected Coroutine currBanner;
    public override void SetBannerText(string text, Color col, float duration = 0)
    {
        bannerParent.SetActive(!string.IsNullOrEmpty(text));

        bannerText.text = text;
        bannerText.color = col;
        bannerTextBkgd.text = text;

        if (duration > 0)
        {
            if (currBanner != null)
                StopCoroutine(currBanner);
            currBanner = StartCoroutine(HideBanner(duration));
        }
    }

    private IEnumerator HideBanner(float duration)
    {
        yield return new WaitForSeconds(duration);
        SetBannerText(string.Empty);
    }
    #endregion

    #region Healthbar
    public override void SetBossHealthTarget(BossBase boss)
    {
        healthbarTarget = boss;
        healthbarParent.SetActive(true);
        healthbarName.text = boss.name.Remove(boss.name.Length - 7);
        healthbarNameOutline.text = boss.name.Remove(boss.name.Length - 7);
    }

    private void SetHealthbarFill(float amount)
    {
        healthbar.value = amount;
    }
    #endregion

    #region Buttons
    public void UpgradeSpeed()
    {
        activePlayer.abilities.UpgradeAbility(AbilityType.speed);
    }
    public void UpgradeBoostMax()
    {
        activePlayer.abilities.UpgradeAbility(AbilityType.boostMax);
    }
    public void UpgradeBoostSpeed()
    {
        activePlayer.abilities.UpgradeAbility(AbilityType.boostSpeed);
    }
    public void UpgradeBoostRecharge()
    {
        activePlayer.abilities.UpgradeAbility(AbilityType.boostRecharge);
    }
    public void UpgradeSpecial1()
    {
        activePlayer.abilities.UpgradeAbility(AbilityType.special1);
    }
    public void UpgradeSpecial2()
    {
        activePlayer.abilities.UpgradeAbility(AbilityType.special2);
    }
    public void UpgradeSpecial3()
    {
        activePlayer.abilities.UpgradeAbility(AbilityType.special3);
    }

    #endregion
}
