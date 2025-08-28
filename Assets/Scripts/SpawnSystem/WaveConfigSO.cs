using UnityEngine;

[CreateAssetMenu(fileName = "NewWaveConfig", menuName = "Wave System/Wave Config")]
public class WaveConfigSO : ScriptableObject
{
    [Header("Идентификация волны")]
    public string waveName = "Новая волна";
    public int waveNumber = 1;
    public WaveType waveType = WaveType.Normal;

    [Header("Настройки врагов")]
    public GameObject enemyPrefab;
    [Tooltip("Минимальный уровень врагов в этой волне")]
    public int minEnemyLevel = 1;
    [Tooltip("Максимальный уровень врагов в этой волне")]
    public int maxEnemyLevel = 1;

    [Header("Количество врагов")]
    public int minEnemies = 5;
    public int maxEnemies = 10;
    public EnemySpawnMode spawnMode = EnemySpawnMode.Constant;

    [Header("Тайминги спавна")]
    public float spawnInterval = 1f;
    public float spawnDuration = 30f;
    public float delayAfterWave = 5f;
    public float timeBetweenSpawnGroups = 0f;

    [Header("Позиционирование")]
    public float spawnRadius = 15f;
    public float minSpawnDistance = 5f;
    public bool spawnAroundPlayer = true;
    public SpawnPattern spawnPattern = SpawnPattern.Random;

    [Header("Модификаторы характеристик")]
    [Range(0.1f, 5f)] public float healthMultiplier = 1f;
    [Range(0.1f, 5f)] public float damageMultiplier = 1f;
    [Range(0.1f, 5f)] public float speedMultiplier = 1f;
    [Range(0f, 5f)] public float defenseBonus = 0f;
    [Range(0f, 1f)] public float eliteChance = 0.1f;

    [Header("Элитные враги")]
    public GameObject eliteEnemyPrefab;
    [Range(0.1f, 5f)] public float eliteHealthMultiplier = 2f;
    [Range(0.1f, 5f)] public float eliteDamageMultiplier = 1.5f;
    public Color eliteColor = Color.yellow;

    [Header("Награды за волну")]
    public int baseExpReward = 100;
    public int minCurrencyReward = 10;
    public int maxCurrencyReward = 20;
    [Range(0f, 1f)] public float itemDropChance = 0.3f;
    public GameObject[] possibleItemDrops;

    [Header("Визуальные эффекты")]
    public GameObject spawnEffect;
    public Color waveColor = Color.white;
    public AudioClip waveStartSound;
    public AudioClip waveEndSound;

    [Header("Особые события")]
    public bool spawnBoss = false;
    public GameObject bossPrefab;
    public int bossSpawnThreshold = 20;

    // Метод для получения случайного количества врагов
    public int GetRandomEnemyCount()
    {
        return Random.Range(minEnemies, maxEnemies + 1);
    }

    // Метод для получения множителя сложности
    public float GetDifficultyMultiplier(int currentWave)
    {
        float waveFactor = 1f + (currentWave * 0.1f);
        return waveFactor;
    }

    // Метод для проверки, должен ли спауниться элитный враг
    public bool ShouldSpawnElite()
    {
        return eliteEnemyPrefab != null && Random.value <= eliteChance;
    }

    // Метод для получения префаба врага (обычный или элитный)
    public GameObject GetEnemyPrefabToSpawn()
    {
        if (ShouldSpawnElite())
        {
            return eliteEnemyPrefab;
        }
        return enemyPrefab;
    }

    // Метод для получения награды за волну
    public int GetCurrencyReward()
    {
        return Random.Range(minCurrencyReward, maxCurrencyReward + 1);
    }

    // Метод для получения случайного предмета из возможных дропов
    public GameObject GetRandomItemDrop()
    {
        if (possibleItemDrops.Length == 0 || Random.value > itemDropChance)
            return null;

        return possibleItemDrops[Random.Range(0, possibleItemDrops.Length)];
    }
}

// Перечисления для лучшей организации
public enum WaveType
{
    Normal,
    Elite,
    Boss,
    Event,
    Survival
}

public enum EnemySpawnMode
{
    Constant,       // Постоянный спавн
    Burst,          // Группами
    Increasing,     // Увеличивающееся количество
    Decreasing      // Уменьшающееся количество
}

public enum SpawnPattern
{
    Random,
    Circle,
    Line,
    FromSides,
    FromCorners
}