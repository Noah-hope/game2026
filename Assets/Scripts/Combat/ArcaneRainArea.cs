using System.Collections.Generic;
using UnityEngine;

public class ArcaneRainArea : MonoBehaviour
{
    private Vector3 center;
    private float radius;
    private float duration;
    private float rainInterval;
    private int damage;
    private float hitRadius;
    private float slowMultiplier = 1f;
    private float elapsed;
    private float nextRainTime;
    private bool isFading;
    private float fadeElapsed;
    private bool endParticlesCreated;
    private const float FadeDuration = 0.35f;
    private const float CastExpandDuration = 0.12f;
    private SpriteRenderer areaRenderer;
    private SpriteRenderer outerRingRenderer;
    private SpriteRenderer innerGlowRenderer;
    private Transform outerRingRoot;
    private Transform innerRingRoot;
    private HashSet<EnemyController> slowedEnemies = new HashSet<EnemyController>();

    public void Initialize(Vector3 areaCenter, float areaRadius, float areaDuration, float interval, int rainDamage, float rainHitRadius, float slow = 1f)
    {
        center = areaCenter;
        radius = areaRadius;
        duration = areaDuration;
        rainInterval = interval;
        damage = rainDamage;
        hitRadius = rainHitRadius;
        slowMultiplier = slow;

        transform.position = center;
        transform.localScale = Vector3.one * 0.1f;

        areaRenderer = gameObject.AddComponent<SpriteRenderer>();
        areaRenderer.sprite = GameData.GetCircleSprite();
        areaRenderer.color = new Color(0.32f, 0.2f, 1f, 0.34f);
        areaRenderer.sortingOrder = -2;

        outerRingRenderer = CreateCircleLayer("Arcane Rain Outer Ring", 1.08f, new Color(0.25f, 0.70f, 1f, 0.42f), -3);
        innerGlowRenderer = CreateCircleLayer("Arcane Rain Inner Glow", 0.70f, new Color(0.68f, 0.28f, 1f, 0.20f), -1);
        outerRingRoot = CreateRingEffect("Arcane Rain Segmented Outer Ring", 18, 0.52f, 0.22f, 0.018f, new Color(0.35f, 0.82f, 1f, 0.82f), 2);
        innerRingRoot = CreateRingEffect("Arcane Rain Segmented Inner Ring", 12, 0.31f, 0.14f, 0.014f, new Color(0.82f, 0.40f, 1f, 0.68f), 3);

        CreateCastFlash();
    }

    private void Update()
    {
        if (isFading)
        {
            UpdateFadeOut();
            return;
        }

        elapsed += Time.deltaTime;
        nextRainTime -= Time.deltaTime;

        float castT = Mathf.Clamp01(elapsed / CastExpandDuration);
        float expand = castT < 1f ? Mathf.Lerp(0.22f, 1.08f, SmoothStep01(castT)) : 1f;
        float pulse = 1f + Mathf.Sin(elapsed * 4.5f) * 0.045f;
        transform.localScale = new Vector3(radius * 2f * pulse * expand, radius * 2f * pulse * expand, 1f);

        if (outerRingRoot != null)
        {
            outerRingRoot.Rotate(0f, 0f, 28f * Time.deltaTime);
        }
        if (innerRingRoot != null)
        {
            innerRingRoot.Rotate(0f, 0f, -18f * Time.deltaTime);
        }
        if (outerRingRenderer != null)
        {
            Color ringColor = outerRingRenderer.color;
            ringColor.a = 0.36f + Mathf.Sin(elapsed * 7.5f) * 0.12f;
            outerRingRenderer.color = ringColor;
        }
        if (innerGlowRenderer != null)
        {
            Color glowColor = innerGlowRenderer.color;
            glowColor.a = 0.17f + Mathf.Sin(elapsed * 5.5f) * 0.08f;
            innerGlowRenderer.color = glowColor;
        }

        if (nextRainTime <= 0f)
        {
            nextRainTime = rainInterval;
            StrikeRandomPoint();
            DamageEnemiesInArea();
        }

        ApplySlowToEnemiesInArea();

        if (elapsed >= duration)
        {
            ResetAllSlowed();
            CreateEndParticles();
            isFading = true;
        }
    }

    private void OnDestroy()
    {
        ResetAllSlowed();
    }

