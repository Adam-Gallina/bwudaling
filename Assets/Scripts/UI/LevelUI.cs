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
    public AbilityUI ability1;
    public AbilityUI ability2;
    public AbilityUI ability3;

    [SerializeField] private RectTransform tooltipObj;
    [SerializeField] private TMPro.TMP_Text tooltipText;
    [SerializeField] private GameObject tooltipUpgradesObj;
    [SerializeField] private TMPro.TMP_Text tooltipUpgradesText;

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
            PlayerUI.Instance.SetStaminaBar(activePlayer.avatar.boost < activePlayer.abilities.vals.BoostMaxVal);
            PlayerUI.Instance.SetStaminaBarFillAmount(activePlayer.avatar.boost / activePlayer.abilities.vals.BoostMaxVal);
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
                ability1.cooldown.UpdateKeySet(lastTarget);
                ability2.cooldown.UpdateKeySet(lastTarget);
                ability3.cooldown.UpdateKeySet(lastTarget);
            }
        }
    }

    public override void UpdateDisplay()
    {
        if (activePlayer.avatar)
            levelText.text = "Lvl " + (activePlayer.abilities.vals.level + 1) + " " + activePlayer.avatar.AvatarName;
        talentsBackground.SetActive(activePlayer.abilities.vals.talentPoints > 0);
        talentsText.text =  activePlayer.abilities.vals.talentPoints + " Smakins";
        xpSlider.value = (float)activePlayer.abilities.vals.currXp / activePlayer.abilities.vals.nextXp;
        xpText.text = activePlayer.abilities.vals.currXp + " / " + activePlayer.abilities.vals.nextXp;

        speedText.text = "Speed: lvl " + activePlayer.abilities.vals.speedLevel;
        boostSpeedText.text = "Boost: lvl " + activePlayer.abilities.vals.boostSpeedLevel;
        boostMaxText.text = "Duration: lvl " + activePlayer.abilities.vals.boostMaxLevel;
        //boostRechargeText.text = "Recharge: lvl " + activePlayer.abilities.boostRechargeLevel;

        ability1.UpdateUI(activePlayer.abilities.vals.special1Level);
        ability2.UpdateUI(activePlayer.abilities.vals.special2Level);
        ability3.UpdateUI(activePlayer.abilities.vals.special3Level);
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

[System.Serializable]
public class AbilityUI
{
    public Button button;
    [HideInInspector] public string abilityName;
    public GameObject unlockImg;
    public GameObject upgradeImg;
    public TMPro.TMP_Text levelText;
    public AbilityCooldown cooldown;
    public TooltipController tooltip;

    public void UpdateUI(int abilityLevel)
    {
        string TooltipLevelText(int level)
        {
            if (level < AbilityLevels.SpecialAbilityMax)
                return level > -1 ? "lvl " + (level + 1) : "";
            return "lvl Max";
        }
        string ButtonLevelText(int level)
        {
            if (level < AbilityLevels.SpecialAbilityMax)
                return level > -1 ? "lvl " + (level + 1) : "Unlock";
            return "Max";
        }

        cooldown.gameObject.SetActive(abilityLevel > -1);
        unlockImg.SetActive(abilityLevel == -1);
        upgradeImg.SetActive(abilityLevel > -1);
        levelText.text = ButtonLevelText(abilityLevel);
        tooltip.tooltipLevel = TooltipLevelText(abilityLevel);
        button.interactable = abilityLevel < AbilityLevels.SpecialAbilityMax;
    }
}