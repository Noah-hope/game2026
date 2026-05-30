using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    private GameManager gameManager;
    private Transform player;
    private float nextSpawnTime;
    private bool isSpawning;
    private int activeEnemyCount;
    private int activeSmallEnemyCount;
    private int activeRangedEnemyCount;
    private bool isSpawningSmall;
    private int waveSpawnedCount;
    private int waveSpawnTotal;
    public int ActiveSmallEnemyCount { get { return activeSmallEnemyCount; } }
    public bool IsSpawningSmall { get { return isSpawningSmall; } }

    private const float SpawnPadding = 1.4f;

    public void Initialize(GameManager manager, Transform playerTransform)
    {
        gameManager = manager;
        player = playerTransform;
        isSpawning = true;
        isSpawningSmall = true;
        waveSpawnedCount = 0;
        waveSpawnTotal = GetWaveSpawnTotal(1);
        nextSpawnTime = Time.time + 0.5f;
    }

    private void Update()
    {
        if (!isSpawning || gameManager == null || gameManager.IsGameOver || Time.timeScale == 0f)
        {
            return;
        }

        if (Time.time >= nextSpawnTime)
        {
            int diff = gameManager.DifficultyLevel;
            float interval = GetSpawnInterval(diff);
            nextSpawnTime = Time.time + interval;
            if (activeEnemyCount < GetMaxEnemies(diff))
            {
                SpawnEnemy(diff);
            }
        }
    }

    public void StopSpawning()
    {
        isSpawning = false;
        isSpawningSmall = false;
    }

    public void StopSpawningSmall()
    {
        isSpawningSmall = false;
    }

    public void StartWave(int wave)
    {
        isSpawningSmall = true;
        waveSpawnedCount = 0;
        waveSpawnTotal = GetWaveSpawnTotal(wave);
        nextSpawnTime = Time.time + 0.3f;
    }

    private int GetWaveSpawnTotal(int wave)
    {
        if (wave == 1) return 8;
        if (wave == 2) return 12;
        if (wave == 3) return 18;
        return 24;
    }

    public void SpawnBoss()
    {
        if (gameManager == null || player == null) return;

        Vector3 position = GetRandomEdgePosition();
        GameObject enemyObject = new GameObject("Enemy");
        enemyObject.transform.position = position;

        EnemyController enemy = enemyObject.AddComponent<EnemyController>();

        int diff = gameManager.DifficultyLevel;
        float hpMul = GetHpMultiplier(diff);
        float dmgMul = GetDamageMultiplier(diff);

        int hp = Mathf.RoundToInt(650f * hpMul);
        int dmg = Mathf.RoundToInt(22f * dmgMul);
        int exp = 80;
        float spd = 1.7f * GetSpeedMultiplier(diff);
        float size = 3.1f;

        enemy.Initialize(gameManager, this, player, "Dreadlord Boss", hp, spd, dmg, exp, Color.white, size, GameData.GetEnemySprite("Dreadlord Boss"));
        enemy.SetSlamBehavior(2.35f, 2.4f, Mathf.RoundToInt(30f * dmgMul), 1.75f);
        RegisterEnemy();
    }

    public void RegisterEnemy()
    {
        activeEnemyCount++;
    }

    public void UnregisterEnemy()
    {
        activeEnemyCount = Mathf.Max(0, activeEnemyCount - 1);
    }

    public void RegisterSmallEnemy()
    {
        activeSmallEnemyCount++;
    }

    public void UnregisterSmallEnemy()
    {
        activeSmallEnemyCount = Mathf.Max(0, activeSmallEnemyCount - 1);
    }

    public void RegisterRangedEnemy()
    {
        activeRangedEnemyCount++;
    }

    public void UnregisterRangedEnemy()
    {
        activeRangedEnemyCount = Mathf.Max(0, activeRangedEnemyCount - 1);
    }

    private float GetSpawnInterval(int diff)
    {
        if (diff == 0) return 2.0f;
        if (diff == 1) return 1.7f;
        if (diff == 2) return 1.35f;
        return 1.1f;
    }

    private int GetMaxEnemies(int diff)
    {
        if (diff == 0) return 10;
        if (diff == 1) return 13;
        if (diff == 2) return 16;
        return 20;
    }

    private int GetMaxSmallEnemies(int diff)
    {
        if (diff == 0) return 2;
        if (diff == 1) return 4;
        return 6;
    }

    private int GetMaxRangedEnemies(int diff)
    {
        if (diff == 0) return 1;
        if (diff == 1) return 2;
        return 3;
    }

    private float GetHpMultiplier(int diff)
    {
        if (diff == 0) return 1.0f;
        if (diff == 1) return 1.2f;
        if (diff == 2) return 1.5f;
        return 1.8f;
    }

    private float GetDamageMultiplier(int diff)
    {
        if (diff == 0) return 1.0f;
        if (diff == 1) return 1.1f;
        if (diff == 2) return 1.25f;
        return 1.4f;
    }

    private float GetSpeedMultiplier(int diff)
    {
        if (diff == 0) return 1.0f;
        if (diff == 1) return 1.05f;
        if (diff == 2) return 1.12f;
        return 1.2f;
    }

    private void SpawnEnemy(int diff)
    {
        if (!isSpawningSmall)
        {
            return;
        }

        if (activeSmallEnemyCount >= GetMaxSmallEnemies(diff))
        {
            return;
        }

        Vector3 position = GetRandomEdgePosition();
        GameObject enemyObject = new GameObject("Enemy");
        enemyObject.transform.position = position;

        EnemyController enemy = enemyObject.AddComponent<EnemyController>();

        float hpMul = GetHpMultiplier(diff);
        float dmgMul = GetDamageMultiplier(diff);
        float spdMul = GetSpeedMultiplier(diff);

        bool isElite = false;
        float eliteChance = 0.10f + diff * 0.05f;
        if (Random.value < eliteChance) isElite = true;

        if (isElite)
        {
            hpMul *= 1.8f;
            dmgMul *= 1.5f;
            spdMul *= 0.9f;
        }

        int enemyType = Random.Range(0, 4);
        bool isRangedType = enemyType >= 2;
        if (isRangedType && activeRangedEnemyCount >= GetMaxRangedEnemies(diff))
        {
            enemyType = Random.Range(0, 2);
            isRangedType = false;
        }

        if (enemyType == 0)
        {
            int hp = Mathf.RoundToInt(30f * hpMul);
            int dmg = Mathf.RoundToInt(8f * dmgMul);
            int exp = Mathf.RoundToInt(12f * (isElite ? 2f : 1f));
            float spd = 4f * spdMul;
            float size = isElite ? 1.15f : 0.85f;
            Color eliteColor = isElite ? new Color(1f, 0.25f, 0.55f) : new Color(0.6f, 0.18f, 0.9f);

            enemy.Initialize(gameManager, this, player, isElite ? "Elite Bat" : "Bat", hp, spd, dmg, exp, eliteColor, size, GameData.GetEnemySprite("Bat"));
        }
        else if (enemyType == 1)
        {
            int hp = Mathf.RoundToInt(110f * hpMul);
            int dmg = Mathf.RoundToInt(10f * dmgMul);
            int exp = Mathf.RoundToInt(10f * (isElite ? 2f : 1f));
            float spd = 2.5f * spdMul;
            float size = isElite ? 1.0f : 0.75f;
            Color eliteColor = isElite ? new Color(0.9f, 0.7f, 0.15f) : new Color(0.1f, 0.85f, 0.25f);

            enemy.Initialize(gameManager, this, player, isElite ? "Elite Slime" : "Slime", hp, spd, dmg, exp, eliteColor, size, GameData.GetEnemySprite("Slime"));
        }
        else if (enemyType == 2)
        {
            int hp = Mathf.RoundToInt(85f * hpMul);
            int dmg = Mathf.RoundToInt(12f * dmgMul);
            int exp = Mathf.RoundToInt(14f * (isElite ? 2f : 1f));
            float spd = 2.7f * spdMul;
            float size = isElite ? 1.1f : 0.8f;
            Color eliteColor = isElite ? new Color(1f, 0.65f, 0.2f) : Color.white;

            enemy.Initialize(gameManager, this, player, isElite ? "Elite Mushroom Fiend" : "Mushroom Fiend", hp, spd, dmg, exp, eliteColor, size, GameData.GetEnemySprite("Mushroom Fiend"));
            enemy.SetPoisonSprayBehavior(4.1f, 1.55f, Mathf.RoundToInt(8f * dmgMul), 4.8f, 2.2f);
        }
        else
        {
            int hp = Mathf.RoundToInt(45f * hpMul);
            int dmg = Mathf.RoundToInt(9f * dmgMul);
            int exp = Mathf.RoundToInt(16f * (isElite ? 2f : 1f));
            float spd = 3.6f * spdMul;
            float size = isElite ? 1.05f : 0.75f;
            Color eliteColor = isElite ? new Color(0.7f, 0.35f, 1f) : Color.white;

            enemy.Initialize(gameManager, this, player, isElite ? "Elite Crystal Wisp" : "Crystal Wisp", hp, spd, dmg, exp, eliteColor, size, GameData.GetEnemySprite("Crystal Wisp"));
            enemy.SetIceSpikeBehavior(4.8f, 1.25f, Mathf.RoundToInt(9f * dmgMul), 6.7f, 2.0f);
        }

        RegisterEnemy();
        RegisterSmallEnemy();
        if (isRangedType)
        {
            RegisterRangedEnemy();
        }
        waveSpawnedCount++;
        if (waveSpawnedCount >= waveSpawnTotal)
        {
            isSpawningSmall = false;
            if (gameManager != null) gameManager.TryAdvanceWave();
        }
    }

    private Vector3 GetRandomEdgePosition()
    {
        int side = Random.Range(0, 4);
        float x;
        float y;

        if (side == 0)
        {
            x = Random.Range(GameUtility.MinX + SpawnPadding, GameUtility.MaxX - SpawnPadding);
            y = GameUtility.MaxY - SpawnPadding;
        }
        else if (side == 1)
        {
            x = Random.Range(GameUtility.MinX + SpawnPadding, GameUtility.MaxX - SpawnPadding);
            y = GameUtility.MinY + SpawnPadding;
        }
        else if (side == 2)
        {
            x = GameUtility.MinX + SpawnPadding;
            y = Random.Range(GameUtility.MinY + SpawnPadding, GameUtility.MaxY - SpawnPadding);
        }
        else
        {
            x = GameUtility.MaxX - SpawnPadding;
            y = Random.Range(GameUtility.MinY + SpawnPadding, GameUtility.MaxY - SpawnPadding);
        }

        return new Vector3(x, y, 0f);
    }
}
