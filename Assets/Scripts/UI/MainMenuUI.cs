using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    private Font font;
    private Sprite whiteSprite;
    private Canvas canvas;
    private GameObject howToPlayPanel;

    private void Start()
    {
        Time.timeScale = 1f;
        font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        whiteSprite = CreateWhiteSprite();
        EnsureEventSystem();
        BuildUI();
    }

    private void BuildUI()
    {
        canvas = CreateCanvas("Main Menu Canvas");
        CreateScreenBackdrop();

        Text title = CreateText(canvas.transform, "\u5730\u4e0b\u57ce\u751f\u5b58", 62, TextAnchor.MiddleCenter);
        title.fontStyle = FontStyle.Bold;
        title.color = new Color(1f, 0.85f, 0.2f);
        AddTextShadow(title, 0.9f);
        SetRect(title.rectTransform, new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), Vector2.zero, new Vector2(820f, 86f));

        Text subtitle = CreateText(canvas.transform, "4 \u6ce2\u6b21\u6e05\u602a\uff0c\u5347\u7ea7\u6784\u7b51\uff0c\u51fb\u8d25 Boss", 22, TextAnchor.MiddleCenter);
        subtitle.color = new Color(0.75f, 0.80f, 0.9f);
        SetRect(subtitle.rectTransform, new Vector2(0.5f, 0.66f), new Vector2(0.5f, 0.66f), Vector2.zero, new Vector2(760f, 40f));

        CreateMenuButton(canvas.transform, "\u5f00\u59cb\u6e38\u620f", new Vector2(0.5f, 0.48f), new Color(0.18f, 0.48f, 0.82f),
            delegate { SceneLoader.LoadCharacterSelect(); });

        CreateMenuButton(canvas.transform, "\u73a9\u6cd5\u8bf4\u660e", new Vector2(0.5f, 0.38f), new Color(0.22f, 0.32f, 0.52f),
            delegate { ShowHowToPlay(); });

        CreateMenuButton(canvas.transform, "\u9000\u51fa\u6e38\u620f", new Vector2(0.5f, 0.28f), new Color(0.42f, 0.18f, 0.18f),
            delegate { Application.Quit(); });
    }

    private void ShowHowToPlay()
    {
        if (howToPlayPanel != null) return;

        howToPlayPanel = CreatePanel("How To Play Panel", new Color(0.015f, 0.02f, 0.045f, 0.94f), new Vector2(620f, 470f));

        Text panelTitle = CreateText(howToPlayPanel.transform, "\u73a9\u6cd5\u8bf4\u660e", 42, TextAnchor.MiddleCenter);
        panelTitle.color = new Color(1f, 0.85f, 0.2f);
        panelTitle.fontStyle = FontStyle.Bold;
        AddTextShadow(panelTitle, 0.85f);
        SetRect(panelTitle.rectTransform, new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.88f), Vector2.zero, new Vector2(500f, 60f));

        string[] instructions =
        {
            "WASD / \u65b9\u5411\u952e  \u79fb\u52a8",
            "\u9f20\u6807\u5de6\u952e  \u666e\u901a\u653b\u51fb",
            "E  \u91ca\u653e\u6280\u80fd",
            "\u51fb\u6740\u654c\u4eba\u83b7\u5f97\u7ecf\u9a8c",
            "\u5347\u7ea7\u65f6\u9009\u62e9\u4e00\u4e2a\u5f3a\u5316",
            "\u6e05\u5b8c\u6bcf\u6ce2\u654c\u4eba\u540e\u664b\u7ea7",
            "\u7b2c4\u6ce2\u6e05\u573a\u540e\u51fb\u8d25 Boss \u80dc\u5229"
        };

        for (int i = 0; i < instructions.Length; i++)
        {
            Text line = CreateText(howToPlayPanel.transform, instructions[i], 23, TextAnchor.MiddleCenter);
            line.color = new Color(0.86f, 0.91f, 1f);
            SetRect(line.rectTransform, new Vector2(0.5f, 0.75f - i * 0.09f), new Vector2(0.5f, 0.75f - i * 0.09f), Vector2.zero, new Vector2(520f, 36f));
        }

        CreateMenuButton(howToPlayPanel.transform, "\u8fd4\u56de", new Vector2(0.5f, 0.10f), new Color(0.28f, 0.34f, 0.52f),
            delegate { HideHowToPlay(); });
    }

    private void HideHowToPlay()
    {
        if (howToPlayPanel != null)
        {
            Destroy(howToPlayPanel);
            howToPlayPanel = null;
        }
    }

    private void CreateScreenBackdrop()
    {
        GameObject backdrop = new GameObject("Menu Backdrop");
        backdrop.transform.SetParent(canvas.transform, false);
        Image image = backdrop.AddComponent<Image>();
        image.sprite = whiteSprite;
        image.color = new Color(0.015f, 0.018f, 0.035f, 0.96f);
        SetRect(image.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        GameObject band = new GameObject("Title Band");
        band.transform.SetParent(canvas.transform, false);
        Image bandImage = band.AddComponent<Image>();
        bandImage.sprite = whiteSprite;
        bandImage.color = new Color(0.06f, 0.08f, 0.14f, 0.72f);
        SetRect(bandImage.rectTransform, new Vector2(0f, 0.58f), new Vector2(1f, 0.82f), Vector2.zero, Vector2.zero);
    }

    private Button CreateMenuButton(Transform parent, string label, Vector2 anchor, Color color, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObj = new GameObject(label + " Button");
        buttonObj.transform.SetParent(parent, false);
        Image image = buttonObj.AddComponent<Image>();
        image.sprite = whiteSprite;
        image.color = color;
        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);
        ApplyButtonColors(button, color);
        AddBorder(buttonObj.transform, new Color(1f, 1f, 1f, 0.18f), 2f);

        RectTransform rect = buttonObj.GetComponent<RectTransform>();
        SetRect(rect, anchor, anchor, Vector2.zero, new Vector2(320f, 58f));

        Text text = CreateText(buttonObj.transform, label, 26, TextAnchor.MiddleCenter);
        text.color = Color.white;
        text.fontStyle = FontStyle.Bold;
        AddTextShadow(text, 0.75f);
        SetRect(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        return button;
    }

    private Sprite CreateWhiteSprite()
    {
        Texture2D texture = new Texture2D(4, 4);
        for (int i = 0; i < 16; i++) texture.SetPixel(i % 4, i / 4, Color.white);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
    }

    private Canvas CreateCanvas(string name)
    {
        GameObject canvasObject = new GameObject(name);
        Canvas c = canvasObject.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();
        return c;
    }

    private GameObject CreatePanel(string name, Color color, Vector2 size)
    {
        GameObject panelObject = new GameObject(name);
        panelObject.transform.SetParent(canvas.transform, false);
        Image image = panelObject.AddComponent<Image>();
        image.sprite = whiteSprite;
        image.color = color;
        SetRect(panelObject.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);
        AddBorder(panelObject.transform, new Color(0.35f, 0.45f, 0.65f, 0.7f), 2f);
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

    private void ApplyButtonColors(Button button, Color color)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = Brighten(color, 1.35f);
        colors.pressedColor = Darken(color, 0.7f);
        colors.selectedColor = color;
        colors.fadeDuration = 0.1f;
        button.colors = colors;
    }

    private void AddBorder(Transform parent, Color color, float thickness)
    {
        CreateBorderPart(parent, color, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -thickness * 0.5f), new Vector2(0f, thickness));
        CreateBorderPart(parent, color, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, thickness * 0.5f), new Vector2(0f, thickness));
        CreateBorderPart(parent, color, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(thickness * 0.5f, 0f), new Vector2(thickness, 0f));
        CreateBorderPart(parent, color, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-thickness * 0.5f, 0f), new Vector2(thickness, 0f));
    }

    private void CreateBorderPart(Transform parent, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
    {
        GameObject part = new GameObject("Border");
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
