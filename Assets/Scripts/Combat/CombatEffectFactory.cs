using UnityEngine;
using UnityEngine.UI;

public static class CombatEffectFactory
{
    public static void CreateCircleEffect(Vector3 position, Color color, float size, float lifetime, int sortingOrder)
    {
        CreateEffect(position, GameData.GetCircleSprite(), color, size, lifetime, sortingOrder);
    }

    public static void CreateSpriteEffect(Vector3 position, Sprite sprite, Color fallbackColor, float size, float lifetime, int sortingOrder)
    {
        CreateEffect(position, sprite != null ? sprite : GameData.GetCircleSprite(), sprite != null ? Color.white : fallbackColor, size, lifetime, sortingOrder);
    }

    public static void CreateDamageText(Vector3 position, string text, Color color)
    {
        GameObject textObject = new GameObject("Damage Text");
        textObject.transform.position = new Vector3(position.x, position.y, 0f);
        textObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        Canvas canvas = textObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 5;

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(200f, 50f);

        DamageTextEffect effect = textObject.AddComponent<DamageTextEffect>();
        effect.Initialize(text, color);
    }

    private static void CreateEffect(Vector3 position, Sprite sprite, Color color, float size, float lifetime, int sortingOrder)
    {
        GameObject effectObject = new GameObject("Combat Effect");
        effectObject.transform.position = position;

        SpriteRenderer renderer = effectObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        float spriteSize = sprite != null ? Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y) : 1f;
        float scale = spriteSize > 0f ? size / spriteSize : size;
        effectObject.transform.localScale = new Vector3(scale, scale, 1f);

        TemporaryEffect effect = effectObject.AddComponent<TemporaryEffect>();
        effect.Initialize(lifetime);
    }
}