    private void ResetAllSlowed()
    {
        foreach (EnemyController enemy in slowedEnemies)
        {
            if (enemy != null)
            {
                enemy.SetExternalSpeedMultiplier(1f);
            }
        }
        slowedEnemies.Clear();
    }

    private void ApplySlowToEnemiesInArea()
    {
        if (slowMultiplier >= 1f) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
        HashSet<EnemyController> currentInArea = new HashSet<EnemyController>();

        for (int i = 0; i < hits.Length; i++)
        {
            EnemyController enemy = hits[i].GetComponent<EnemyController>();
            if (enemy != null)
            {
                currentInArea.Add(enemy);
                enemy.SetExternalSpeedMultiplier(slowMultiplier);
            }
        }

        HashSet<EnemyController> toRestore = new HashSet<EnemyController>();
        foreach (EnemyController enemy in slowedEnemies)
        {
            if (enemy != null && !currentInArea.Contains(enemy))
            {
                toRestore.Add(enemy);
            }
        }

        foreach (EnemyController enemy in toRestore)
        {
            if (enemy != null)
            {
                enemy.SetExternalSpeedMultiplier(1f);
            }
            slowedEnemies.Remove(enemy);
        }

        foreach (EnemyController enemy in currentInArea)
        {
            slowedEnemies.Add(enemy);
        }
    }

    private void StrikeRandomPoint()
    {
        Vector2 randomOffset = Random.insideUnitCircle * radius;
        Vector3 strikePosition = center + new Vector3(randomOffset.x, randomOffset.y, 0f);

        Color color = Random.value > 0.5f ? new Color(0.2f, 0.65f, 1f, 0.85f) : new Color(0.65f, 0.25f, 1f, 0.85f);
        CombatEffectFactory.CreateSpriteEffect(strikePosition, GameData.GetEffectSprite("arcane_rain_hit"), color, hitRadius * 2f, 0.16f, 4);
        CreateLightningFlash(strikePosition, color);
        ShakeCamera(0.045f, 0.035f);
    }

    private void DamageEnemiesInArea()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
        for (int i = 0; i < hits.Length; i++)
        {
            EnemyController enemy = hits[i].GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
    }

    private SpriteRenderer CreateCircleLayer(string layerName, float scale, Color color, int sortingOrder)
    {
        GameObject layer = new GameObject(layerName);
        layer.transform.SetParent(transform, false);
        layer.transform.localScale = new Vector3(scale, scale, 1f);

        SpriteRenderer renderer = layer.AddComponent<SpriteRenderer>();
        renderer.sprite = GameData.GetCircleSprite();
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }

    private void CreateLightningFlash(Vector3 strikePosition, Color color)
    {
        CreateBeam(strikePosition + new Vector3(0f, 0.55f, 0f), 0.10f, 1.55f, new Color(color.r, color.g, color.b, 0.76f), 5, 0.14f);
        CreateBeam(strikePosition + new Vector3(0f, 0.58f, 0f), 0.035f, 1.85f, new Color(0.92f, 0.96f, 1f, 0.88f), 6, 0.10f);

        CombatEffectFactory.CreateCircleEffect(strikePosition, new Color(0.78f, 0.88f, 1f, 0.58f), hitRadius * 1.25f, 0.12f, 5);
        CombatEffectFactory.CreateCircleEffect(strikePosition, new Color(0.65f, 0.25f, 1f, 0.58f), hitRadius * 0.82f, 0.16f, 6);
    }

