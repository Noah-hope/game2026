using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    private GameManager gameManager;
    private UpgradeManager upgradeManager;
    private Font font;
    private Canvas gameCanvas;
    private Sprite whiteSprite;
    private GameObject infoPanel;
    private Text statusText;
    private Text hpText;
    private Image hpBarBackground;
    private Image hpBarFill;
    private GameObject upgradePanel;
    private GameObject gameOverPanel;

    public void Initialize(GameManager manager, UpgradeManager upgrades)
    {
        gameManager = manager;
        upgradeManager = upgrades;
        font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        EnsureEventSystem();
        BuildUI();
        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (statusText == null || gameManager == null || gameManager.PlayerHealth == null || gameManager.PlayerController == null)
        {
            return;
        }

        string skillText = gameManager.PlayerController.GetSkillCooldownText();
        CharacterStats stats = gameManager.PlayerStats;
        statusText.text =
            stats.DisplayName + "\n" +
            "LV: " + gameManager.Level + "  EXP: " + gameManager.CurrentExp + " / " + gameManager.NextLevelExp + "\n" +
            "Move Speed: " + stats.MoveSpeed.ToString("0.0") + "\n" +
            "Attack: " + stats.AttackDamage + "\n" +
            "Cooldown: " + stats.AttackCooldown.ToString("0.00") + "s\n" +
            "Skill: " + skillText;

        if (hpText != null)
        {
            hpText.text = gameManager.PlayerHealth.CurrentHealth + " / " + gameManager.PlayerHealth.MaxHealth;
        }

        if (hpBarFill != null)
        {
            float ratio = Mathf.Clamp01((float)gameManager.PlayerHealth.CurrentHealth / gameManager.PlayerHealth.MaxHealth);
            hpBarFill.rectTransform.sizeDelta = new Vector2(300f * ratio, 0f);
        }
    }

    public void ShowUpgradePanel(List<UpgradeOption> options)
    {
        HideUpgradePanel();
        upgradePanel = CreatePanel("Upgrade Panel", new Color(0f, 0f, 0f, 0.82f), new Vector2(640f, 480f));

        Text title = CreateText(upgradePanel.transform, "\u9009\u62e9\u4e00\u4e2a\u5347\u7ea7", 34, TextAnchor.MiddleCenter);
        SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -55f), new Vector2(500f, 60f));

        for (int i = 0; i < options.Count; i++)
        {
            UpgradeOption option = options[i];
            Button button = CreateButton(upgradePanel.transform, option.Title + "\n" + option.Description, new Color(0.18f, 0.35f, 0.62f));
            SetRect(button.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -150f - i * 110f), new Vector2(540f, 88f));
            button.onClick.AddListener(delegate { upgradeManager.ApplyUpgrade(option); });
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
        if (gameOverPanel != null)
        {
            return;
        }

        gameOverPanel = CreatePanel("Game Over Panel", new Color(0f, 0f, 0f, 0.86f), new Vector2(520f, 300f));

        Text title = CreateText(gameOverPanel.transform, "\u6e38\u620f\u7ed3\u675f", 42, TextAnchor.MiddleCenter);
        SetRect(title.rectTransform, new Vector2(0.5f, 0.68f), new Vector2(0.5f, 0.68f), Vector2.zero, new Vector2(420f, 80f));

        Button retryButton = CreateButton(gameOverPanel.transform, "\u91cd\u65b0\u5f00\u59cb", new Color(0.2f, 0.42f, 0.72f));
        SetRect(retryButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.38f), new Vector2(0f, 0f), new Vector2(260f, 62f));
        retryButton.onClick.AddListener(delegate { SceneLoader.LoadGame(); });

        Button backButton = CreateButton(gameOverPanel.transform, "\u8fd4\u56de\u4e3b\u83dc\u5355", new Color(0.5f, 0.16f, 0.18f));
        SetRect(backButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.16f), new Vector2(0.5f, 0.16f), Vector2.zero, new Vector2(260f, 62f));
        backButton.onClick.AddListener(delegate { SceneLoader.LoadMainMenu(); });
    }

    private void BuildUI()
    {
        whiteSprite = CreateWhiteSprite();
        gameCanvas = CreateCanvas("Game Canvas");
        gameCanvas.sortingOrder = 1;
        gameCanvas.overrideSorting = true;

        RectTransform canvasRect = gameCanvas.GetComponent<RectTransform>();

        GameObject panelObject = new GameObject("Info Panel");
        panelObject.transform.SetParent(canvasRect, false);
        infoPanel = panelObject;
        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.sprite = whiteSprite;
        panelImage.color = new Color(0.05f, 0.05f, 0.1f, 0.75f);
        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.anchoredPosition = new Vector2(-948f, 528f);
        panelRect.sizeDelta = new Vector2(340f, 290f);

        statusText = CreateText(panelObject.transform, "Loading...", 18, TextAnchor.UpperLeft);
        statusText.color = new Color(0.95f, 0.95f, 0.95f, 1f);
        statusText.rectTransform.anchorMin = new Vector2(0f, 1f);
        statusText.rectTransform.anchorMax = new Vector2(0f, 1f);
        statusText.rectTransform.pivot = new Vector2(0f, 1f);
        statusText.rectTransform.anchoredPosition = new Vector2(12f, -10f);
        statusText.rectTransform.sizeDelta = new Vector2(316f, 150f);

        hpText = CreateText(panelObject.transform, "HP: -- / --", 15, TextAnchor.MiddleCenter);
        hpText.fontStyle = FontStyle.Bold;
        hpText.color = Color.white;
        hpText.rectTransform.anchorMin = new Vector2(0f, 1f);
        hpText.rectTransform.anchorMax = new Vector2(0f, 1f);
        hpText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        hpText.rectTransform.anchoredPosition = new Vector2(170f, -178f);
        hpText.rectTransform.sizeDelta = new Vector2(300f, 22f);

        GameObject hpBgObject = new GameObject("HP Bar Background");
        hpBgObject.transform.SetParent(panelObject.transform, false);
        hpBarBackground = hpBgObject.AddComponent<Image>();
        hpBarBackground.sprite = whiteSprite;
        hpBarBackground.color = new Color(0.15f, 0.03f, 0.03f, 0.85f);
        RectTransform hpBgRect = hpBarBackground.rectTransform;
        hpBgRect.anchorMin = new Vector2(0f, 1f);
        hpBgRect.anchorMax = new Vector2(0f, 1f);
        hpBgRect.pivot = new Vector2(0f, 1f);
        hpBgRect.anchoredPosition = new Vector2(20f, -200f);
        hpBgRect.sizeDelta = new Vector2(300f, 18f);

        GameObject hpFillObject = new GameObject("HP Bar Fill");
        hpFillObject.transform.SetParent(hpBgObject.transform, false);
        hpBarFill = hpFillObject.AddComponent<Image>();
        hpBarFill.sprite = whiteSprite;
        hpBarFill.color = new Color(0.9f, 0.15f, 0.15f, 0.92f);
        RectTransform hpFillRect = hpBarFill.rectTransform;
        hpFillRect.pivot = new Vector2(0f, 0.5f);
        hpFillRect.anchorMin = new Vector2(0f, 0f);
        hpFillRect.anchorMax = new Vector2(0f, 1f);
        hpFillRect.anchoredPosition = Vector2.zero;
        hpFillRect.sizeDelta = new Vector2(300f, 0f);
    }

    private Sprite CreateWhiteSprite()
    {
        Texture2D texture = new Texture2D(4, 4);
        Color[] pixels = new Color[16];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        texture.SetPixels(pixels);
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

        Text text = CreateText(buttonObject.transform, label, 24, TextAnchor.MiddleCenter);
        text.color = Color.white;
        text.fontStyle = FontStyle.Bold;
        Shadow shadow = text.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.75f);
        shadow.effectDistance = new Vector2(1.5f, -1.5f);
        SetRect(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        return button;
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
