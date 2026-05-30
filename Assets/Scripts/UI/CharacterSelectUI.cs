using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    private Font font;
    private Sprite whiteSprite;
    private Sprite circleSprite;
    private Canvas canvas;
    private CharacterType selectedType = CharacterType.Mage;
    private GameObject startButtonObject;
    private Image mageHighlightBorder;
    private Image warriorHighlightBorder;
    private RectTransform mageCardRect;
    private RectTransform warriorCardRect;
    private GameObject mageSelectedLabel;
    private GameObject warriorSelectedLabel;
    private Image mageCardBackground;
    private Image warriorCardBackground;

    private void Start()
    {
        Time.timeScale = 1f;
        font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        whiteSprite = CreateWhiteSprite();
        circleSprite = CreateCircleSprite();
        EnsureEventSystem();
        BuildUI();
    }

    private void BuildUI()
    {
        canvas = CreateCanvas("Character Select Canvas");
        CreateScreenBackdrop();

        Text title = CreateText(canvas.transform, "\u9009\u62e9\u89d2\u8272", 52, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.2f));
        title.fontStyle = FontStyle.Bold;
        AddTextShadow(title, 0.9f);
        SetRect(title.rectTransform, new Vector2(0.5f, 0.92f), new Vector2(0.5f, 0.92f), Vector2.zero, new Vector2(620f, 76f));

        BuildCharacterCard(new Vector2(-305f, -18f), CharacterType.Mage);
        BuildCharacterCard(new Vector2(305f, -18f), CharacterType.Warrior);

        Button backButton = CreateMenuButton(canvas.transform, "\u8fd4\u56de\u4e3b\u83dc\u5355", new Vector2(0.14f, 0.07f), new Color(0.28f, 0.32f, 0.46f), 240f, 50f, 22);
        backButton.onClick.AddListener(delegate { SceneLoader.LoadMainMenu(); });
    }

    private void BuildCharacterCard(Vector2 position, CharacterType type)
    {
        CharacterStats stats = GameData.GetCharacterStats(type);
        bool isMage = type == CharacterType.Mage;
        Sprite portrait = GameData.GetCharacterPortrait(type);
        Color accentColor = isMage ? new Color(0.42f, 0.50f, 1f, 0.95f) : new Color(1f, 0.38f, 0.16f, 0.95f);
        Color accentSoft = isMage ? new Color(0.42f, 0.55f, 1f, 0.28f) : new Color(1f, 0.33f, 0.12f, 0.28f);
        Color roleColor = isMage ? new Color(0.58f, 0.66f, 1f) : new Color(1f, 0.55f, 0.25f);

        GameObject cardObj = new GameObject(isMage ? "Mage Card" : "Warrior Card");
        cardObj.transform.SetParent(canvas.transform, false);

        Image cardBg = cardObj.AddComponent<Image>();
        cardBg.sprite = whiteSprite;
        cardBg.color = new Color(0.025f, 0.032f, 0.07f, 0.94f);

        Button cardButton = cardObj.AddComponent<Button>();
        cardButton.targetGraphic = cardBg;
        ApplyButtonColors(cardButton, cardBg.color);
        cardButton.onClick.AddListener(delegate { SelectCharacter(type); });

        RectTransform cardRect = cardObj.GetComponent<RectTransform>();
        SetRect(cardRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, new Vector2(360f, 680f));
        AddBorder(cardObj.transform, accentColor, 3f);

        if (isMage)
        {
            mageCardRect = cardRect;
            mageCardBackground = cardBg;
        }
        else
        {
            warriorCardRect = cardRect;
            warriorCardBackground = cardBg;
        }

        GameObject highlightObj = new GameObject("Highlight Border");
        highlightObj.transform.SetParent(cardObj.transform, false);
        Image highlightImage = highlightObj.AddComponent<Image>();
        highlightImage.sprite = whiteSprite;
        highlightImage.color = new Color(0.95f, 0.78f, 0.26f, 0f);
        highlightImage.raycastTarget = false;
        SetRect(highlightImage.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        AddBorder(highlightObj.transform, new Color(1f, 0.86f, 0.28f, 0.98f), 5f);
        highlightObj.SetActive(false);

        if (isMage) mageHighlightBorder = highlightImage;
        else warriorHighlightBorder = highlightImage;

        BuildPortraitShowcase(cardObj.transform, portrait, isMage, accentColor, accentSoft);

        string displayName = isMage ? "\u6cd5\u5e08" : "\u6218\u58eb";
        string role = isMage ? "\u8fdc\u7a0b / \u8303\u56f4\u63a7\u5236" : "\u8fd1\u6218 / \u51b2\u523a\u7206\u53d1";
        string playstyle = isMage ? "\u4ee5\u5965\u672f\u9886\u57df\u538b\u5236\u654c\u7fa4" : "\u7a81\u5165\u6218\u573a\u5b8c\u6210\u7206\u53d1\u65a9\u51fb";

        Text selectedLabel = CreateText(cardObj.transform, "\u5df2\u9009\u62e9", 16, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.28f));
        selectedLabel.fontStyle = FontStyle.Bold;
        selectedLabel.gameObject.SetActive(false);
        SetRect(selectedLabel.rectTransform, new Vector2(0.78f, 0.94f), new Vector2(0.78f, 0.94f), Vector2.zero, new Vector2(110f, 24f));
        AddTextShadow(selectedLabel, 0.75f);
        if (isMage) mageSelectedLabel = selectedLabel.gameObject;
        else warriorSelectedLabel = selectedLabel.gameObject;

        CreateText(cardObj.transform, displayName, 30, TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.32f),
            new Vector2(0.5f, 0.225f), new Vector2(0.5f, 0.225f), Vector2.zero, new Vector2(260f, 34f), true);

        CreateText(cardObj.transform, role, 16, TextAnchor.MiddleCenter, roleColor,
            new Vector2(0.5f, 0.184f), new Vector2(0.5f, 0.184f), Vector2.zero, new Vector2(270f, 22f), true);

        string statsLine1 = string.Format("\u751f\u547d {0}    \u901f\u5ea6 {1}", stats.MaxHealth, stats.MoveSpeed.ToString("0.0"));
        string statsLine2 = string.Format("{0}  {1}\u4f24\u5bb3", stats.AttackName, stats.AttackDamage);
        string statsLine3 = string.Format("{0}  {1}\u4f24\u5bb3 / CD {2}s", stats.SkillName, stats.SkillDamage, stats.SkillCooldown.ToString("0.0"));

        CreateText(cardObj.transform, statsLine1, 15, TextAnchor.MiddleCenter, new Color(0.86f, 0.90f, 0.96f),
            new Vector2(0.5f, 0.142f), new Vector2(0.5f, 0.142f), Vector2.zero, new Vector2(280f, 21f), false);
        CreateText(cardObj.transform, statsLine2, 14, TextAnchor.MiddleCenter, new Color(0.72f, 0.77f, 0.84f),
            new Vector2(0.5f, 0.106f), new Vector2(0.5f, 0.106f), Vector2.zero, new Vector2(290f, 20f), false);
        CreateText(cardObj.transform, statsLine3, 14, TextAnchor.MiddleCenter, new Color(0.72f, 0.77f, 0.84f),
            new Vector2(0.5f, 0.072f), new Vector2(0.5f, 0.072f), Vector2.zero, new Vector2(315f, 20f), false);
        CreateText(cardObj.transform, playstyle, 13, TextAnchor.MiddleCenter, new Color(0.62f, 0.68f, 0.78f),
            new Vector2(0.5f, 0.032f), new Vector2(0.5f, 0.032f), Vector2.zero, new Vector2(300f, 24f), false);
    }

    private void SelectCharacter(CharacterType type)
    {
        selectedType = type;

        SetHighlight(mageHighlightBorder, type == CharacterType.Mage);
        SetHighlight(warriorHighlightBorder, type == CharacterType.Warrior);
        SetCardSelected(mageCardRect, mageCardBackground, mageSelectedLabel, type == CharacterType.Mage);
        SetCardSelected(warriorCardRect, warriorCardBackground, warriorSelectedLabel, type == CharacterType.Warrior);

        if (startButtonObject != null)
        {
            Destroy(startButtonObject);
        }

        GameObject buttonObj = new GameObject("Ready Button Container");
        buttonObj.transform.SetParent(canvas.transform, false);
        startButtonObject = buttonObj;

        Button startBtn = CreateMenuButton(buttonObj.transform, "\u5f00\u59cb\u6e38\u620f", new Vector2(0.5f, 0.07f), new Color(0.2f, 0.55f, 0.25f), 270f, 56f, 26);
        startBtn.onClick.AddListener(delegate
        {
            GameData.SelectedCharacter = selectedType;
            SceneLoader.LoadGame();
        });
    }

    private void SetHighlight(Image highlight, bool visible)
    {
        if (highlight == null) return;
        highlight.gameObject.SetActive(visible);
        highlight.color = new Color(0.95f, 0.78f, 0.26f, visible ? 0.16f : 0f);
    }

    private void SetCardSelected(RectTransform cardRect, Image cardBackground, GameObject selectedLabel, bool selected)
    {
        if (cardRect != null)
        {
            cardRect.localScale = selected ? new Vector3(1.035f, 1.035f, 1f) : Vector3.one;
        }

        if (cardBackground != null)
        {
            cardBackground.color = selected ? new Color(0.045f, 0.055f, 0.11f, 0.97f) : new Color(0.025f, 0.032f, 0.07f, 0.94f);
        }

        if (selectedLabel != null)
        {
            selectedLabel.SetActive(selected);
        }
    }

    private void BuildPortraitShowcase(Transform parent, Sprite portrait, bool isMage, Color accentColor, Color accentSoft)
    {
        GameObject portraitFrameObj = new GameObject("Portrait Frame");
        portraitFrameObj.transform.SetParent(parent, false);
        Image frameImage = portraitFrameObj.AddComponent<Image>();
        frameImage.sprite = whiteSprite;
        frameImage.color = new Color(0.045f, 0.055f, 0.10f, 0.96f);
        frameImage.raycastTarget = false;
        SetRect(frameImage.rectTransform, new Vector2(0.05f, 0.245f), new Vector2(0.95f, 0.965f), Vector2.zero, Vector2.zero);
        portraitFrameObj.AddComponent<RectMask2D>();

        CreatePortraitGlow(frameImage.transform, accentSoft, new Vector2(0f, -10f), new Vector2(330f, 330f));
        CreatePortraitGlow(frameImage.transform, isMage ? new Color(0.50f, 0.26f, 1f, 0.20f) : new Color(1f, 0.62f, 0.16f, 0.20f), new Vector2(42f, -48f), new Vector2(240f, 240f));

        if (portrait != null)
        {
            GameObject portraitObj = new GameObject("Portrait");
            portraitObj.transform.SetParent(frameImage.transform, false);
            Image portraitImage = portraitObj.AddComponent<Image>();
            portraitImage.sprite = portrait;
            portraitImage.color = Color.white;
            portraitImage.preserveAspect = true;
            portraitImage.raycastTarget = false;
            SetRect(portraitImage.rectTransform, new Vector2(0.035f, 0.025f), new Vector2(0.965f, 0.975f), Vector2.zero, Vector2.zero);
        }

        AddBorder(portraitFrameObj.transform, accentColor, 2f);
    }

    private void CreatePortraitGlow(Transform parent, Color color, Vector2 position, Vector2 size)
    {
        GameObject glow = new GameObject("Portrait Glow");
        glow.transform.SetParent(parent, false);
        Image glowImage = glow.AddComponent<Image>();
        glowImage.sprite = circleSprite;
        glowImage.color = color;
        glowImage.raycastTarget = false;
        SetRect(glowImage.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);
    }

    private void CreateScreenBackdrop()
    {
        GameObject backdrop = new GameObject("Character Select Backdrop");
        backdrop.transform.SetParent(canvas.transform, false);
        Image image = backdrop.AddComponent<Image>();
        image.sprite = whiteSprite;
        image.color = new Color(0.015f, 0.018f, 0.035f, 0.96f);
        SetRect(image.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
    }

    private Button CreateMenuButton(Transform parent, string label, Vector2 anchor, Color color, float width, float height, int fontSize)
    {
        GameObject buttonObj = new GameObject(label + " Button");
        buttonObj.transform.SetParent(parent, false);
        Image image = buttonObj.AddComponent<Image>();
        image.sprite = whiteSprite;
        image.color = color;
        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = image;
        ApplyButtonColors(button, color);
        AddBorder(buttonObj.transform, new Color(1f, 1f, 1f, 0.18f), 2f);

        RectTransform rect = buttonObj.GetComponent<RectTransform>();
        SetRect(rect, anchor, anchor, Vector2.zero, new Vector2(width, height));

        Text text = CreateText(buttonObj.transform, label, fontSize, TextAnchor.MiddleCenter, Color.white);
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

    private Sprite CreateCircleSprite()
    {
        const int size = 64;
        Texture2D texture = new Texture2D(size, size);
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(1f - (distance / radius));
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha * alpha));
            }
        }

        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
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

    private Text CreateText(Transform parent, string value, int size, TextAnchor alignment, Color color)
    {
        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(parent, false);
        Text text = textObject.AddComponent<Text>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        return text;
    }

    private Text CreateText(Transform parent, string value, int size, TextAnchor alignment, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta, bool bold)
    {
        Text text = CreateText(parent, value, size, alignment, color);
        text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        SetRect(text.rectTransform, anchorMin, anchorMax, anchoredPosition, sizeDelta);
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
