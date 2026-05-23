using UnityEngine;

public class ArcaneRainArea : MonoBehaviour
{
    private Vector3 center;
    private float radius;
    private float duration;
    private float rainInterval;
    private int damage;
    private float hitRadius;
    private float elapsed;
    private float nextRainTime;

    public void Initialize(Vector3 areaCenter, float areaRadius, float areaDuration, float interval, int rainDamage, float rainHitRadius)
    {
        center = areaCenter;
        radius = areaRadius;
        duration = areaDuration;
        rainInterval = interval;
        damage = rainDamage;
        hitRadius = rainHitRadius;

        transform.position = center;

        SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = GameData.GetCircleSprite();
        renderer.color = new Color(0.35f, 0.18f, 1f, 0.28f);
        renderer.sortingOrder = -1;
        transform.localScale = new Vector3(radius * 2f, radius * 2f, 1f);
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        nextRainTime -= Time.deltaTime;

        if (nextRainTime <= 0f)
        {
            nextRainTime = rainInterval;
            StrikeRandomPoint();
        }

        if (elapsed >= duration)
        {
            Destroy(gameObject);
        }
    }

    private void StrikeRandomPoint()
    {
        Vector2 randomOffset = Random.insideUnitCircle * radius;
        Vector3 strikePosition = center + new Vector3(randomOffset.x, randomOffset.y, 0f);

        CreateStrikeEffect(strikePosition);

        Collider2D[] hits = Physics2D.OverlapCircleAll(strikePosition, hitRadius);
        for (int i = 0; i < hits.Length; i++)
        {
            EnemyController enemy = hits[i].GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
    }

    private void CreateStrikeEffect(Vector3 position)
    {
        Color color = Random.value > 0.5f ? new Color(0.2f, 0.65f, 1f, 0.85f) : new Color(0.65f, 0.25f, 1f, 0.85f);
        CombatEffectFactory.CreateSpriteEffect(position, GameData.GetEffectSprite("arcane_rain_hit"), color, hitRadius * 2f, 0.15f, 4);
    }
}
