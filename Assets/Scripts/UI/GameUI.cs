using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    private GameManager gameManager;
    private UpgradeManager upgradeManager;
    private Font font;
    private Text statusText;
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
        statusText.text =
            gameManager.PlayerStats.DisplayName + "\n" +
            "HP: " + gameManager.PlayerHealth.CurrentHealth + " / " + gameManager.PlayerHealth.MaxHealth + "\n" +
            "LV: " + gameManager.Level + "\n" +
            "EXP: " + gameManager.CurrentExp + " / " + gameManager.NextLevelExp + "\n" +
            "Skill: " + skillText;

        if (hpBarFill != null)
        {
            float ratio = Mathf.Clamp01((float)gameManager.PlayerHealth.CurrentHealth / gameManager.PlayerHealth.MaxHealth);
            hpBarFill.rectTransform.sizeDelta = new Vector2(300f * ratio, 0f);
        }
    }

    public void ShowUpgradePanel(List<UpgradeOption> options)
    {
        HideUpgradePanel();
        upgradePanel = CreatePanel("Upgrade Panel", new Color(0f, 0f, 0f, 0.82f), new Vector2(620f, 430f));

        Text title = CreateText(upgradePanel.transform, "\u9009\u62e9\u4e00\u4e2a\u5347\u7ea7", 34, TextAnchor.MiddleCenter);
        SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -55f), new Vector2(500f, 60f));

        for (int i = 0; i < options.Count; i++)
        {
            UpgradeOption option = options[i];
            Button button = CreateButton(upgradePanel.transform, option.Title + "\n" + option.Description, new Color(0.18f, 0.35f, 0.62f));
            SetRect(button.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -140f - i * 95f), new Vector2(520f, 76f));
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
        Canvas canvas = CreateCanvas("Game Canvas");
        statusText = CreateText(canvas.transform, "", 23, TextAnchor.UpperLeft);
        SetRect(statusText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(170f, -95f), new Vector2(320f, 170f));

        GameObject hpBgObject = new GameObject("HP Bar Background");
        hpBgObject.transform.SetParent(canvas.transform, false);
        hpBarBackground = hpBgObject.AddComponent<Image>();
        hpBarBackground.color = new Color(0.2f, 0.04f, 0.04f, 0.85f);
        SetRect(hpBarBackground.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(170f, -195f), new Vector2(300f, 16f));

        GameObject hpFillObject = new GameObject("HP Bar Fill");
        hpFillObject.transform.SetParent(hpBgObject.transform, false);
        hpBarFill = hpFillObject.AddComponent<Image>();
        hpBarFill.color = new Color(0.88f, 0.15f, 0.15f, 0.92f);
        hpBarFill.rectTransform.pivot = new Vector2(0f, 0.5f);
        hpBarFill.rectTransform.anchorMin = new Vector2(0f, 0f);
        hpBarFill.rectTransform.anchorMax = new Vector2(0f, 1f);
        hpBarFill.rectTransform.anchoredPosition = Vector2.zero;
        hpBarFill.rectTransform.sizeDelta = new Vector2(300f, 0f);
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
        Canvas canvas = FindObjectOfType<Canvas>();
        panelObject.transform.SetParent(canvas.transform, false);
        Image image = panelObject.AddComponent<Image>();
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
        image.color = color;
        Button button = buttonObject.AddComponent<Button>();

        Text text = CreateText(buttonObject.transform, label, 22, TextAnchor.MiddleCenter);
        text.color = Color.white;
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
