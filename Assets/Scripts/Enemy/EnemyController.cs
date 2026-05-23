using UnityEngine;

public class EnemyController : MonoBehaviour
{
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
    private const float DamageInterval = 0.8f;

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
    }

    private void FixedUpdate()
    {
        if (gameManager == null || gameManager.IsGameOver || player == null || Time.timeScale == 0f)
        {
            StopMovement();
            return;
        }

        Vector2 direction = (player.position - transform.position).normalized;
        body.velocity = direction * moveSpeed;
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            return;
        }

        currentHealth -= damage;
        CombatEffectFactory.CreateCircleEffect(transform.position, new Color(1f, 1f, 1f, 0.65f), 0.45f, 0.08f, 5);
        StopCoroutine("HitFlash");
        StartCoroutine("HitFlash");

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

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        StopMovement();
        CombatEffectFactory.CreateCircleEffect(transform.position, new Color(0.8f, 0.85f, 1f, 0.8f), 0.9f, 0.14f, 6);
        if (enemySpawner != null)
        {
            enemySpawner.UnregisterEnemy();
        }

        if (gameManager != null)
        {
            gameManager.AddExperience(expReward);
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
