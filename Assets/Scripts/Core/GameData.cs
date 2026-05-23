using System.Collections.Generic;
using UnityEngine;

public static class GameData
{
    public const float PixelsPerUnit = 64f;

    public static CharacterType SelectedCharacter = CharacterType.Mage;

    private static Sprite squareSprite;
    private static Sprite circleSprite;
    private static readonly Dictionary<string, Sprite> runtimeSprites = new Dictionary<string, Sprite>();

    public static CharacterStats GetSelectedCharacterStats()
    {
        return GetCharacterStats(SelectedCharacter).Clone();
    }

    public static CharacterStats GetCharacterStats(CharacterType type)
    {
        if (type == CharacterType.Warrior)
        {
            return new CharacterStats
            {
                Type = CharacterType.Warrior,
                DisplayName = "Warrior",
                MaxHealth = 140,
                MoveSpeed = 4f,
                AttackName = "Sword Wave",
                AttackDamage = 30,
                AttackCooldown = 0.6f,
                BulletSpeed = 8f,
                BulletLifetime = 0.7f,
                SkillName = "Dash Slash",
                SkillDamage = 50,
                SkillCooldown = 6f,
                SkillRadius = 0f,
                SkillDuration = 0f,
                RainInterval = 0f,
                RainHitRadius = 0f,
                DashDistance = 3f,
                DashDuration = 0.15f,
                BodyColor = new Color(0.9f, 0.15f, 0.12f),
                BulletColor = new Color(1f, 0.92f, 0.15f)
            };
        }

        return new CharacterStats
        {
            Type = CharacterType.Mage,
            DisplayName = "Mage",
            MaxHealth = 80,
            MoveSpeed = 5f,
            AttackName = "Fireball",
            AttackDamage = 20,
            AttackCooldown = 0.4f,
            BulletSpeed = 10f,
            BulletLifetime = 2f,
            SkillName = "Arcane Rain",
            SkillDamage = 12,
            SkillCooldown = 7f,
            SkillRadius = 2.5f,
            SkillDuration = 3f,
            RainInterval = 0.25f,
            RainHitRadius = 0.45f,
            DashDistance = 0f,
            DashDuration = 0f,
            BodyColor = new Color(0.1f, 0.35f, 1f),
            BulletColor = new Color(1f, 0.45f, 0.05f)
        };
    }

    public static Sprite GetSquareSprite()
    {
        if (squareSprite != null)
        {
            return squareSprite;
        }

        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        squareSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return squareSprite;
    }

    public static Sprite GetCircleSprite()
    {
        if (circleSprite != null)
        {
            return circleSprite;
        }

        const int size = 64;
        Texture2D texture = new Texture2D(size, size);
        Vector2 centerPixel = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radiusPixel = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), centerPixel);
                texture.SetPixel(x, y, distance <= radiusPixel ? Color.white : new Color(1f, 1f, 1f, 0f));
            }
        }

        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        circleSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return circleSprite;
    }

    public static Sprite LoadRuntimeSprite(string resourcePath)
    {
        if (runtimeSprites.ContainsKey(resourcePath) && runtimeSprites[resourcePath] != null)
        {
            return runtimeSprites[resourcePath];
        }

        Sprite importedSprite = Resources.Load<Sprite>(resourcePath);
        if (importedSprite != null)
        {
            runtimeSprites[resourcePath] = importedSprite;
            return importedSprite;
        }

        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null)
        {
            return null;
        }

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Repeat;
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            PixelsPerUnit,
            0,
            SpriteMeshType.FullRect);
        runtimeSprites[resourcePath] = sprite;
        return sprite;
    }

    public static Sprite GetCharacterSprite(CharacterType type)
    {
        string path = type == CharacterType.Mage ? "Art/Sprites/mage" : "Art/Sprites/warrior";
        return LoadRuntimeSprite(path);
    }

    public static Sprite GetCharacterPortrait(CharacterType type)
    {
        string path = type == CharacterType.Mage ? "Art/Portraits/mage_portrait" : "Art/Portraits/warrior_portrait";
        return LoadRuntimeSprite(path);
    }

    public static Sprite GetEnemySprite(string enemyName)
    {
        if (enemyName == "Bat")
        {
            return LoadRuntimeSprite("Art/Sprites/bat");
        }

        return LoadRuntimeSprite("Art/Sprites/slime");
    }

    public static Sprite GetAttackSprite(CharacterType type)
    {
        if (type == CharacterType.Warrior)
        {
            return LoadRuntimeSprite("Art/Sprites/sword_wave");
        }

        return LoadRuntimeSprite("Art/Sprites/fireball");
    }

    public static Sprite GetEffectSprite(string effectName)
    {
        return LoadRuntimeSprite("Art/Sprites/" + effectName);
    }

    public static Sprite GetMapSprite(string spriteName)
    {
        return LoadRuntimeSprite("Art/Sprites/" + spriteName);
    }

    public static List<UpgradeOption> CreateUpgradeOptions()
    {
        return new List<UpgradeOption>
        {
            new UpgradeOption("\u666e\u653b\u4f24\u5bb3 +10", "Attack Damage +10", UpgradeType.AttackDamage),
            new UpgradeOption("\u6280\u80fd\u4f24\u5bb3 +5", "Skill Damage +5", UpgradeType.SkillDamage),
            new UpgradeOption("\u6700\u5927\u751f\u547d +20", "Max HP +20 and heal 20 HP", UpgradeType.MaxHealth),
            new UpgradeOption("\u79fb\u52a8\u901f\u5ea6 +10%", "Move Speed +10%", UpgradeType.MoveSpeed),
            new UpgradeOption("\u666e\u653b\u51b7\u5374 -10%", "Attack Cooldown -10%", UpgradeType.AttackCooldown),
            new UpgradeOption("\u751f\u547d\u6062\u590d +40", "Heal 40 HP instantly", UpgradeType.Heal)
        };
    }
}
