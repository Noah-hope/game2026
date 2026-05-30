using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private enum AttackStyle
    {
        Chase,
        Fireball,
        PoisonSpray,
        IceSpike,
        Slam
    }

    private GameManager gameManager;
    private EnemySpawner enemySpawner;
    private Transform player;
    private int currentHealth;
    private int maxHealth;
    private float moveSpeed;
    private int contactDamage;
    private int expReward;
    private float nextDamageTime;
    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private Color baseColor;
    private bool isDead;
    public bool IsBoss { get; private set; }
    private bool isRangedEnemy;
    private EnemyHealthBar healthBar;
    private float knockbackEndTime;
    private Vector2 knockbackVelocity;
    private const float DamageInterval = 0.8f;
    private const float KnockbackForce = 7f;
    private const float KnockbackDuration = 0.1f;
    private const float BossMoveFrameDuration = 0.14f;

    private bool isRanged;
    private bool isAttacking;
    private AttackStyle attackStyle = AttackStyle.Chase;
    private float attackRange;
    private float shootInterval;
    private float nextShootTime;
    private int bulletDamage;
    private float bulletSpeed;
    private float bulletLife;
    private float slamRadius;
    private Sprite idleSprite;
    private Sprite slamWindupSprite;
    private Sprite slamImpactSprite;
    private Sprite slamRecoverSprite;
    private Sprite[] bossMoveSprites;
    private int bossMoveFrameIndex;
    private float bossMoveFrameTimer;
    private float currentSlowMultiplier = 1f;
    private Vector2 obstacleCheckSize = new Vector2(0.8f, 0.8f);

    public void Initialize(GameManager manager, EnemySpawner spawner, Transform playerTransform, string enemyName, int health, float speed, int damage, int exp, Color color, float size, Sprite sprite)
    {
        gameManager = manager;
        enemySpawner = spawner;
        player = playerTransform;
        maxHealth = health;
        currentHealth = health;
        moveSpeed = speed;
        contactDamage = damage;
        expReward = exp;
        name = enemyName;

        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite != null ? sprite : GameData.GetSquareSprite();
        spriteRenderer.color = sprite != null ? Color.white : color;
        spriteRenderer.sortingOrder = 1;
        baseColor = spriteRenderer.color;

        transform.localScale = new Vector3(size, size, 1f);

        body = gameObject.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.9f, 0.9f);
        obstacleCheckSize = collider.size * Mathf.Max(0.65f, size * 0.85f);

        healthBar = gameObject.AddComponent<EnemyHealthBar>();
        healthBar.Initialize(maxHealth);
    }

    public void SetExternalSpeedMultiplier(float multiplier)
    {
        currentSlowMultiplier = multiplier;
    }

    public void SetRangedBehavior(float range, float cooldown, int bDamage, float bSpeed, float bLife)
    {
        isRanged = true;
        attackStyle = AttackStyle.Fireball;
        attackRange = range;
        shootInterval = cooldown;
        bulletDamage = bDamage;
        bulletSpeed = bSpeed;
        bulletLife = bLife;
        nextShootTime = Time.time + 1f;
    }

    public void SetPoisonSprayBehavior(float range, float cooldown, int pDamage, float pSpeed, float pLife)
    {
        isRanged = true;
        isRangedEnemy = true;
        attackStyle = AttackStyle.PoisonSpray;
        attackRange = range;
        shootInterval = cooldown;
        bulletDamage = pDamage;
        bulletSpeed = pSpeed;
        bulletLife = pLife;
        nextShootTime = Time.time + 0.8f;
    }

    public void SetIceSpikeBehavior(float range, float cooldown, int iDamage, float iSpeed, float iLife)
    {
        isRanged = true;
        isRangedEnemy = true;
        attackStyle = AttackStyle.IceSpike;
        attackRange = range;
        shootInterval = cooldown;
        bulletDamage = iDamage;
        bulletSpeed = iSpeed;
        bulletLife = iLife;
        nextShootTime = Time.time + 0.7f;
    }

    public void SetSlamBehavior(float range, float cooldown, int sDamage, float radius)
    {
        IsBoss = true;
        isRanged = false;
        attackStyle = AttackStyle.Slam;
        attackRange = range;
        shootInterval = cooldown;
        bulletDamage = sDamage;
        slamRadius = radius;
        nextShootTime = Time.time + 1.5f;

        idleSprite = spriteRenderer != null ? spriteRenderer.sprite : null;
        slamWindupSprite = GameData.GetEffectSprite("dreadlord_boss_windup");
        slamImpactSprite = GameData.GetEffectSprite("dreadlord_boss_slam");
        slamRecoverSprite = GameData.GetEffectSprite("dreadlord_boss_recover");
        bossMoveSprites = new Sprite[]
        {
            GameData.GetEffectSprite("dreadlord_boss_walk_1"),
            GameData.GetEffectSprite("dreadlord_boss_walk_2"),
            GameData.GetEffectSprite("dreadlord_boss_walk_3"),
            GameData.GetEffectSprite("dreadlord_boss_walk_4")
        };
    }

    private void FixedUpdate()
    {
        if (gameManager == null || gameManager.IsGameOver || player == null || Time.timeScale == 0f)
        {
            StopMovement();
            return;
        }

        if (Time.time < knockbackEndTime)
        {
            MoveWithObstacleBlocking(knockbackVelocity);
            return;
        }

        Vector2 direction = (player.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, player.position);

        if (isAttacking)
        {
            StopMovement();
            return;
        }

        if (attackStyle == AttackStyle.Slam)
        {
            UpdateSlamBehavior(direction, distance);
            return;
        }

        if (isRanged)
        {
            float stopDistance = attackRange * 0.7f;
            if (distance > attackRange)
            {
                MoveWithObstacleBlocking(direction * moveSpeed * currentSlowMultiplier);
            }
            else if (distance < stopDistance)
            {
                MoveWithObstacleBlocking(-direction * moveSpeed * 0.6f * currentSlowMultiplier);
            }
            else
            {
                body.velocity = Vector2.zero;
            }

            if (Time.time >= nextShootTime)
            {
                nextShootTime = Time.time + shootInterval;
                ShootAtPlayer(direction);
            }

            return;
        }

        MoveWithObstacleBlocking(direction * moveSpeed * currentSlowMultiplier);
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            return;
        }

        currentHealth -= damage;
        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth);
        }
        CombatEffectFactory.CreateCircleEffect(transform.position, new Color(1f, 1f, 1f, 0.65f), 0.45f, 0.08f, 5);
        CombatEffectFactory.CreateDamageText(transform.position + new Vector3(0f, 0.5f, 0f), damage.ToString(), new Color(1f, 0.9f, 0.2f, 1f));
        StopCoroutine("HitFlash");
        StartCoroutine("HitFlash");

        if (player != null && body != null)
        {
            Vector2 knockDir = ((Vector2)(transform.position - player.position)).normalized;
            knockbackVelocity = knockDir * KnockbackForce;
            knockbackEndTime = Time.time + KnockbackDuration;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null || Time.time < nextDamageTime || gameManager == null || gameManager.IsGameOver)
        {
            return;
        }

        nextDamageTime = Time.time + DamageInterval;
        playerHealth.TakeDamage(contactDamage);
    }

    private void ShootAtPlayer(Vector2 direction)
    {
        if (attackStyle == AttackStyle.PoisonSpray)
        {
            CreateEnemyProjectile(direction, "Poison Glob", GameData.GetCircleSprite(), new Color(0.35f, 1f, 0.15f), new Vector3(0.36f, 0.36f, 1f), new Vector2(0.9f, 0.9f), bulletSpeed, bulletDamage, bulletLife, new Color(0.35f, 1f, 0.15f, 0.75f));
            CombatEffectFactory.CreateCircleEffect(transform.position + (Vector3)(direction * 0.45f), new Color(0.25f, 1f, 0.1f, 0.55f), 0.42f, 0.12f, 4);
            return;
        }

        if (attackStyle == AttackStyle.IceSpike)
        {
            CreateEnemyProjectile(direction, "Ice Spike", GameData.GetSquareSprite(), new Color(0.45f, 0.9f, 1f), new Vector3(0.22f, 0.6f, 1f), new Vector2(0.8f, 1.35f), bulletSpeed, bulletDamage, bulletLife, new Color(0.45f, 0.9f, 1f, 0.75f));
            CombatEffectFactory.CreateCircleEffect(transform.position + (Vector3)(direction * 0.45f), new Color(0.65f, 0.95f, 1f, 0.5f), 0.42f, 0.12f, 4);
            return;
        }

        CreateEnemyProjectile(direction, "Enemy Bullet", GameData.GetCircleSprite(), new Color(1f, 0.35f, 0.2f), new Vector3(0.25f, 0.25f, 1f), Vector2.one, bulletSpeed, bulletDamage, bulletLife, new Color(1f, 0.35f, 0.2f, 0.75f));
    }

    private void CreateEnemyProjectile(Vector2 direction, string projectileName, Sprite sprite, Color color, Vector3 scale, Vector2 colliderSize, float speed, int damage, float lifetime, Color trailColor)
    {
        GameObject bulletObj = new GameObject(projectileName);
        bulletObj.transform.position = transform.position + (Vector3)(direction.normalized * 0.55f);
        bulletObj.transform.localScale = scale;

        SpriteRenderer renderer = bulletObj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = 3;

        Rigidbody2D bulletBody = bulletObj.AddComponent<Rigidbody2D>();
        bulletBody.gravityScale = 0f;
        bulletBody.freezeRotation = true;

        BoxCollider2D collider = bulletObj.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = colliderSize;

        Projectile bullet = bulletObj.AddComponent<Projectile>();
        bullet.Initialize(direction, speed, damage, lifetime, trailColor);
        bullet.SetTargetsPlayer();
        bullet.SetHitEffectColor(trailColor);
    }

    private void UpdateSlamBehavior(Vector2 direction, float distance)
    {
        if (distance > attackRange)
        {
            MoveWithObstacleBlocking(direction * moveSpeed * currentSlowMultiplier);
            UpdateBossMoveAnimation(true);
            return;
        }

        StopMovement();
        UpdateBossMoveAnimation(false);

        if (Time.time >= nextShootTime)
        {
            nextShootTime = Time.time + shootInterval;
            StartCoroutine(SlamAttack(direction));
        }
    }

    private System.Collections.IEnumerator SlamAttack(Vector2 direction)
    {
        isAttacking = true;
        StopMovement();

        Vector3 slamCenter = transform.position + (Vector3)(direction.normalized * 0.75f);
        SetEnemySprite(slamWindupSprite);
        CombatEffectFactory.CreateCircleEffect(slamCenter, new Color(1f, 0.25f, 0.05f, 0.32f), slamRadius * 2f, 0.28f, 4);
        yield return new WaitForSeconds(0.24f);

        SetEnemySprite(slamImpactSprite);
        DealSlamDamage(slamCenter);
        CombatEffectFactory.CreateCircleEffect(slamCenter, new Color(1f, 0.7f, 0.1f, 0.72f), slamRadius * 2.05f, 0.16f, 7);
        CombatEffectFactory.CreateCircleEffect(slamCenter, new Color(1f, 1f, 1f, 0.62f), slamRadius * 0.85f, 0.08f, 8);
        yield return new WaitForSeconds(0.14f);

        SetEnemySprite(slamRecoverSprite);
        yield return new WaitForSeconds(0.2f);

        SetEnemySprite(idleSprite);
        isAttacking = false;
    }

    private void DealSlamDamage(Vector3 center)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, slamRadius);
        for (int i = 0; i < hits.Length; i++)
        {
            PlayerHealth playerHealth = hits[i].GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(bulletDamage);
                CombatEffectFactory.CreateDamageText(playerHealth.transform.position + new Vector3(0f, 0.85f, 0f), "-" + bulletDamage, new Color(1f, 0.2f, 0.1f, 1f));
                return;
            }
        }
    }

    private void SetEnemySprite(Sprite sprite)
    {
        if (spriteRenderer != null && sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }

    private void UpdateBossMoveAnimation(bool isMoving)
    {
        if (attackStyle != AttackStyle.Slam || isAttacking || spriteRenderer == null)
        {
            return;
        }

        if (!isMoving || bossMoveSprites == null || bossMoveSprites.Length == 0)
        {
            bossMoveFrameTimer = 0f;
            bossMoveFrameIndex = 0;
            SetEnemySprite(idleSprite);
            return;
        }

        bossMoveFrameTimer += Time.fixedDeltaTime;
        if (bossMoveFrameTimer >= BossMoveFrameDuration)
        {
            bossMoveFrameTimer = 0f;
            bossMoveFrameIndex = (bossMoveFrameIndex + 1) % bossMoveSprites.Length;
        }

        SetEnemySprite(bossMoveSprites[bossMoveFrameIndex]);
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        StopMovement();
        CombatEffectFactory.CreateCircleEffect(transform.position, new Color(1f, 0.7f, 0.2f, 0.8f), 1.2f, 0.3f, 7);
        CombatEffectFactory.CreateCircleEffect(transform.position, Color.white, 0.55f, 0.12f, 7);
        for (int i = 0; i < 5; i++)
        {
            float angle = i * 72f * Mathf.Deg2Rad + Random.Range(-0.2f, 0.2f);
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * 0.55f;
            CombatEffectFactory.CreateCircleEffect(transform.position + offset, new Color(1f, 0.85f, 0.2f, 0.65f), 0.22f, 0.25f, 7);
        }
        if (enemySpawner != null)
        {
            enemySpawner.UnregisterEnemy();
            if (!IsBoss)
            {
                enemySpawner.UnregisterSmallEnemy();
                if (isRangedEnemy)
                {
                    enemySpawner.UnregisterRangedEnemy();
                }
            }
        }

        if (gameManager != null)
        {
            gameManager.AddKill();
            gameManager.AddExperience(expReward);
            if (IsBoss)
            {
                gameManager.OnBossDefeated();
            }
        }

        Destroy(gameObject);
    }

    private void StopMovement()
    {
        if (body != null)
        {
            body.velocity = Vector2.zero;
        }
    }

    private void MoveWithObstacleBlocking(Vector2 desiredVelocity)
    {
        if (body == null)
        {
            return;
        }

        if (desiredVelocity.sqrMagnitude <= 0.0001f)
        {
            body.velocity = Vector2.zero;
            return;
        }

        float stepTime = Time.fixedDeltaTime;
        Vector2 currentPosition = body.position;
        Vector2 desiredStep = desiredVelocity * stepTime;
        Vector2 desiredPosition = currentPosition + desiredStep;

        if (!WouldOverlapObstacle(desiredPosition))
        {
            body.velocity = desiredVelocity;
            return;
        }

        Vector2 slideX = new Vector2(desiredStep.x, 0f);
        if (Mathf.Abs(slideX.x) > 0.0001f && !WouldOverlapObstacle(currentPosition + slideX))
        {
            body.velocity = new Vector2(desiredVelocity.x, 0f);
            return;
        }

        Vector2 slideY = new Vector2(0f, desiredStep.y);
        if (Mathf.Abs(slideY.y) > 0.0001f && !WouldOverlapObstacle(currentPosition + slideY))
        {
            body.velocity = new Vector2(0f, desiredVelocity.y);
            return;
        }

        body.velocity = Vector2.zero;
    }

    private bool WouldOverlapObstacle(Vector2 position)
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(position, obstacleCheckSize, 0f);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit != null && !hit.isTrigger && hit.GetComponent<Obstacle>() != null)
            {
                return true;
            }
        }

        return false;
    }

    private System.Collections.IEnumerator HitFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        yield return new WaitForSeconds(0.08f);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = baseColor;
        }
    }
}