    private void UpdateFadeOut()
    {
        fadeElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(fadeElapsed / FadeDuration);
        SetRendererAlpha(areaRenderer, Mathf.Lerp(0.34f, 0f, t));
        SetRendererAlpha(outerRingRenderer, Mathf.Lerp(0.42f, 0f, t));
        SetRendererAlpha(innerGlowRenderer, Mathf.Lerp(0.20f, 0f, t));
        SetRenderersAlpha(outerRingRoot, Mathf.Lerp(0.82f, 0f, t));
        SetRenderersAlpha(innerRingRoot, Mathf.Lerp(0.68f, 0f, t));
        transform.localScale = Vector3.Lerp(new Vector3(radius * 2f, radius * 2f, 1f), new Vector3(radius * 2.25f, radius * 2.25f, 1f), t);
        if (outerRingRoot != null)
        {
            outerRingRoot.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.72f, t);
            outerRingRoot.Rotate(0f, 0f, 60f * Time.deltaTime);
        }

        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }

    private Transform CreateRingEffect(string ringName, int segmentCount, float localRadius, float segmentLength, float thickness, Color color, int sortingOrder)
    {
        GameObject ring = new GameObject(ringName);
        ring.transform.SetParent(transform, false);

        for (int i = 0; i < segmentCount; i++)
        {
            float angle = (360f / segmentCount) * i;
            float radians = angle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * localRadius;

            GameObject segment = new GameObject("Ring Segment");
            segment.transform.SetParent(ring.transform, false);
            segment.transform.localPosition = offset;
            segment.transform.localRotation = Quaternion.Euler(0f, 0f, angle + 90f);
            segment.transform.localScale = new Vector3(segmentLength, thickness, 1f);

            SpriteRenderer renderer = segment.AddComponent<SpriteRenderer>();
            renderer.sprite = GameData.GetSquareSprite();
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
        }

        return ring.transform;
    }

    private void CreateCastFlash()
    {
        CombatEffectFactory.CreateCircleEffect(center, new Color(0.95f, 0.88f, 1f, 0.92f), radius * 0.95f, 0.09f, 7);
        CombatEffectFactory.CreateCircleEffect(center, new Color(0.55f, 0.22f, 1f, 0.62f), radius * 1.45f, 0.18f, 6);
        for (int i = 0; i < 10; i++)
        {
            float angle = (360f / 10f) * i;
            CreateArcaneParticle(center, angle, 0.32f, 0.22f, new Color(0.75f, 0.45f, 1f, 0.82f));
        }
    }

    private void CreateEndParticles()
    {
        if (endParticlesCreated) return;
        endParticlesCreated = true;

        int count = 18;
        for (int i = 0; i < count; i++)
        {
            float angle = (360f / count) * i + Random.Range(-6f, 6f);
            CreateArcaneParticle(center, angle, Random.Range(0.7f, 1.15f), Random.Range(0.26f, 0.38f), new Color(0.72f, 0.30f, 1f, 0.78f));
        }
    }

    private void CreateArcaneParticle(Vector3 position, float angle, float speed, float lifetime, Color color)
    {
        float radians = angle * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f);
        GameObject particle = new GameObject("Arcane Rain Particle");
        particle.transform.position = position + direction * (radius * 0.18f);
        particle.transform.localScale = new Vector3(0.10f, 0.10f, 1f);

        SpriteRenderer renderer = particle.AddComponent<SpriteRenderer>();
        renderer.sprite = GameData.GetCircleSprite();
        renderer.color = color;
        renderer.sortingOrder = 7;

        TemporaryEffect effect = particle.AddComponent<TemporaryEffect>();
        effect.Initialize(lifetime, true, 0.45f);
        effect.SetMotion(direction * speed, Random.Range(-120f, 120f));
    }

    private void CreateBeam(Vector3 position, float width, float height, Color color, int sortingOrder, float lifetime)
    {
        GameObject beam = new GameObject("Arcane Rain Beam");
        beam.transform.position = position;
        beam.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(-4f, 4f));
        beam.transform.localScale = new Vector3(width, height, 1f);

        SpriteRenderer renderer = beam.AddComponent<SpriteRenderer>();
        renderer.sprite = GameData.GetSquareSprite();
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;

        TemporaryEffect effect = beam.AddComponent<TemporaryEffect>();
        effect.Initialize(lifetime, true, 0.55f);
    }

    private float SmoothStep01(float value)
    {
        value = Mathf.Clamp01(value);
        return value * value * (3f - 2f * value);
    }

    private void SetRendererAlpha(SpriteRenderer renderer, float alpha)
    {
        if (renderer == null) return;
        Color color = renderer.color;
        color.a = alpha;
        renderer.color = color;
    }

    private void SetRenderersAlpha(Transform root, float alpha)
    {
        if (root == null) return;
        SpriteRenderer[] renderers = root.GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            SetRendererAlpha(renderers[i], alpha);
        }
    }

    private void ShakeCamera(float duration, float strength)
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPaused)
        {
            GameManager.Instance.ShakeCamera(duration, strength);
        }
    }
}
