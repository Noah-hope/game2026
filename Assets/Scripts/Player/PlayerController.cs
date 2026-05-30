using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private const float Acceleration = 34f;
    private const float Deceleration = 42f;

    private GameManager gameManager;
    private CharacterStats stats;
    private Rigidbody2D body;
    private Camera mainCamera;
    private Vector2 moveInput;
    private float nextAttackTime;
    private float nextSkillTime;
    private bool isDashing;
    private bool controlsLocked;
    private bool isAttackWindingUp;
    private SpriteRenderer spriteRenderer;
    private Transform visualTransform;
    private Color baseColor;
    private Vector3 baseVisualScale;
    private Vector3 baseVisualLocalPosition;

    public void Initialize(GameManager manager, CharacterStats characterStats, PlayerHealth playerHealth)
    {
        gameManager = manager;
        stats = characterStats;
        body = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        SetupVisual();
        if (playerHealth != null)
        {
            playerHealth.SetSpriteRenderer(spriteRenderer);
        }
    }

    private void Update()
    {
        if (gameManager == null || gameManager.IsGameOver || Time.timeScale == 0f)
        {
            StopMovement();
            return;
        }

        ReadMovementInput();
        UpdateFacing(GetAimDirection());
        UpdateMovementVisual();

        if (Input.GetMouseButton(0))
        {
            TryAttack();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            TrySkill();
        }
    }

    private void FixedUpdate()
    {
        if (body == null || gameManager == null || gameManager.IsGameOver || Time.timeScale == 0f)
        {
            StopMovement();
            return;
        }

        if (isDashing)
        {
            return;
        }

        Vector2 targetVelocity = moveInput * stats.MoveSpeed;
        float speedChange = targetVelocity.sqrMagnitude > 0.001f ? Acceleration : Deceleration;
        body.velocity = Vector2.MoveTowards(body.velocity, targetVelocity, speedChange * Time.fixedDeltaTime);
        transform.position = GameUtility.ClampToArena(transform.position, 0.45f);
    }

    public string GetSkillCooldownText()
    {
        float remaining = nextSkillTime - Time.time;
        if (remaining <= 0f)
        {
            return "\u5c31\u7eea";
        }
        if (remaining <= 0f)
        {
            return "就绪";
        }

        return remaining.ToString("0.0") + "s";
    }

    public string GetAttackCooldownText()
    {
        float remaining = nextAttackTime - Time.time;
        if (remaining <= 0f)
        {
            return "\u5c31\u7eea";
        }
        if (remaining <= 0f)
        {
            return "就绪";
        }

        return remaining.ToString("0.0") + "s";
    }

    public float GetAttackCooldownRatio()
    {
        if (stats == null || stats.AttackCooldown <= 0f)
        {
            return 0f;
        }

        float remaining = nextAttackTime - Time.time;
        return Mathf.Clamp01(remaining / stats.AttackCooldown);
    }

    public float GetSkillCooldownRatio()
    {
        if (stats == null || stats.SkillCooldown <= 0f)
        {
            return 0f;
        }

        float remaining = nextSkillTime - Time.time;
        return Mathf.Clamp01(remaining / stats.SkillCooldown);
    }

    public void LockControls()
    {
        controlsLocked = true;
        StopMovement();
    }

    public void UnlockControls()
    {
        controlsLocked = false;
    }

    private void ReadMovementInput()
    {
        if (controlsLocked)
        {
            moveInput = Vector2.zero;
            return;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            horizontal -= 1f;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            horizontal += 1f;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            vertical -= 1f;
        }
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            vertical += 1f;
        }

        Vector2 rawInput = new Vector2(horizontal, vertical);
        moveInput = rawInput.sqrMagnitude > 1f ? rawInput.normalized : rawInput;
    }

    private void UpdateFacing(Vector2 aimDirection)
    {
        if (aimDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        if (spriteRenderer != null && Mathf.Abs(aimDirection.x) > 0.08f)
        {
            spriteRenderer.flipX = aimDirection.x < 0f;
        }
    }

    private void UpdateMovementVisual()
    {
        if (visualTransform == null || isAttackWindingUp || isDashing)
        {
            return;
        }

        float moveAmount = 0f;
        if (body != null && stats != null && stats.MoveSpeed > 0f)
        {
            moveAmount = Mathf.Clamp01(body.velocity.magnitude / stats.MoveSpeed);
        }

        if (moveAmount <= 0.01f)
        {
            visualTransform.localScale = Vector3.Lerp(visualTransform.localScale, baseVisualScale, 14f * Time.deltaTime);
            visualTransform.localPosition = Vector3.Lerp(visualTransform.localPosition, baseVisualLocalPosition, 14f * Time.deltaTime);
            return;
        }

        float pulse = Mathf.Sin(Time.time * 12f);
        float lift = Mathf.Abs(pulse) * 0.035f * moveAmount;
        Vector3 targetScale = new Vector3(
            baseVisualScale.x * (1f + pulse * 0.025f * moveAmount),
            baseVisualScale.y * (1f - pulse * 0.020f * moveAmount),
            baseVisualScale.z);
        Vector3 targetPosition = baseVisualLocalPosition + new Vector3(0f, lift, 0f);

        visualTransform.localScale = Vector3.Lerp(visualTransform.localScale, targetScale, 18f * Time.deltaTime);
        visualTransform.localPosition = Vector3.Lerp(visualTransform.localPosition, targetPosition, 18f * Time.deltaTime);
    }

    private void SetupVisual()
    {
        SpriteRenderer rootRenderer = GetComponent<SpriteRenderer>();
        if (rootRenderer == null)
        {
            return;
        }

        GameObject visualObject = new GameObject("Player Visual");
        visualObject.transform.SetParent(transform, false);
        visualObject.transform.localPosition = Vector3.zero;
        visualObject.transform.localScale = Vector3.one;

        SpriteRenderer visualRenderer = visualObject.AddComponent<SpriteRenderer>();
        visualRenderer.sprite = rootRenderer.sprite;
        visualRenderer.color = rootRenderer.color;
        visualRenderer.sortingOrder = rootRenderer.sortingOrder;
        visualRenderer.flipX = rootRenderer.flipX;
        visualRenderer.flipY = rootRenderer.flipY;

        rootRenderer.enabled = false;
        spriteRenderer = visualRenderer;
        visualTransform = visualObject.transform;
        baseColor = visualRenderer.color;
        baseVisualScale = visualTransform.localScale;
        baseVisualLocalPosition = visualTransform.localPosition;
    }

    private void StopMovement()
    {
        moveInput = Vector2.zero;
        if (body != null)
        {
            body.velocity = Vector2.zero;
        }
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime)
        {
            return;
        }

        nextAttackTime = Time.time + stats.AttackCooldown;
        if (!isAttackWindingUp)
        {
            StartCoroutine(AttackWindupRoutine(GetAimDirection()));
        }
    }

    private void TrySkill()
    {
        if (Time.time < nextSkillTime)
        {
            return;
        }

        nextSkillTime = Time.time + stats.SkillCooldown;

        if (stats.Type == CharacterType.Mage)
        {
            StartCoroutine(SkillWindupRoutine(GetAimDirection(), GetMouseWorldPosition()));
        }
        else
        {
            Vector2 direction = GetAimDirection();
            StartCoroutine(SkillWindupRoutine(direction, Vector3.zero));
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        return GameUtility.GetMouseWorldPosition(mainCamera);
    }

    private Vector2 GetAimDirection()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        Vector2 direction = mouseWorldPosition - transform.position;
        if (direction.sqrMagnitude < 0.001f)
        {
            direction = Vector2.right;
        }

        return direction.normalized;
    }

    private void CastArcaneRain(Vector3 center)
    {
        GameObject rainObject = new GameObject(stats.SkillName);
        rainObject.transform.position = center;

        ArcaneRainArea rainArea = rainObject.AddComponent<ArcaneRainArea>();
        rainArea.Initialize(center,
            stats.SkillRadius + stats.SkillRadiusBonus,
            stats.SkillDuration + stats.SkillDurationBonus,
            stats.RainInterval * stats.SkillTickIntervalMultiplier,
            stats.SkillDamage,
            stats.RainHitRadius,
            stats.RainSlowMultiplier);
    }

    private void FireProjectile(Vector2 direction, int damage, float speed, float lifetime, Color color, float size)
    {
        GameObject projectileObject = new GameObject(stats.AttackName);
        projectileObject.transform.position = transform.position + (Vector3)(direction * 0.55f);
        float finalSize = size * stats.ProjectileScaleMultiplier;
        projectileObject.transform.localScale = new Vector3(finalSize, finalSize, 1f);

        SpriteRenderer renderer = projectileObject.AddComponent<SpriteRenderer>();
        Sprite attackSprite = GameData.GetAttackSprite(stats.Type);
        renderer.sprite = attackSprite != null ? attackSprite : GameData.GetSquareSprite();
        renderer.color = attackSprite != null ? Color.white : color;
        renderer.sortingOrder = 3;
        if (attackSprite != null)
        {
            float spriteScale = stats.ProjectileScaleMultiplier;
            projectileObject.transform.localScale = stats.Type == CharacterType.Warrior
                ? new Vector3(0.75f * spriteScale, 0.75f * spriteScale, 1f)
                : new Vector3(0.68f * spriteScale, 0.68f * spriteScale, 1f);
        }

        Rigidbody2D projectileBody = projectileObject.AddComponent<Rigidbody2D>();
        projectileBody.gravityScale = 0f;
        projectileBody.freezeRotation = true;
        projectileBody.interpolation = RigidbodyInterpolation2D.Interpolate;
        projectileBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        BoxCollider2D collider = projectileObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        if (attackSprite != null)
        {
            Vector2 baseCollider = stats.Type == CharacterType.Warrior ? new Vector2(0.9f, 0.45f) : new Vector2(0.5f, 0.5f);
            collider.size = baseCollider * stats.ProjectileScaleMultiplier;
        }

        Projectile projectile = projectileObject.AddComponent<Projectile>();
        projectile.Initialize(direction, speed, damage, lifetime, color);
        if (stats.ProjectilePierce)
        {
            projectile.SetPierce(true);
        }
    }

    private IEnumerator AttackWindupRoutine(Vector2 direction)
    {
        isAttackWindingUp = true;
        yield return PlayWindupVisual(direction, 0.08f, 0.05f, new Color(1f, 1f, 1f, 1f));

        int count = stats.AttackProjectileCount;
        float spreadAngle = stats.AttackSpreadAngle;
        for (int i = 0; i < count; i++)
        {
            float offsetAngle = (i - (count - 1f) * 0.5f) * spreadAngle;
            Vector2 spreadDir = Quaternion.Euler(0f, 0f, offsetAngle) * direction;
            FireProjectile(spreadDir, stats.AttackDamage, stats.BulletSpeed, stats.BulletLifetime, stats.BulletColor, 0.32f);
        }

        isAttackWindingUp = false;
    }

    private IEnumerator SkillWindupRoutine(Vector2 direction, Vector3 targetPosition)
    {
        yield return PlayWindupVisual(direction, 0.14f, 0.07f, new Color(0.75f, 0.85f, 1f, 1f));

        if (stats.Type == CharacterType.Mage)
        {
            CastArcaneRain(targetPosition);
        }
        else
        {
            StartCoroutine(DashSlash(direction));
        }
    }

    private IEnumerator PlayWindupVisual(Vector2 direction, float windupTime, float releaseTime, Color flashColor)
    {
        if (visualTransform == null)
        {
            yield break;
        }

        Vector3 startScale = baseVisualScale;
        Vector3 squashScale = new Vector3(baseVisualScale.x * 0.90f, baseVisualScale.y * 1.08f, baseVisualScale.z);
        Vector3 releaseScale = new Vector3(baseVisualScale.x * 1.12f, baseVisualScale.y * 0.92f, baseVisualScale.z);
        float elapsed = 0f;
        Vector3 windupOffset = -(Vector3)(direction.normalized * 0.08f);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = flashColor;
        }

        while (elapsed < windupTime)
        {
            float t = SmoothStep01(elapsed / windupTime);
            visualTransform.localScale = Vector3.Lerp(startScale, squashScale, t);
            visualTransform.localPosition = Vector3.Lerp(baseVisualLocalPosition, baseVisualLocalPosition + windupOffset, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        CombatEffectFactory.CreateCircleEffect(transform.position + (Vector3)(direction.normalized * 0.55f), flashColor, 0.32f, 0.08f, 4);

        elapsed = 0f;
        Vector3 releaseStartPosition = visualTransform.localPosition;
        while (elapsed < releaseTime)
        {
            float t = SmoothStep01(elapsed / releaseTime);
            visualTransform.localScale = Vector3.Lerp(releaseScale, baseVisualScale, t);
            visualTransform.localPosition = Vector3.Lerp(releaseStartPosition, baseVisualLocalPosition, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        visualTransform.localScale = baseVisualScale;
        visualTransform.localPosition = baseVisualLocalPosition;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = baseColor;
        }
    }

    private float SmoothStep01(float value)
    {
        value = Mathf.Clamp01(value);
        return value * value * (3f - 2f * value);
    }

    private IEnumerator DashSlash(Vector2 direction)
    {
        isDashing = true;
        float elapsed = 0f;
        float dashSpeed = stats.DashDistance / Mathf.Max(0.01f, stats.DashDuration);
        float hitRadius = 0.75f;
        float nextAfterImageTime = 0f;
        float nextTrailTime = 0f;
        HashSet<EnemyController> hitEnemies = new HashSet<EnemyController>();

        Vector3 startPos = transform.position;
        CreateDashChargeEffect(startPos);
        CreateDashStartFlash(startPos, direction);
        if (gameManager != null)
        {
            gameManager.ShakeCamera(0.1f, 0.12f);
        }

        while (elapsed < stats.DashDuration)
        {
            body.velocity = direction * dashSpeed;

            if (elapsed >= nextAfterImageTime)
            {
                nextAfterImageTime = elapsed + 0.04f;
                CreateDashAfterImage(transform.position, spriteRenderer != null ? spriteRenderer.sprite : null);
            }

            if (elapsed >= nextTrailTime)
            {
                nextTrailTime = elapsed + 0.05f;
                CreateDashSlashTrail(transform.position, direction);
            }

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, hitRadius);
            for (int i = 0; i < hits.Length; i++)
            {
                EnemyController enemy = hits[i].GetComponent<EnemyController>();
                if (enemy != null && !hitEnemies.Contains(enemy))
                {
                    hitEnemies.Add(enemy);
                    enemy.TakeDamage(stats.SkillDamage);
                    CreateDashHitEffect(enemy.transform.position, direction);
                    if (gameManager != null)
                    {
                        gameManager.ShakeCamera(0.06f, 0.08f);
                    }
                    if (stats.DashHealOnHit > 0 && gameManager != null && gameManager.PlayerHealth != null)
                    {
                        gameManager.PlayerHealth.Heal(stats.DashHealOnHit);
                    }
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        body.velocity = Vector2.zero;
        isDashing = false;

        CreateDashEndBurst(transform.position, direction, stats.DashEndExplosion);

        if (stats.DashEndExplosion)
        {
            float explosionRadius = 1.4f;
            CombatEffectFactory.CreateCircleEffect(transform.position, new Color(1f, 0.3f, 0f, 0.75f), explosionRadius, 0.25f, 6);
            Collider2D[] expHits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
            for (int i = 0; i < expHits.Length; i++)
            {
                EnemyController enemy = expHits[i].GetComponent<EnemyController>();
                if (enemy != null && !hitEnemies.Contains(enemy))
                {
                    hitEnemies.Add(enemy);
                    enemy.TakeDamage(stats.SkillDamage);
                    if (stats.DashHealOnHit > 0 && gameManager != null && gameManager.PlayerHealth != null)
                    {
                        gameManager.PlayerHealth.Heal(stats.DashHealOnHit);
                    }
                }
            }
        }

        if (stats.BattleFrenzyDuration > 0f && hitEnemies.Count > 0)
        {
            StartCoroutine(BattleFrenzyRoutine(stats.BattleFrenzyDuration));
        }

        if (gameManager != null)
        {
            gameManager.ShakeCamera(0.14f, 0.2f);
        }
    }

    private void CreateDashChargeEffect(Vector3 position)
    {
        CombatEffectFactory.CreateCircleEffect(position, new Color(1f, 0.38f, 0.06f, 0.78f), 0.95f, 0.14f, 6);
        CombatEffectFactory.CreateCircleEffect(position, new Color(1f, 0.88f, 0.48f, 0.86f), 0.42f, 0.09f, 7);
    }

    private void CreateDashStartFlash(Vector3 position, Vector2 direction)
    {
        Vector2 dashDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector2.right;
        GameObject flash = new GameObject("Dash Slash Start Flash");
        flash.transform.position = position + (Vector3)(dashDirection * 0.35f);
        flash.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dashDirection.y, dashDirection.x) * Mathf.Rad2Deg);
        flash.transform.localScale = new Vector3(1.1f, 0.20f, 1f);

        SpriteRenderer renderer = flash.AddComponent<SpriteRenderer>();
        renderer.sprite = GameData.GetSquareSprite();
        renderer.color = new Color(1f, 0.92f, 0.65f, 0.85f);
        renderer.sortingOrder = 7;

        TemporaryEffect effect = flash.AddComponent<TemporaryEffect>();
        effect.Initialize(0.10f, true, 1.45f);

        CreateDashPreviewLine(position, dashDirection);
    }

    private void CreateDashPreviewLine(Vector3 position, Vector2 direction)
    {
        GameObject line = new GameObject("Dash Slash Preview Line");
        line.transform.position = position + (Vector3)(direction * 1.15f);
        line.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        line.transform.localScale = new Vector3(2.35f, 0.055f, 1f);

        SpriteRenderer renderer = line.AddComponent<SpriteRenderer>();
        renderer.sprite = GameData.GetSquareSprite();
        renderer.color = new Color(1f, 0.42f, 0.08f, 0.62f);
        renderer.sortingOrder = 6;

        TemporaryEffect effect = line.AddComponent<TemporaryEffect>();
        effect.Initialize(0.11f, true, 1.12f);
    }

    private void CreateDashAfterImage(Vector3 position, Sprite sprite)
    {
        GameObject afterImage = new GameObject("Dash AfterImage");
        afterImage.transform.position = position;

        SpriteRenderer renderer = afterImage.AddComponent<SpriteRenderer>();
        if (sprite != null)
        {
            renderer.sprite = sprite;
        }
        else
        {
            renderer.sprite = GameData.GetSquareSprite();
            afterImage.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
        }
        renderer.color = new Color(1f, 0.30f, 0.08f, 0.55f);
        renderer.sortingOrder = 1;
        if (spriteRenderer != null)
        {
            renderer.flipX = spriteRenderer.flipX;
        }

        TemporaryEffect effect = afterImage.AddComponent<TemporaryEffect>();
        effect.Initialize(0.30f, true, 0.88f);
    }

    private void CreateDashSlashTrail(Vector3 position, Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Vector3 side = new Vector3(-direction.y, direction.x, 0f);

        CreateSlashTrail(position + side * 0.10f, angle + 3f, new Vector3(2.35f, 0.18f, 1f), new Color(1f, 0.38f, 0.05f, 0.74f), 0.23f, 4, 0.72f);
        CreateSlashTrail(position - side * 0.10f, angle - 5f, new Vector3(1.80f, 0.12f, 1f), new Color(1f, 0.72f, 0.16f, 0.58f), 0.18f, 5, 0.62f);
        CreateSlashTrail(position + (Vector3)(direction.normalized * 0.20f), angle, new Vector3(1.25f, 0.055f, 1f), new Color(1f, 0.92f, 0.50f, 0.78f), 0.13f, 6, 0.52f);
    }

    private void CreateDashHitEffect(Vector3 position, Vector2 direction)
    {
        CombatEffectFactory.CreateCircleEffect(position, new Color(1f, 0.74f, 0.16f, 0.88f), 0.55f, 0.10f, 7);
        CombatEffectFactory.CreateCircleEffect(position, new Color(1f, 1f, 1f, 0.72f), 0.28f, 0.075f, 8);

        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        for (int i = 0; i < 5; i++)
        {
            float sparkAngle = baseAngle + Random.Range(-55f, 55f);
            CreateHitSpark(position, sparkAngle, Random.Range(0.24f, 0.48f), new Color(1f, Random.Range(0.55f, 0.94f), 0.12f, 0.88f));
        }
    }

    private void CreateDashEndBurst(Vector3 position, Vector2 direction, bool empowered)
    {
        int arcCount = empowered ? 13 : 9;
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float totalSpread = empowered ? 108f : 82f;
        float burstScale = empowered ? 1.25f : 1f;

        for (int i = 0; i < arcCount; i++)
        {
            float t = (i - (arcCount - 1f) * 0.5f) / ((arcCount - 1f) * 0.5f);
            float arcAngle = baseAngle + t * totalSpread * 0.5f;
            float radians = arcAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * (0.42f + Mathf.Abs(t) * 0.12f) * burstScale;

            GameObject arc = new GameObject("Slash Arc");
            arc.transform.position = position + offset;
            arc.transform.rotation = Quaternion.Euler(0f, 0f, arcAngle);
            arc.transform.localScale = new Vector3((0.82f - Mathf.Abs(t) * 0.15f) * burstScale, 0.13f * burstScale, 1f);

            SpriteRenderer renderer = arc.AddComponent<SpriteRenderer>();
            renderer.sprite = GameData.GetSquareSprite();
            float alpha = 0.90f - Mathf.Abs(t) * 0.42f;
            renderer.color = empowered ? new Color(1f, 0.42f, 0.05f, alpha) : new Color(1f, 0.70f, 0.15f, alpha);
            renderer.sortingOrder = 7;

            TemporaryEffect effect = arc.AddComponent<TemporaryEffect>();
            effect.Initialize(empowered ? 0.34f : 0.27f, true, 1.28f);
        }

        CombatEffectFactory.CreateCircleEffect(position, new Color(1f, 0.45f, 0f, 0.72f), 1.05f * burstScale, 0.24f, 6);
        CombatEffectFactory.CreateCircleEffect(position + (Vector3)(direction.normalized * 0.38f), new Color(1f, 0.82f, 0.25f, 0.62f), 0.78f * burstScale, 0.17f, 8);
        if (empowered)
        {
            CombatEffectFactory.CreateCircleEffect(position, new Color(1f, 0.18f, 0f, 0.52f), 1.75f, 0.34f, 6);
        }
    }

    private void CreateSlashTrail(Vector3 position, float angle, Vector3 scale, Color color, float lifetime, int sortingOrder, float finalScaleMultiplier)
    {
        GameObject trail = new GameObject("Dash Slash Trail");
        trail.transform.position = position;
        trail.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        trail.transform.localScale = scale;

        SpriteRenderer renderer = trail.AddComponent<SpriteRenderer>();
        renderer.sprite = GameData.GetSquareSprite();
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;

        TemporaryEffect effect = trail.AddComponent<TemporaryEffect>();
        effect.Initialize(lifetime, true, finalScaleMultiplier);
    }

    private void CreateHitSpark(Vector3 position, float angle, float length, Color color)
    {
        GameObject spark = new GameObject("Dash Slash Spark");
        spark.transform.position = position;
        spark.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        spark.transform.localScale = new Vector3(length, 0.055f, 1f);

        SpriteRenderer renderer = spark.AddComponent<SpriteRenderer>();
        renderer.sprite = GameData.GetSquareSprite();
        renderer.color = color;
        renderer.sortingOrder = 8;

        Vector3 direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0f);
        TemporaryEffect effect = spark.AddComponent<TemporaryEffect>();
        effect.Initialize(0.13f, true, 0.35f);
        effect.SetMotion(direction * 0.7f, Random.Range(-60f, 60f));
    }

    private IEnumerator BattleFrenzyRoutine(float duration)
    {
        float originalCooldown = stats.AttackCooldown;
        stats.AttackCooldown *= 0.5f;
        yield return new WaitForSeconds(duration);
        stats.AttackCooldown = originalCooldown;
    }

}
