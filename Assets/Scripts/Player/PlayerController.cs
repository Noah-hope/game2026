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
        if (body == null || isDashing || gameManager == null || gameManager.IsGameOver || Time.timeScale == 0f)
        {
            StopMovement();
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
            return "Ready";
        }

        return remaining.ToString("0.0") + "s";
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
        rainArea.Initialize(center, stats.SkillRadius, stats.SkillDuration, stats.RainInterval, stats.SkillDamage, stats.RainHitRadius);
    }

    private void FireProjectile(Vector2 direction, int damage, float speed, float lifetime, Color color, float size)
    {
        GameObject projectileObject = new GameObject(stats.AttackName);
        projectileObject.transform.position = transform.position + (Vector3)(direction * 0.55f);
        projectileObject.transform.localScale = new Vector3(size, size, 1f);

        SpriteRenderer renderer = projectileObject.AddComponent<SpriteRenderer>();
        Sprite attackSprite = GameData.GetAttackSprite(stats.Type);
        renderer.sprite = attackSprite != null ? attackSprite : GameData.GetSquareSprite();
        renderer.color = attackSprite != null ? Color.white : color;
        renderer.sortingOrder = 3;
        if (attackSprite != null)
        {
            projectileObject.transform.localScale = stats.Type == CharacterType.Warrior
                ? new Vector3(0.75f, 0.75f, 1f)
                : new Vector3(0.68f, 0.68f, 1f);
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
            collider.size = stats.Type == CharacterType.Warrior ? new Vector2(0.9f, 0.45f) : new Vector2(0.5f, 0.5f);
        }

        Projectile projectile = projectileObject.AddComponent<Projectile>();
        projectile.Initialize(direction, speed, damage, lifetime, color);
    }

    private IEnumerator AttackWindupRoutine(Vector2 direction)
    {
        isAttackWindingUp = true;
        yield return PlayWindupVisual(direction, 0.08f, 0.05f, new Color(1f, 1f, 1f, 1f));
        FireProjectile(direction, stats.AttackDamage, stats.BulletSpeed, stats.BulletLifetime, stats.BulletColor, 0.32f);
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
        HashSet<EnemyController> hitEnemies = new HashSet<EnemyController>();

        while (elapsed < stats.DashDuration)
        {
            body.velocity = direction * dashSpeed;
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, hitRadius);
            for (int i = 0; i < hits.Length; i++)
            {
                EnemyController enemy = hits[i].GetComponent<EnemyController>();
                if (enemy != null && !hitEnemies.Contains(enemy))
                {
                    hitEnemies.Add(enemy);
                    enemy.TakeDamage(stats.SkillDamage);
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        body.velocity = Vector2.zero;
        isDashing = false;
    }

}
