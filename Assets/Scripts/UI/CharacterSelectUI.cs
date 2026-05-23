using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    private Font font;

    private void Start()
    {
        Time.timeScale = 1f;
        font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        EnsureEventSystem();
        BuildUI();
    }

    private void BuildUI()
    {
        Canvas canvas = CreateCanvas("Character Select Canvas");

        Text title = CreateText(canvas.transform, "\u9009\u62e9\u89d2\u8272", 42, TextAnchor.MiddleCenter);
        SetRect(title.rectTransform, new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), Vector2.zero, new Vector2(600f, 80f));

        CreateCharacterButton(canvas.transform, CharacterType.Mage, new Vector2(-260f, -10f));
        CreateCharacterButton(canvas.transform, CharacterType.Warrior, new Vector2(260f, -10f));
    }

    private void CreateCharacterButton(Transform parent, CharacterType type, Vector2 position)
    {
        CharacterStats stats = GameData.GetCharacterStats(type);
        string description = type == CharacterType.Mage
            ? "\u6cd5\u5e08 Mage\nHP 80 / Speed 5\nFireball + Arcane Rain"
            : "\u6218\u58eb Warrior\nHP 140 / Speed 4\nSword Wave + Dash Slash";

        Button button = CreateCharacterCard(parent, description, stats.BodyColor, GameData.GetCharacterPortrait(type), type);
        SetRect(button.GetComponent<RectTransform>(), new Vector2(0.5f, 0.47f), new Vector2(0.5f, 0.47f), position, new Vector2(420f, 360f));
        button.onClick.AddListener(delegate
        {
            GameData.SelectedCharacter = type;
            SceneLoader.LoadGame();
        });
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
        GameObject buttonObject = new GameObject("Character Button");
        buttonObject.transform.SetParent(parent, false);
        Image image = buttonObject.AddComponent<Image>();
        image.color = color;
        Button button = buttonObject.AddComponent<Button>();

        Text text = CreateText(buttonObject.transform, label, 25, TextAnchor.MiddleCenter);
        text.color = Color.white;
        SetRect(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        return button;
    }

    private Button CreateCharacterCard(Transform parent, string label, Color color, Sprite portrait, CharacterType type)
    {
        GameObject buttonObject = new GameObject("Character Card");
        buttonObject.transform.SetParent(parent, false);
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(color.r * 0.55f, color.g * 0.55f, color.b * 0.55f, 0.92f);
        Button button = buttonObject.AddComponent<Button>();

        GameObject borderObject = new GameObject("Gold Border");
        borderObject.transform.SetParent(buttonObject.transform, false);
        Image borderImage = borderObject.AddComponent<Image>();
        borderImage.color = new Color(0.85f, 0.68f, 0.28f, 0.95f);
        borderImage.raycastTarget = false;
        SetRect(borderImage.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        GameObject innerObject = new GameObject("Inner Panel");
        innerObject.transform.SetParent(buttonObject.transform, false);
        Image innerImage = innerObject.AddComponent<Image>();
        innerImage.color = new Color(0.05f, 0.06f, 0.10f, 0.96f);
        innerImage.raycastTarget = false;
        SetRect(innerImage.rectTransform, new Vector2(0.025f, 0.025f), new Vector2(0.975f, 0.975f), Vector2.zero, Vector2.zero);

        if (portrait != null)
        {
            GameObject portraitFrameObject = new GameObject("Portrait Frame");
            portraitFrameObject.transform.SetParent(buttonObject.transform, false);
            Image portraitFrameImage = portraitFrameObject.AddComponent<Image>();
            portraitFrameImage.color = new Color(0.12f, 0.14f, 0.22f, 1f);
            portraitFrameImage.raycastTarget = false;
            SetRect(portraitFrameImage.rectTransform, new Vector2(0.08f, 0.33f), new Vector2(0.92f, 0.92f), Vector2.zero, Vector2.zero);

            GameObject portraitObject = new GameObject("Portrait");
            portraitObject.transform.SetParent(portraitFrameObject.transform, false);
            Image portraitImage = portraitObject.AddComponent<Image>();
            portraitImage.sprite = portrait;
            portraitImage.color = Color.white;
            portraitImage.preserveAspect = true;
            portraitImage.raycastTarget = false;
            SetRect(portraitImage.rectTransform, new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.96f), Vector2.zero, Vector2.zero);

            GameObject textPanelObject = new GameObject("Description Panel");
            textPanelObject.transform.SetParent(buttonObject.transform, false);
            Image textPanelImage = textPanelObject.AddComponent<Image>();
            textPanelImage.color = new Color(0f, 0f, 0f, 0.35f);
            textPanelImage.raycastTarget = false;
            SetRect(textPanelImage.rectTransform, new Vector2(0.06f, 0.05f), new Vector2(0.94f, 0.30f), Vector2.zero, Vector2.zero);

            Text text = CreateText(textPanelObject.transform, label, 23, TextAnchor.MiddleCenter);
            text.color = Color.white;
            SetRect(text.rectTransform, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.98f), Vector2.zero, Vector2.zero);
        }
        else
        {
            string portraitPath = type == CharacterType.Mage
                ? "Assets/Resources/Art/Portraits/mage_portrait.png"
                : "Assets/Resources/Art/Portraits/warrior_portrait.png";

            Text missingText = CreateText(buttonObject.transform, "\u7f3a\u5c11\u7acb\u7ed8\u6587\u4ef6\n" + portraitPath, 18, TextAnchor.MiddleCenter);
            missingText.color = new Color(1f, 0.88f, 0.45f);
            SetRect(missingText.rectTransform, new Vector2(0.08f, 0.36f), new Vector2(0.92f, 0.88f), Vector2.zero, Vector2.zero);

            Text text = CreateText(buttonObject.transform, label, 23, TextAnchor.MiddleCenter);
            text.color = Color.white;
            SetRect(text.rectTransform, new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.30f), Vector2.zero, Vector2.zero);
        }

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
