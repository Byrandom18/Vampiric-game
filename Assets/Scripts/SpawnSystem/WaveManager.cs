using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    [Header("Ссылки")]
    public WaveSequenceSO currentWaveSequence;

    [Header("Настройки пулинга")]
    [SerializeField] private bool useObjectPooling = true;
    [SerializeField] private int prewarmEnemiesCount = 20;

    [Header("Текущее состояние")]
    public int currentWaveIndex = 0;
    public int currentWaveNumber = 1;
    public bool isWaveActive = false;
    public int enemiesRemaining = 0;
    public int totalWavesCompleted = 0;

    private List<GameObject> activeEnemies = new List<GameObject>();
    private Dictionary<GameObject, Queue<GameObject>> enemyPools = new Dictionary<GameObject, Queue<GameObject>>();
    private Coroutine waveCoroutine;
    private bool isPlayerAlive = true;
    private Vector3 lastKnownPlayerPosition;
    private Transform playerTransform;
    private bool isInitialized = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        // Подписываемся на события игрока
        PlayerStats.OnPlayerSpawned += OnPlayerSpawned;
        PlayerStats.OnPlayerDeath += OnPlayerDeath;
    }

    private void OnDisable()
    {
        // Отписываемся от событий
        PlayerStats.OnPlayerSpawned -= OnPlayerSpawned;
        PlayerStats.OnPlayerDeath -= OnPlayerDeath;
    }

    private void Start()
    {
        InitializeWaveManager();
    }

    private void InitializeWaveManager()
    {
        FindPlayer();

        if (useObjectPooling)
        {
            PrewarmEnemyPools();
        }

        if (isPlayerAlive)
        {
            StartWaveSequence();
        }

        isInitialized = true;
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            lastKnownPlayerPosition = playerTransform.position;
            isPlayerAlive = true;
            Debug.Log("Player found by WaveManager");
        }
        else
        {
            playerTransform = null;
            isPlayerAlive = false;
            Debug.LogWarning("Player not found! WaveManager will pause until player is available.");
        }
    }

    private void OnPlayerSpawned(Transform player)
    {
        playerTransform = player;
        lastKnownPlayerPosition = player.position;
        isPlayerAlive = true;

        if (isInitialized && !isWaveActive)
        {
            StartWaveSequence();
        }
    }

    private void OnPlayerDeath()
    {
        isPlayerAlive = false;
        playerTransform = null;

        // Останавливаем текущую волну
        if (waveCoroutine != null)
        {
            StopCoroutine(waveCoroutine);
            waveCoroutine = null;
        }

        // Очищаем всех врагов
        ClearAllEnemies();

        Debug.Log("WaveManager paused due to player death");
    }

    private void PrewarmEnemyPools()
    {
        if (currentWaveSequence == null)
        {
            Debug.LogWarning("No wave sequence assigned!");
            return;
        }

        foreach (var waveStage in currentWaveSequence.waves)
        {
            if (waveStage.waveConfig != null && waveStage.waveConfig.enemyPrefab != null)
            {
                GameObject enemyPrefab = waveStage.waveConfig.enemyPrefab;
                if (!enemyPools.ContainsKey(enemyPrefab))
                {
                    enemyPools[enemyPrefab] = new Queue<GameObject>();
                    for (int i = 0; i < prewarmEnemiesCount; i++)
                    {
                        GameObject enemy = Instantiate(enemyPrefab, transform);
                        enemy.SetActive(false);
                        enemyPools[enemyPrefab].Enqueue(enemy);

                        if (!enemy.TryGetComponent<PoolableEnemy>(out _))
                        {
                            var poolable = enemy.AddComponent<PoolableEnemy>();
                            poolable.enemyPrefab = enemyPrefab;
                        }
                    }
                    Debug.Log($"Pre-warmed pool for {enemyPrefab.name}, size: {prewarmEnemiesCount}");
                }
            }
        }
    }

    public void StartWaveSequence()
    {
        if (waveCoroutine != null)
        {
            StopCoroutine(waveCoroutine);
        }

        waveCoroutine = StartCoroutine(WaveLoopCoroutine());
    }

    private IEnumerator WaveLoopCoroutine()
    {
        while (true)
        {
            // Ждем, пока игрок будет доступен
            if (!isPlayerAlive || playerTransform == null)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            if (currentWaveSequence != null && currentWaveIndex < currentWaveSequence.waves.Length)
            {
                yield return StartCoroutine(StartNextWaveCoroutine());
            }
            else if (currentWaveSequence != null && currentWaveSequence.infiniteWaves)
            {
                yield return StartCoroutine(StartInfiniteWaveCoroutine());
            }
            else
            {
                Debug.Log("Все волны завершены!");
                yield return new WaitForSeconds(5f);
                ResetWaveManager();
            }
        }
    }

    private IEnumerator StartNextWaveCoroutine()
    {
        var waveStage = currentWaveSequence.waves[currentWaveIndex];
        if (waveStage.waveConfig == null)
        {
            Debug.LogError($"Wave config is null at index {currentWaveIndex}!");
            yield break;
        }

        var waveConfig = waveStage.waveConfig;

        isWaveActive = true;
        Debug.Log($"Начинается волна {currentWaveNumber}: {waveConfig.waveName}");

        PlayWaveStartEffects(waveConfig);

        int enemyCount = waveConfig.GetRandomEnemyCount();
        enemiesRemaining = enemyCount;

        yield return StartCoroutine(SpawnWaveEnemiesCoroutine(waveConfig, enemyCount));

        if (waveStage.waitForAllEnemiesDead)
        {
            yield return new WaitUntil(() => enemiesRemaining <= 0 && activeEnemies.Count == 0 || !isPlayerAlive);
        }
        else
        {
            yield return new WaitForSeconds(waveStage.waveDuration);
        }

        if (isPlayerAlive)
        {
            CompleteWave(waveConfig);
        }
        else
        {
            isWaveActive = false;
        }
    }

    private IEnumerator StartInfiniteWaveCoroutine()
    {
        WaveConfigSO waveConfig = currentWaveSequence.CreateDynamicWave(currentWaveIndex);
        if (waveConfig == null)
        {
            Debug.LogError("Failed to create dynamic wave!");
            yield break;
        }

        isWaveActive = true;
        Debug.Log($"Бесконечная волна {currentWaveNumber}: {waveConfig.waveName}");

        PlayWaveStartEffects(waveConfig);

        int enemyCount = waveConfig.GetRandomEnemyCount();
        enemiesRemaining = enemyCount;

        yield return StartCoroutine(SpawnWaveEnemiesCoroutine(waveConfig, enemyCount));

        yield return new WaitUntil(() => enemiesRemaining <= 0 && activeEnemies.Count == 0 || !isPlayerAlive);

        if (isPlayerAlive)
        {
            CompleteWave(waveConfig);
        }
        else
        {
            isWaveActive = false;
        }
    }

    private IEnumerator SpawnWaveEnemiesCoroutine(WaveConfigSO waveConfig, int enemyCount)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            // Проверяем, жив ли игрок перед спавном
            if (!isPlayerAlive)
            {
                yield break;
            }

            // Обновляем позицию игрока
            if (playerTransform != null)
            {
                lastKnownPlayerPosition = playerTransform.position;
            }

            // Лимит активных врагов
            if (activeEnemies.Count >= currentWaveSequence.maxActiveEnemies)
            {
                yield return new WaitUntil(() =>
                    activeEnemies.Count < currentWaveSequence.maxActiveEnemies ||
                    !isPlayerAlive
                );
            }

            if (isPlayerAlive)
            {
                SpawnEnemy(waveConfig);
                yield return new WaitForSeconds(waveConfig.spawnInterval);
            }
            else
            {
                yield break;
            }
        }
    }

    private void SpawnEnemy(WaveConfigSO waveConfig)
    {
        if (!isPlayerAlive || waveConfig == null)
        {
            return;
        }

        Vector3 spawnPosition = GetSpawnPosition(waveConfig);

        // Эффект спавна
        if (waveConfig.spawnEffect != null)
        {
            Instantiate(waveConfig.spawnEffect, spawnPosition, Quaternion.identity);
        }

        GameObject enemyPrefab = waveConfig.GetEnemyPrefabToSpawn();
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy prefab is null!");
            return;
        }

        GameObject enemy = GetEnemyFromPool(enemyPrefab, spawnPosition);
        if (enemy == null)
        {
            Debug.LogError("Failed to get enemy from pool or instantiate!");
            return;
        }

        activeEnemies.Add(enemy);
        SetupEnemy(enemy, waveConfig);

        EnemyDamage enemyDamage = enemy.GetComponent<EnemyDamage>();
        if (enemyDamage != null)
        {
            enemyDamage.OnDeath += OnEnemyDeath;
        }
    }

    private GameObject GetEnemyFromPool(GameObject enemyPrefab, Vector3 position)
    {
        if (useObjectPooling && enemyPools.ContainsKey(enemyPrefab) && enemyPools[enemyPrefab].Count > 0)
        {
            GameObject enemy = enemyPools[enemyPrefab].Dequeue();
            if (enemy != null)
            {
                enemy.transform.position = position;
                enemy.transform.rotation = Quaternion.identity;
                enemy.SetActive(true);
                return enemy;
            }
        }

        // Резервное создание если пул пуст или отключен
        return Instantiate(enemyPrefab, position, Quaternion.identity);
    }

    private Vector3 GetSpawnPosition(WaveConfigSO waveConfig)
    {
        Vector3 spawnCenter;

        if (waveConfig.spawnAroundPlayer && isPlayerAlive)
        {
            // Используем последнюю известную позицию игрока
            spawnCenter = lastKnownPlayerPosition;
        }
        else
        {
            // Спавн вокруг менеджера волн
            spawnCenter = transform.position;
        }

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        float distance = Random.Range(waveConfig.minSpawnDistance, waveConfig.spawnRadius);

        return spawnCenter + (Vector3)randomDir * distance;
    }

    private void SetupEnemy(GameObject enemy, WaveConfigSO waveConfig)
    {
        if (enemy == null || waveConfig == null) return;

        EnemyDamage enemyStats = enemy.GetComponent<EnemyDamage>();
        if (enemyStats != null && PlayerStats.Instance != null)
        {
            int enemyLevel = Random.Range(waveConfig.minEnemyLevel, waveConfig.maxEnemyLevel + 1);

            enemyStats.InitializeEnemy(
                Mathf.RoundToInt(PlayerStats.Instance.difficultyMod),
                enemyLevel
            );

            // Применяем модификаторы
            //enemyStats.SetSpeedMultiplier(waveConfig.speedMultiplier);
            enemyStats.health *= waveConfig.healthMultiplier;
            enemyStats.damage *= waveConfig.damageMultiplier;
            enemyStats.defence += waveConfig.defenseBonus;

            //if (enemy.CompareTag("EliteEnemy"))
            //{
            //    enemyStats.health *= waveConfig.eliteHealthMultiplier;
            //    enemyStats.damage *= waveConfig.eliteDamageMultiplier;
            //}
        }
    }

    private void OnEnemyDeath(EnemyDamage enemyDamage)
    {
        if (enemyDamage == null) return;

        GameObject enemy = enemyDamage.gameObject;
        enemiesRemaining--;
        activeEnemies.Remove(enemy);

        enemyDamage.OnDeath -= OnEnemyDeath;

        ReturnEnemyToPool(enemy);

        // Статистика убийств
        if (PlayerStats.Instance != null && isPlayerAlive)
        {
            PlayerStats.Instance.AddKill();
        }
    }

    private void ReturnEnemyToPool(GameObject enemy)
    {
        if (enemy == null) return;

        if (useObjectPooling)
        {
            PoolableEnemy poolable = enemy.GetComponent<PoolableEnemy>();
            if (poolable != null && poolable.enemyPrefab != null &&
                enemyPools.ContainsKey(poolable.enemyPrefab))
            {
                enemy.SetActive(false);
                enemyPools[poolable.enemyPrefab].Enqueue(enemy);
                return;
            }
        }

        // Если пулинг отключен или не удалось вернуть в пул - уничтожаем
        Destroy(enemy);
    }

    private void PlayWaveStartEffects(WaveConfigSO waveConfig)
    {
        if (!isPlayerAlive) return;

        //if (UIManager.Instance != null)
        //{
        //    UIManager.Instance.ShowWaveStartText($"Wave {currentWaveNumber}: {waveConfig.waveName}");
        //}

        //if (waveConfig.waveStartSound != null && AudioManager.Instance != null)
        //{
        //    AudioManager.Instance.PlaySFX(waveConfig.waveStartSound);
        //}
    }

    private void CompleteWave(WaveConfigSO waveConfig)
    {
        isWaveActive = false;
        totalWavesCompleted++;

        GiveWaveRewards(waveConfig);
        Debug.Log($"Волна {currentWaveNumber} завершена!");

        currentWaveIndex++;
        currentWaveNumber++;
    }

    private void GiveWaveRewards(WaveConfigSO waveConfig)
    {
        if (PlayerStats.Instance != null && isPlayerAlive)
        {
            PlayerStats.Instance.AddExp(waveConfig.baseExpReward);

            int currencyReward = waveConfig.GetCurrencyReward();
            PlayerStats.Instance.AddGold(currencyReward);

            GameObject itemDrop = waveConfig.GetRandomItemDrop();
            if (itemDrop != null)
            {
                Instantiate(itemDrop, lastKnownPlayerPosition, Quaternion.identity);
            }

            PlayerStats.Instance.AddWaveCompleted();
        }
    }

    private void ClearAllEnemies()
    {
        foreach (var enemy in new List<GameObject>(activeEnemies))
        {
            if (enemy != null)
            {
                EnemyDamage enemyDamage = enemy.GetComponent<EnemyDamage>();
                if (enemyDamage != null)
                {
                    enemyDamage.OnDeath -= OnEnemyDeath;
                }
                ReturnEnemyToPool(enemy);
            }
        }
        activeEnemies.Clear();
        enemiesRemaining = 0;
    }

    public void SetWaveSequence(WaveSequenceSO newSequence)
    {
        currentWaveSequence = newSequence;
        ResetWaveManager();
    }

    public void ResetWaveManager()
    {
        currentWaveIndex = 0;
        currentWaveNumber = 1;
        enemiesRemaining = 0;
        isWaveActive = false;

        ClearAllEnemies();

        if (waveCoroutine != null)
        {
            StopCoroutine(waveCoroutine);
            waveCoroutine = null;
        }

        FindPlayer();

        if (isPlayerAlive)
        {
            StartWaveSequence();
        }
    }

    public int GetActiveEnemiesCount()
    {
        return activeEnemies.Count;
    }

    public bool IsWaveInProgress()
    {
        return isWaveActive;
    }

    public int currentWave => currentWaveNumber;
}

[System.Serializable]
public class PoolableEnemy : MonoBehaviour
{
    public GameObject enemyPrefab;
}