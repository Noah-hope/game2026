using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    private GameManager gameManager;
    private Transform player;
    private float nextSpawnTime;
    private bool isSpawning;
    private int activeEnemyCount;

    private const float SpawnInterval = 2f;
    private const int MaxEnemies = 10;
    private const float SpawnPadding = 1.4f;

    public void Initialize(GameManager manager, Transform playerTransform)
    {
        gameManager = manager;
        player = playerTransform;
        isSpawning = true;
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
            nextSpawnTime = Time.time + SpawnInterval;
            if (activeEnemyCount < MaxEnemies)
            {
                SpawnEnemy();
            }
        }
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }

    public void RegisterEnemy()
    {
        activeEnemyCount++;
    }

    public void UnregisterEnemy()
    {
        activeEnemyCount = Mathf.Max(0, activeEnemyCount - 1);
    }

    private void SpawnEnemy()
    {
        Vector3 position = GetRandomEdgePosition();
        GameObject enemyObject = new GameObject("Enemy");
        enemyObject.transform.position = position;

        EnemyController enemy = enemyObject.AddComponent<EnemyController>();
        bool spawnBat = Random.value > 0.5f;

        if (spawnBat)
        {
            enemy.Initialize(gameManager, this, player, "Bat", 30, 4f, 8, 12, new Color(0.6f, 0.18f, 0.9f), 0.85f, GameData.GetEnemySprite("Bat"));
        }
        else
        {
            enemy.Initialize(gameManager, this, player, "Slime", 110, 2.5f, 10, 10, new Color(0.1f, 0.85f, 0.25f), 0.75f, GameData.GetEnemySprite("Slime"));
        }

        RegisterEnemy();
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
