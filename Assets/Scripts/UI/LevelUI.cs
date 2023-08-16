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
    public TMPro.TMP_Text special1Text;
    [HideInInspector] public string special1Name;
    public TMPro.TMP_Text special2Text;
    [HideInInspector] public string special2Name;
    public TMPro.TMP_Text special3Text;
    [HideInInspector] public string special3Name;
    [SerializeField] private RectTransform tooltipObj;
    [SerializeField] private TMPro.TMP_Text tooltipText;
    [SerializeField] private GameObject tooltipUpgradesObj;
    [SerializeField] private TMPro.TMP_Text tooltipUpgradesText;

    [Header("Player Skills")]
    //[SerializeField] private TMPro.TMP_Text shieldText;
    //[SerializeField] private Slider boostSlider;
    public AbilityCooldown special1Cooldown;
    public AbilityCooldown special2Cooldown;
    public AbilityCooldown special3Cooldown;

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
    [SerializeField] private GameObject restartBtn;

    private NetworkPlayer activePlayer;

    private void Start()
    {
        restartBtn.SetActive(false);

        NetworkPlayer p = BwudalingNetworkManager.Instance.ActivePlayer;
        if (p != null)
        {
            activePlayer = p;
            activePlayer.abilities.OnAddXp += UpdateDisplay;
            activePlayer.abilities.OnLevelUp += UpdateDisplay;
            activePlayer.abilities.OnUpgrade += UpdateDisplay;

            restartBtn.SetActive(p.IsLeader);
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

    private bool lastTarget = true, updated;
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
        levelText.text = "Lvl " + (activePlayer.abilities.level + 1);
        talentsBackground.SetActive(activePlayer.abilities.talentPoints > 0);
        talentsText.text = "Talent Points: " + activePlayer.abilities.talentPoints;
        xpSlider.value = (float)activePlayer.abilities.currXp / activePlayer.abilities.nextXp;
        xpText.text = activePlayer.abilities.currXp + " / " + activePlayer.abilities.nextXp;

        speedText.text = "Speed\nlvl " + activePlayer.abilities.speedLevel;
        boostSpeedText.text = "Boost\nlvl " + activePlayer.abilities.boostSpeedLevel;
        boostMaxText.text = "Duration\nlvl " + activePlayer.abilities.boostMaxLevel;
        boostRechargeText.text = "Recharge\nlvl " + activePlayer.abilities.boostRechargeLevel;

        string SpecialText(string name, int level) {
            if (level == -1)
                return $"Unlock {name}";
            else if (level == AbilityLevels.SpecialAbilityMax)
                return $"{name}: max";
            return $"{name}: lvl {level + 1}";
        }

        special1Cooldown.gameObject.SetActive(activePlayer.abilities.special1Level > -1);
        special1Text.text = SpecialText(special1Name, activePlayer.abilities.special1Level);

        special2Cooldown.gameObject.SetActive(activePlayer.abilities.special2Level > -1);
        special2Text.text = SpecialText(special2Name, activePlayer.abilities.special2Level);

        special3Cooldown.gameObject.SetActive(activePlayer.abilities.special3Level > -1);
        special3Text.text = SpecialText(special3Name, activePlayer.abilities.special3Level);
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

    public void RestartLevel()
    {
        BwudalingNetworkManager.Instance.RestartMap();
    }
    #endregion
}
