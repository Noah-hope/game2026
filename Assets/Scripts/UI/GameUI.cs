using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    private const float MainBarWidth = 390f;

    private GameManager gameManager;
    private UpgradeManager upgradeManager;
    private Font font;
    private Canvas gameCanvas;
    private Sprite whiteSprite;

    private Text characterText;
    private Text levelText;
    private Text hpText;
    private Image hpBarFill;
    private Text expText;
    private Image expBarFill;
    private Text attackNameText;
    private Text attackCooldownText;
    private Text attackDamageText;
    private Image attackCooldownOverlay;
    private Text skillNameText;
    private Text skillCooldownText;
    private Text skillDamageText;
    private Image skillCooldownOverlay;
    private Text waveText;
    private Text dangerText;
    private Text killsText;
    private Button invincibleButton;
    private Text invincibleButtonText;
    private Image invincibleButtonImage;

    private GameObject infoPanel;
    private GameObject pauseMenu;
    private GameObject upgradePanel;
    private GameObject gameOverPanel;
    private GameObject victoryPanel;
    private int lastDangerLevel = -1;
    private float dangerNotifyTimer;
    private float hpFlashTimer;

    public void Initialize(GameManager manager, UpgradeManager upgrades)
    {
        gameManager = manager;
        upgradeManager = upgrades;
        font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        whiteSprite = CreateWhiteSprite();
        EnsureEventSystem();
        BuildUI();
        Refresh();
    }

    private void Update()
    {
        if (gameManager == null) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (upgradePanel != null || gameOverPanel != null || victoryPanel != null) return;
            HandlePause();
        }

        Refresh();
    }

    public void Refresh()
    {
        if (characterText == null || gameManager == null || gameManager.PlayerHealth == null || gameManager.PlayerController == null)
        {
            return;
        }

        CharacterStats stats = gameManager.PlayerStats;
        if (stats == null)
        {
            return;
        }

        int totalSeconds = Mathf.FloorToInt(gameManager.SurvivalTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        if (gameManager.IsBossWave)
        {
            waveText.text = "\u7ec8\u6781\u5bf9\u51b3!";
            waveText.color = Mathf.Sin(Time.unscaledTime * 6f) > 0f ? new Color(1f, 0.25f, 0.2f) : new Color(1f, 0.85f, 0.2f);
        }
        else
        {
            waveText.text = "\u6ce2\u6b21  " + gameManager.CurrentWave + " / " + GameManager.TotalWaves;
            waveText.color = new Color(0.95f, 0.85f, 0.35f);
        }
        killsText.text = "\u51fb\u6740  " + gameManager.KillCount;

        int diff = gameManager.DifficultyLevel;
        dangerText.text = "\u5371\u9669 Lv." + diff;
        if (diff == 3) dangerText.color = new Color(1f, 0.25f, 0.2f);
        else if (diff == 2) dangerText.color = new Color(1f, 0.5f, 0.1f);
        else if (diff == 1) dangerText.color = new Color(0.95f, 0.7f, 0.25f);
        else dangerText.color = new Color(0.6f, 0.85f, 0.65f);

        if (diff > lastDangerLevel)
        {
            lastDangerLevel = diff;
            dangerNotifyTimer = 1.5f;
        }
        if (dangerNotifyTimer > 0f)
        {
            dangerNotifyTimer -= Time.unscaledDeltaTime;
            if (dangerNotifyTimer > 0f && Mathf.FloorToInt(dangerNotifyTimer * 10f) % 2 == 0)
            {
                dangerText.color = Color.white;
                dangerText.text = "\u5371\u9669\u7b49\u7ea7\u63d0\u5347!";
            }
        }

        characterText.text = GetCharacterLabel(stats.Type);
        levelText.text = "Lv." + gameManager.Level;

        float hpRatio = Mathf.Clamp01((float)gameManager.PlayerHealth.CurrentHealth / gameManager.PlayerHealth.MaxHealth);
        hpText.text = "\u751f\u547d  " + gameManager.PlayerHealth.CurrentHealth + " / " + gameManager.PlayerHealth.MaxHealth;
        hpBarFill.rectTransform.sizeDelta = new Vector2(MainBarWidth * hpRatio, 0f);

        if (hpRatio < 0.3f)
        {
            hpFlashTimer += Time.unscaledDeltaTime;
            float flash = Mathf.Sin(hpFlashTimer * 8f) * 0.5f + 0.5f;
            hpBarFill.color = new Color(0.95f, 0.12f, 0.12f + flash * 0.55f, 0.95f);
            hpText.color = flash > 0.7f ? new Color(1f, 0.55f + flash * 0.4f, 0.55f + flash * 0.4f) : Color.white;
        }
        else
        {
            hpFlashTimer = 0f;
            hpBarFill.color = new Color(0.9f, 0.12f, 0.12f, 0.95f);
            hpText.color = Color.white;
        }

        float expRatio = Mathf.Clamp01((float)gameManager.CurrentExp / gameManager.NextLevelExp);
        expText.text = "\u7ecf\u9a8c  " + gameManager.CurrentExp + " / " + gameManager.NextLevelExp;
        expBarFill.rectTransform.sizeDelta = new Vector2(MainBarWidth * expRatio, 0f);

        attackNameText.text = stats.AttackName;
        attackDamageText.text = stats.AttackDamage + " \u4f24\u5bb3";
        string atkCdText = gameManager.PlayerController.GetAttackCooldownText();
        bool attackReady = !atkCdText.EndsWith("s");
        attackCooldownText.text = attackReady ? "\u5c31\u7eea" : atkCdText;
        attackCooldownText.color = attackReady ? new Color(0.45f, 1f, 0.45f) : Color.white;
        attackCooldownOverlay.fillAmount = gameManager.PlayerController.GetAttackCooldownRatio();

        skillNameText.text = stats.SkillName;
        skillDamageText.text = stats.SkillDamage + " \u4f24\u5bb3";
        string skillCdText = gameManager.PlayerController.GetSkillCooldownText();
        bool skillReady = !skillCdText.EndsWith("s");
        skillCooldownText.text = skillReady ? "\u5c31\u7eea" : skillCdText;
        skillCooldownText.color = skillReady ? new Color(0.45f, 1f, 0.45f) : Color.white;
        skillCooldownOverlay.fillAmount = gameManager.PlayerController.GetSkillCooldownRatio();

        RefreshInvincibleButton();
    }

    public void ShowUpgradePanel(List<UpgradeOption> options)
    {
        HideUpgradePanel();
        upgradePanel = CreatePanel("Upgrade Panel", new Color(0.015f, 0.02f, 0.045f, 0.93f), new Vector2(860f, 450f));

        Text title = CreateText(upgradePanel.transform, "\u5347\u7ea7", 44, TextAnchor.MiddleCenter);
        title.color = new Color(1f, 0.85f, 0.2f);
        title.fontStyle = FontStyle.Bold;
        AddTextShadow(title, 0.85f);
        SetRect(title.rectTransform, new Vector2(0.5f, 0.90f), new Vector2(0.5f, 0.90f), Vector2.zero, new Vector2(400f, 56f));

        Text chooseText = CreateText(upgradePanel.transform, "\u9009\u62e9\u4e00\u4e2a\u5f3a\u5316", 20, TextAnchor.MiddleCenter);
        chooseText.color = new Color(0.68f, 0.74f, 0.86f);
        SetRect(chooseText.rectTransform, new Vector2(0.5f, 0.81f), new Vector2(0.5f, 0.81f), Vector2.zero, new Vector2(360f, 30f));

        for (int i = 0; i < options.Count; i++)
        {
            float xOffset = (i - 1f) * 246f;
            BuildUpgradeCard(upgradePanel.transform, options[i], xOffset);
        }
    }

    public void HideUpgradePanel()
    {
        if (upgradePanel != null)
        {
            Destroy(upgradePanel);
            upgradePanel = null;
        }
    }

    public void ShowGameOverPanel()
    {
        if (gameOverPanel != null) return;
        gameOverPanel = CreatePanel("Game Over Panel", new Color(0.04f, 0.018f, 0.02f, 0.94f), new Vector2(620f, 500f));
        BuildResultPanel(gameOverPanel.transform, "\u6e38\u620f\u7ed3\u675f", new Color(1f, 0.25f, 0.2f), new Color(0.2f, 0.42f, 0.72f), new Color(0.5f, 0.16f, 0.18f), false);
    }

    public void ShowVictoryPanel()
    {
        if (victoryPanel != null) return;
        victoryPanel = CreatePanel("Victory Panel", new Color(0.02f, 0.035f, 0.075f, 0.94f), new Vector2(620f, 500f));
        BuildResultPanel(victoryPanel.transform, "\u80dc\u5229", new Color(1f, 0.85f, 0.2f), new Color(0.2f, 0.55f, 0.25f), new Color(0.5f, 0.16f, 0.18f), true);
    }

    public void HideVictoryPanel()
    {
        if (victoryPanel != null)
        {
            Destroy(victoryPanel);
            victoryPanel = null;
        }
    }

    private void BuildUI()
    {
        gameCanvas = CreateCanvas("Game Canvas");
        Canvas.ForceUpdateCanvases();

        GameObject topStatusPanel = CreatePanel("Top Status Panel", new Color(0.02f, 0.025f, 0.055f, 0.86f), new Vector2(720f, 64f));
        RectTransform topRect = topStatusPanel.GetComponent<RectTransform>();
        topRect.anchorMin = new Vector2(0.5f, 1f);
        topRect.anchorMax = new Vector2(0.5f, 1f);
        topRect.pivot = new Vector2(0.5f, 1f);
        topRect.anchoredPosition = new Vector2(0f, -10f);

        killsText = CreateText(topStatusPanel.transform, "\u51fb\u6740  0", 20, TextAnchor.MiddleCenter);
        killsText.fontStyle = FontStyle.Bold;
        killsText.color = new Color(0.9f, 0.85f, 0.5f);
        SetRect(killsText.rectTransform, new Vector2(0.13f, 0.5f), new Vector2(0.13f, 0.5f), Vector2.zero, new Vector2(150f, 30f));

        waveText = CreateText(topStatusPanel.transform, "\u6ce2\u6b21  1 / 4", 22, TextAnchor.MiddleCenter);
        waveText.fontStyle = FontStyle.Bold;
        waveText.color = new Color(0.95f, 0.85f, 0.35f);
        SetRect(waveText.rectTransform, new Vector2(0.39f, 0.5f), new Vector2(0.39f, 0.5f), Vector2.zero, new Vector2(270f, 34f));

        dangerText = CreateText(topStatusPanel.transform, "\u5371\u9669 Lv.0", 18, TextAnchor.MiddleCenter);
        dangerText.fontStyle = FontStyle.Bold;
        dangerText.color = new Color(0.95f, 0.55f, 0.25f);
        SetRect(dangerText.rectTransform, new Vector2(0.74f, 0.5f), new Vector2(0.74f, 0.5f), Vector2.zero, new Vector2(210f, 30f));

        CreateDivider(topStatusPanel.transform, 0.25f);
        CreateDivider(topStatusPanel.transform, 0.58f);

        infoPanel = CreatePanel("Info Panel", new Color(0.02f, 0.025f, 0.055f, 0.88f), new Vector2(800f, 168f));
        RectTransform panelRect = infoPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 10f);

        characterText = CreateText(infoPanel.transform, "", 24, TextAnchor.MiddleLeft);
        characterText.fontStyle = FontStyle.Bold;
        characterText.color = new Color(0.95f, 0.85f, 0.4f);
        SetRect(characterText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -18f), new Vector2(150f, 30f));

        levelText = CreateText(infoPanel.transform, "", 18, TextAnchor.MiddleLeft);
        levelText.color = new Color(0.75f, 0.85f, 1f);
        SetRect(levelText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -56f), new Vector2(150f, 24f));

        hpText = CreateText(infoPanel.transform, "", 16, TextAnchor.MiddleCenter);
        hpText.fontStyle = FontStyle.Bold;
        SetRect(hpText.rectTransform, new Vector2(0.48f, 1f), new Vector2(0.48f, 1f), new Vector2(-18f, -16f), new Vector2(MainBarWidth, 22f));
        hpBarFill = CreateBar(infoPanel.transform, "HP", new Vector2(0.48f, 1f), new Vector2(-18f, -44f), 18f, new Color(0.16f, 0.03f, 0.035f, 0.9f), new Color(0.9f, 0.12f, 0.12f, 0.95f));

        expText = CreateText(infoPanel.transform, "", 14, TextAnchor.MiddleCenter);
        expText.color = new Color(0.68f, 0.8f, 1f);
        SetRect(expText.rectTransform, new Vector2(0.48f, 1f), new Vector2(0.48f, 1f), new Vector2(-18f, -86f), new Vector2(MainBarWidth, 20f));
        expBarFill = CreateBar(infoPanel.transform, "EXP", new Vector2(0.48f, 1f), new Vector2(-18f, -112f), 14f, new Color(0.03f, 0.05f, 0.18f, 0.9f), new Color(0.2f, 0.45f, 0.95f, 0.95f));

        CreateDivider(infoPanel.transform, 0.21f);
        CreateAttackSlot();
        CreateSkillSlot();
        CreateInvincibleButton();
    }

    private void CreateInvincibleButton()
    {
        invincibleButton = CreateButton(gameCanvas.transform, "\u65e0\u654c OFF", new Color(0.22f, 0.28f, 0.38f));
        RectTransform rect = invincibleButton.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-16f, -16f);
        rect.sizeDelta = new Vector2(150f, 46f);

        invincibleButtonImage = invincibleButton.targetGraphic as Image;
        invincibleButtonText = invincibleButton.GetComponentInChildren<Text>();
        if (invincibleButtonText != null)
        {
            invincibleButtonText.fontSize = 20;
        }

        invincibleButton.onClick.AddListener(delegate
        {
            if (gameManager != null && gameManager.PlayerHealth != null)
            {
                gameManager.PlayerHealth.ToggleInvincible();
                RefreshInvincibleButton();
            }
        });

        RefreshInvincibleButton();
    }

    private void RefreshInvincibleButton()
    {
        if (invincibleButtonText == null || invincibleButtonImage == null || gameManager == null || gameManager.PlayerHealth == null)
        {
            return;
        }

        bool enabled = gameManager.PlayerHealth.IsInvincible;
        Color color = enabled ? new Color(0.15f, 0.58f, 0.92f, 0.96f) : new Color(0.22f, 0.28f, 0.38f, 0.92f);
        invincibleButtonText.text = enabled ? "\u65e0\u654c ON" : "\u65e0\u654c OFF";
        invincibleButtonImage.color = color;

        if (invincibleButton != null)
        {
            ColorBlock colors = invincibleButton.colors;
            colors.normalColor = color;
            colors.highlightedColor = Brighten(color, 1.25f);
            colors.pressedColor = Darken(color, 0.72f);
            colors.selectedColor = color;
            invincibleButton.colors = colors;
        }
    }

    private void BuildUpgradeCard(Transform parent, UpgradeOption option, float xOffset)
    {
        Color cardColor;
        Color borderColor;
        Color labelColor;
        string labelText;

        if (option.Category == UpgradeCategory.Mage)
        {
            cardColor = new Color(0.055f, 0.045f, 0.18f, 1f);
            borderColor = new Color(0.45f, 0.35f, 1f, 0.95f);
            labelColor = new Color(0.6f, 0.55f, 1f);
            labelText = "\u6cd5\u5e08";
        }
        else if (option.Category == UpgradeCategory.Warrior)
        {
            cardColor = new Color(0.18f, 0.045f, 0.035f, 1f);
            borderColor = new Color(1f, 0.42f, 0.15f, 0.95f);
            labelColor = new Color(1f, 0.5f, 0.18f);
            labelText = "\u6218\u58eb";
        }
        else if (option.Category == UpgradeCategory.Rare)
        {
            cardColor = new Color(0.13f, 0.09f, 0.025f, 1f);
            borderColor = new Color(1f, 0.78f, 0.18f, 0.98f);
            labelColor = new Color(1f, 0.82f, 0.25f);
            labelText = "\u7a00\u6709";
        }
        else
        {
            cardColor = new Color(0.05f, 0.075f, 0.15f, 1f);
            borderColor = new Color(0.34f, 0.55f, 0.82f, 0.92f);
            labelColor = new Color(0.5f, 0.68f, 0.95f);
            labelText = "\u901a\u7528";
        }

        GameObject cardObj = new GameObject("Upgrade Card");
        cardObj.transform.SetParent(parent, false);
        Image raycastImage = cardObj.AddComponent<Image>();
        raycastImage.sprite = whiteSprite;
        raycastImage.color = cardColor;

        RectTransform cardRect = cardObj.GetComponent<RectTransform>();
        SetRect(cardRect, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(xOffset, 0f), new Vector2(226f, 292f));
        AddBorder(cardObj.transform, borderColor, 4f);

        Button cardButton = cardObj.AddComponent<Button>();
        cardButton.targetGraphic = raycastImage;
        ColorBlock colors = cardButton.colors;
        colors.normalColor = cardColor;
        colors.highlightedColor = Brighten(cardColor, 1.38f);
        colors.pressedColor = Darken(cardColor, 0.72f);
        colors.selectedColor = cardColor;
        colors.fadeDuration = 0.1f;
        cardButton.colors = colors;
        cardButton.onClick.AddListener(delegate
        {
            if (upgradePanel != null)
            {
                upgradeManager.ApplyUpgrade(option);
            }
        });

        Text label = CreateText(cardObj.transform, labelText, 14, TextAnchor.MiddleCenter);
        label.color = labelColor;
        label.fontStyle = FontStyle.Bold;
        label.raycastTarget = false;
        SetRect(label.rectTransform, new Vector2(0.05f, 0.82f), new Vector2(0.95f, 0.95f), Vector2.zero, Vector2.zero);

        Text name = CreateText(cardObj.transform, LocalizeUpgradeTitle(option), 18, TextAnchor.MiddleCenter);
        name.color = Color.white;
        name.fontStyle = FontStyle.Bold;
        name.raycastTarget = false;
        SetRect(name.rectTransform, new Vector2(0.06f, 0.58f), new Vector2(0.94f, 0.79f), Vector2.zero, Vector2.zero);

        Text desc = CreateText(cardObj.transform, LocalizeUpgradeDescription(option), 14, TextAnchor.MiddleCenter);
        desc.color = new Color(0.72f, 0.76f, 0.84f);
        desc.raycastTarget = false;
        SetRect(desc.rectTransform, new Vector2(0.08f, 0.18f), new Vector2(0.92f, 0.55f), Vector2.zero, Vector2.zero);
    }

    private void HandlePause()
    {
        if (gameManager == null) return;

        if (gameManager.IsPaused)
        {
            HidePauseMenu();
            gameManager.TogglePause();
        }
        else
        {
            gameManager.TogglePause();
            ShowPauseMenu();
        }
    }

    private void ShowPauseMenu()
    {
        if (pauseMenu != null) return;

        pauseMenu = CreatePanel("Pause Menu", new Color(0.015f, 0.02f, 0.045f, 0.93f), new Vector2(440f, 360f));

        Text title = CreateText(pauseMenu.transform, "\u6682\u505c", 42, TextAnchor.MiddleCenter);
        title.color = new Color(1f, 0.85f, 0.2f);
        title.fontStyle = FontStyle.Bold;
        AddTextShadow(title, 0.85f);
        SetRect(title.rectTransform, new Vector2(0.5f, 0.82f), new Vector2(0.5f, 0.82f), Vector2.zero, new Vector2(360f, 60f));

        Button resumeBtn = CreateButton(pauseMenu.transform, "\u7ee7\u7eed", new Color(0.18f, 0.48f, 0.82f));
        SetRect(resumeBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.60f), new Vector2(0.5f, 0.60f), Vector2.zero, new Vector2(280f, 56f));
        resumeBtn.onClick.AddListener(delegate { HandlePause(); });

        Button restartBtn = CreateButton(pauseMenu.transform, "\u91cd\u65b0\u5f00\u59cb", new Color(0.22f, 0.32f, 0.52f));
        SetRect(restartBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), Vector2.zero, new Vector2(280f, 56f));
        restartBtn.onClick.AddListener(delegate
        {
            HidePauseMenu();
            gameManager.TogglePause();
            SceneLoader.LoadGame();
        });

        Button menuBtn = CreateButton(pauseMenu.transform, "\u8fd4\u56de\u4e3b\u83dc\u5355", new Color(0.42f, 0.18f, 0.18f));
        SetRect(menuBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.24f), new Vector2(0.5f, 0.24f), Vector2.zero, new Vector2(280f, 56f));
        menuBtn.onClick.AddListener(delegate { SceneLoader.LoadMainMenu(); });
    }

    private void HidePauseMenu()
    {
        if (pauseMenu != null)
        {
            Destroy(pauseMenu);
            pauseMenu = null;
        }
    }

    private void BuildResultPanel(Transform parent, string titleText, Color titleColor, Color retryColor, Color backColor, bool victory)
    {
        Text title = CreateText(parent, titleText, 50, TextAnchor.MiddleCenter);
        title.color = titleColor;
        title.fontStyle = FontStyle.Bold;
        AddTextShadow(title, 0.9f);
        SetRect(title.rectTransform, new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.88f), Vector2.zero, new Vector2(480f, 72f));

        int totalSeconds = Mathf.FloorToInt(gameManager.SurvivalTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        int diff = gameManager.DifficultyLevel;

        CreateResultLine(parent, string.Format("\u751f\u5b58\u65f6\u95f4  {0:00}:{1:00}", minutes, seconds), 0.68f, new Color(0.86f, 0.91f, 1f));
        CreateResultLine(parent, string.Format("\u51fb\u6740\u6570  {0}      \u7b49\u7ea7  Lv.{1}", gameManager.KillCount, gameManager.Level), 0.57f, new Color(0.86f, 0.91f, 1f));
        CreateResultLine(parent, string.Format("\u901a\u5173\u6ce2\u6b21  Wave {0} / {1}", victory ? GameManager.TotalWaves : Mathf.Min(gameManager.CurrentWave, GameManager.TotalWaves - 1), GameManager.TotalWaves), 0.46f, diff >= 3 ? new Color(1f, 0.35f, 0.2f) : new Color(1f, 0.7f, 0.2f));

        Text rating = CreateText(parent, "\u8bc4\u4ef7  " + GetResultRating(victory), 22, TextAnchor.MiddleCenter);
        rating.color = titleColor;
        rating.fontStyle = FontStyle.Bold;
        SetRect(rating.rectTransform, new Vector2(0.5f, 0.34f), new Vector2(0.5f, 0.34f), Vector2.zero, new Vector2(460f, 34f));

        Button retryButton = CreateButton(parent, "\u91cd\u65b0\u5f00\u59cb", retryColor);
        SetRect(retryButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.18f), Vector2.zero, new Vector2(290f, 58f));
        retryButton.onClick.AddListener(delegate { SceneLoader.LoadGame(); });

        Button backButton = CreateButton(parent, "\u8fd4\u56de\u4e3b\u83dc\u5355", backColor);
        SetRect(backButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.06f), new Vector2(0.5f, 0.06f), Vector2.zero, new Vector2(290f, 58f));
        backButton.onClick.AddListener(delegate { SceneLoader.LoadMainMenu(); });
    }

    private void CreateResultLine(Transform parent, string value, float y, Color color)
    {
        Text text = CreateText(parent, value, 22, TextAnchor.MiddleCenter);
        text.color = color;
        SetRect(text.rectTransform, new Vector2(0.5f, y), new Vector2(0.5f, y), Vector2.zero, new Vector2(500f, 36f));
    }

    private void CreateAttackSlot()
    {
        GameObject slot = CreateSkillBox("Attack Slot BG", new Vector2(-148f, 0f));
        attackCooldownOverlay = CreateCooldownOverlay(slot.transform, "Attack Cooldown Overlay");
        attackNameText = CreateSlotText(slot.transform, "", 16, -14f, new Color(1f, 0.85f, 0.5f), true);
        CreateSlotText(slot.transform, "LMB", 13, -38f, new Color(0.65f, 0.68f, 0.75f), false);
        attackDamageText = CreateSlotText(slot.transform, "", 14, -60f, new Color(0.86f, 0.86f, 0.86f), false);
        attackCooldownText = CreateSlotBottomText(slot.transform);
    }

    private void CreateSkillSlot()
    {
        GameObject slot = CreateSkillBox("Skill Slot BG", new Vector2(-18f, 0f));
        skillCooldownOverlay = CreateCooldownOverlay(slot.transform, "Skill Cooldown Overlay");
        skillNameText = CreateSlotText(slot.transform, "", 16, -14f, new Color(1f, 0.85f, 0.5f), true);
        CreateSlotText(slot.transform, "E", 13, -38f, new Color(0.65f, 0.68f, 0.75f), false);
        skillDamageText = CreateSlotText(slot.transform, "", 14, -60f, new Color(0.86f, 0.86f, 0.86f), false);
        skillCooldownText = CreateSlotBottomText(slot.transform);
    }

    private GameObject CreateSkillBox(string name, Vector2 anchoredPosition)
    {
        GameObject slotBg = new GameObject(name);
        slotBg.transform.SetParent(infoPanel.transform, false);
        Image bgImage = slotBg.AddComponent<Image>();
        bgImage.sprite = whiteSprite;
        bgImage.color = new Color(0.08f, 0.10f, 0.18f, 0.92f);
        RectTransform slotRect = bgImage.rectTransform;
        slotRect.anchorMin = new Vector2(1f, 0.5f);
        slotRect.anchorMax = new Vector2(1f, 0.5f);
        slotRect.pivot = new Vector2(1f, 0.5f);
        slotRect.anchoredPosition = anchoredPosition;
        slotRect.sizeDelta = new Vector2(118f, 118f);
        AddBorder(slotBg.transform, new Color(0.25f, 0.35f, 0.55f, 0.8f), 2f);
        return slotBg;
    }

    private Image CreateCooldownOverlay(Transform parent, string name)
    {
        GameObject overlayObj = new GameObject(name);
        overlayObj.transform.SetParent(parent, false);
        Image overlay = overlayObj.AddComponent<Image>();
        overlay.sprite = whiteSprite;
        overlay.color = new Color(0f, 0f, 0f, 0.58f);
        overlay.type = Image.Type.Filled;
        overlay.fillMethod = Image.FillMethod.Radial360;
        overlay.fillOrigin = 2;
        overlay.fillClockwise = false;
        overlay.fillAmount = 0f;
        SetRect(overlay.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        return overlay;
    }

    private Text CreateSlotText(Transform parent, string value, int size, float y, Color color, bool bold)
    {
        Text text = CreateText(parent, value, size, TextAnchor.MiddleCenter);
        text.color = color;
        text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        SetRect(text.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(105f, 22f));
        return text;
    }

    private Text CreateSlotBottomText(Transform parent)
    {
        Text text = CreateText(parent, "", 15, TextAnchor.MiddleCenter);
        text.fontStyle = FontStyle.Bold;
        text.color = new Color(0.4f, 1f, 0.4f);
        SetRect(text.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(105f, 22f));
        return text;
    }

    private Image CreateBar(Transform parent, string name, Vector2 anchor, Vector2 position, float height, Color backColor, Color fillColor)
    {
        GameObject bgObject = new GameObject(name + " Bar Background");
        bgObject.transform.SetParent(parent, false);
        Image bg = bgObject.AddComponent<Image>();
        bg.sprite = whiteSprite;
        bg.color = backColor;
        SetRect(bg.rectTransform, anchor, anchor, position, new Vector2(MainBarWidth, height));

        GameObject fillObject = new GameObject(name + " Bar Fill");
        fillObject.transform.SetParent(bgObject.transform, false);
        Image fill = fillObject.AddComponent<Image>();
        fill.sprite = whiteSprite;
        fill.color = fillColor;
        RectTransform fillRect = fill.rectTransform;
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(0f, 1f);
        fillRect.anchoredPosition = Vector2.zero;
        fillRect.sizeDelta = new Vector2(MainBarWidth, 0f);
        return fill;
    }

    private Sprite CreateWhiteSprite()
    {
        Texture2D texture = new Texture2D(4, 4);
        for (int i = 0; i < 16; i++)
        {
            texture.SetPixel(i % 4, i / 4, Color.white);
        }
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
    }

    private Canvas CreateCanvas(string name)
    {
        GameObject canvasObject = new GameObject(name);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private GameObject CreatePanel(string name, Color color, Vector2 size)
    {
        GameObject panelObject = new GameObject(name);
        panelObject.transform.SetParent(gameCanvas.transform, false);
        Image image = panelObject.AddComponent<Image>();
        image.sprite = whiteSprite;
        image.color = color;
        SetRect(panelObject.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);
        AddBorder(panelObject.transform, new Color(0.35f, 0.45f, 0.65f, 0.65f), 2f);
        return panelObject;
    }

    private Text CreateText(Transform parent, string value, int size, TextAnchor alignment)
    {
        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(parent, false);
        Text text = textObject.AddComponent<Text>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    private Button CreateButton(Transform parent, string label, Color color)
    {
        GameObject buttonObject = new GameObject("Button");
        buttonObject.transform.SetParent(parent, false);
        Image image = buttonObject.AddComponent<Image>();
        image.sprite = whiteSprite;
        image.color = color;
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = Brighten(color, 1.35f);
        colors.pressedColor = Darken(color, 0.7f);
        colors.selectedColor = color;
        colors.fadeDuration = 0.1f;
        button.colors = colors;

        AddBorder(buttonObject.transform, new Color(1f, 1f, 1f, 0.18f), 2f);

        Text text = CreateText(buttonObject.transform, label, 24, TextAnchor.MiddleCenter);
        text.color = Color.white;
        text.fontStyle = FontStyle.Bold;
        AddTextShadow(text, 0.75f);
        SetRect(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        return button;
    }

    private void CreateDivider(Transform parent, float xAnchor)
    {
        GameObject divider = new GameObject("Divider");
        divider.transform.SetParent(parent, false);
        Image image = divider.AddComponent<Image>();
        image.sprite = whiteSprite;
        image.color = new Color(1f, 1f, 1f, 0.12f);
        SetRect(image.rectTransform, new Vector2(xAnchor, 0.18f), new Vector2(xAnchor, 0.82f), Vector2.zero, new Vector2(2f, 0f));
    }

    private void AddBorder(Transform parent, Color color, float thickness)
    {
        CreateBorderPart(parent, "Border Top", color, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -thickness * 0.5f), new Vector2(0f, thickness));
        CreateBorderPart(parent, "Border Bottom", color, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, thickness * 0.5f), new Vector2(0f, thickness));
        CreateBorderPart(parent, "Border Left", color, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(thickness * 0.5f, 0f), new Vector2(thickness, 0f));
        CreateBorderPart(parent, "Border Right", color, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-thickness * 0.5f, 0f), new Vector2(thickness, 0f));
    }

    private void CreateBorderPart(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
    {
        GameObject part = new GameObject(name);
        part.transform.SetParent(parent, false);
        Image image = part.AddComponent<Image>();
        image.sprite = whiteSprite;
        image.color = color;
        image.raycastTarget = false;
        SetRect(image.rectTransform, anchorMin, anchorMax, position, size);
    }

    private void AddTextShadow(Text text, float alpha)
    {
        Shadow shadow = text.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, alpha);
        shadow.effectDistance = new Vector2(1.5f, -1.5f);
    }

    private Color Brighten(Color color, float amount)
    {
        return new Color(Mathf.Clamp01(color.r * amount), Mathf.Clamp01(color.g * amount), Mathf.Clamp01(color.b * amount), color.a);
    }

    private Color Darken(Color color, float amount)
    {
        return new Color(color.r * amount, color.g * amount, color.b * amount, color.a);
    }

    private string GetCharacterLabel(CharacterType type)
    {
        return type == CharacterType.Warrior ? "\u6218\u58eb" : "\u6cd5\u5e08";
    }

    private string GetResultRating(bool victory)
    {
        if (victory)
        {
            if (gameManager.KillCount >= 55) return "\u5b8c\u7f8e\u6e05\u573a";
            if (gameManager.KillCount >= 35) return "\u7a33\u5065\u901a\u5173";
            return "\u6210\u529f\u751f\u8fd8";
        }

        int reachedWave = Mathf.Min(gameManager.CurrentWave, GameManager.TotalWaves - 1);
        if (reachedWave >= 3 && gameManager.IsBossWave) return "\u5dee\u4e00\u70b9";
        if (reachedWave >= 3) return "\u7ad9\u7a33\u811a\u8ddf";
        if (reachedWave >= 2) return "\u7ee7\u7eed\u52aa\u529b";
        return "\u518d\u6765\u4e00\u5c40";
    }

    private string LocalizeUpgradeTitle(UpgradeOption option)
    {
        switch (option.Type)
        {
            case UpgradeType.AttackDamage: return "\u666e\u653b\u4f24\u5bb3 +10";
            case UpgradeType.SkillDamage: return "\u6280\u80fd\u4f24\u5bb3 +5";
            case UpgradeType.MaxHealth: return "\u6700\u5927\u751f\u547d +20";
            case UpgradeType.MoveSpeed: return "\u79fb\u52a8\u901f\u5ea6 +10%";
            case UpgradeType.AttackCooldown: return "\u666e\u653b\u51b7\u5374 -10%";
            case UpgradeType.Heal: return "\u751f\u547d\u6062\u590d +40";
            case UpgradeType.SkillCooldown: return "\u6280\u80fd\u51b7\u5374 -10%";
            case UpgradeType.FireballSplit: return "\u706b\u7403\u5206\u88c2";
            case UpgradeType.ArcaneRainBigger: return "\u5965\u672f\u96e8\u6269\u5c55";
            case UpgradeType.ArcaneRainLonger: return "\u5965\u672f\u96e8\u5ef6\u957f";
            case UpgradeType.ArcaneRainFaster: return "\u5965\u672f\u96e8\u52a0\u901f";
            case UpgradeType.SwordWavePierce: return "\u5251\u6c14\u7a7f\u900f";
            case UpgradeType.SwordWaveBigger: return "\u5251\u6c14\u6269\u5927";
            case UpgradeType.DashSlashHeal: return "\u51b2\u523a\u56de\u590d";
            case UpgradeType.DashSlashCooldown: return "\u51b2\u523a\u51b7\u5374";
            case UpgradeType.FrostField: return "\u971c\u51bb\u6cd5\u9635";
            case UpgradeType.ManaSurge: return "\u9b54\u529b\u6d8c\u52a8";
            case UpgradeType.EarthSplitter: return "\u5927\u5730\u88c2\u65a9";
            case UpgradeType.BattleFrenzy: return "\u6218\u6597\u72c2\u70ed";
        }
        return option.Title;
    }

    private string LocalizeUpgradeDescription(UpgradeOption option)
    {
        switch (option.Type)
        {
            case UpgradeType.AttackDamage: return "\u666e\u901a\u653b\u51fb\u4f24\u5bb3\u63d0\u5347\u3002";
            case UpgradeType.SkillDamage: return "\u6280\u80fd\u4f24\u5bb3\u63d0\u5347\u3002";
            case UpgradeType.MaxHealth: return "\u751f\u547d\u4e0a\u9650\u63d0\u5347\uff0c\u5e76\u7acb\u5373\u56de\u590d\u751f\u547d\u3002";
            case UpgradeType.MoveSpeed: return "\u79fb\u52a8\u901f\u5ea6\u63d0\u5347\u3002";
            case UpgradeType.AttackCooldown: return "\u666e\u901a\u653b\u51fb\u51b7\u5374\u7f29\u77ed\u3002";
            case UpgradeType.Heal: return "\u7acb\u5373\u56de\u590d\u751f\u547d\u3002";
            case UpgradeType.SkillCooldown: return "\u6280\u80fd\u51b7\u5374\u7f29\u77ed\u3002";
            case UpgradeType.FireballSplit: return "\u706b\u7403\u4ee5\u5c0f\u89d2\u5ea6\u5206\u88c2\u53d1\u5c04\u3002";
            case UpgradeType.ArcaneRainBigger: return "\u5965\u672f\u96e8\u8303\u56f4\u589e\u5927\u3002";
            case UpgradeType.ArcaneRainLonger: return "\u5965\u672f\u96e8\u6301\u7eed\u65f6\u95f4\u5ef6\u957f\u3002";
            case UpgradeType.ArcaneRainFaster: return "\u5965\u672f\u96e8\u89e6\u53d1\u66f4\u9891\u7e41\u3002";
            case UpgradeType.SwordWavePierce: return "\u5251\u6c14\u53ef\u7a7f\u900f\u654c\u4eba\u3002";
            case UpgradeType.SwordWaveBigger: return "\u5251\u6c14\u53d8\u5f97\u66f4\u5927\u3002";
            case UpgradeType.DashSlashHeal: return "\u51b2\u523a\u65a9\u547d\u4e2d\u65f6\u56de\u590d\u751f\u547d\u3002";
            case UpgradeType.DashSlashCooldown: return "\u51b2\u523a\u65a9\u51b7\u5374\u7f29\u77ed\u3002";
            case UpgradeType.FrostField: return "\u5965\u672f\u96e8\u4e2d\u654c\u4eba\u88ab\u66f4\u5f3a\u51cf\u901f\u3002";
            case UpgradeType.ManaSurge: return "\u6280\u80fd\u51b7\u5374\u5927\u5e45\u7f29\u77ed\u3002";
            case UpgradeType.EarthSplitter: return "\u51b2\u523a\u65a9\u7ed3\u675f\u65f6\u5f15\u53d1\u7206\u53d1\u3002";
            case UpgradeType.BattleFrenzy: return "\u51b2\u523a\u547d\u4e2d\u540e\u77ed\u6682\u63d0\u5347\u653b\u901f\u3002";
        }
        return option.Description;
    }

    private void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }
    }
}
