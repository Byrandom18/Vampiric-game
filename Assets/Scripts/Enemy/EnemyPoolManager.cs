using System.Collections.Generic;
using UnityEngine;

public class EnemyPoolManager : MonoBehaviour
{
    public static EnemyPoolManager Instance;

    [System.Serializable]
    public class EnemyPool
    {
        public GameObject enemyPrefab;
        public int initialPoolSize = 10;
        [HideInInspector] public Queue<GameObject> pool = new Queue<GameObject>();
    }

    public List<EnemyPool> enemyPools = new List<EnemyPool>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
            InitializePools();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePools()
    {
        foreach (var enemyPool in enemyPools)
        {
            for (int i = 0; i < enemyPool.initialPoolSize; i++)
            {
                GameObject enemy = Instantiate(enemyPool.enemyPrefab, transform);
                enemy.SetActive(false);
                enemyPool.pool.Enqueue(enemy);
            }
        }
    }

    public GameObject GetEnemyFromPool(GameObject enemyPrefab, Vector3 position, Quaternion rotation)
    {
        EnemyPool pool = enemyPools.Find(p => p.enemyPrefab == enemyPrefab);
        if (pool == null)
        {
            // Создаем новый пул, если не найден
            pool = new EnemyPool { enemyPrefab = enemyPrefab };
            enemyPools.Add(pool);
        }

        GameObject enemy;
        if (pool.pool.Count > 0)
        {
            enemy = pool.pool.Dequeue();
        }
        else
        {
            enemy = Instantiate(enemyPrefab, transform);
        }

        enemy.transform.position = position;
        enemy.transform.rotation = rotation;
        enemy.SetActive(true);

        // Инициализируем врага
        EnemyDamage enemyDamage = enemy.GetComponent<EnemyDamage>();
        if (enemyDamage != null)
        {
            enemyDamage.InitializeEnemy(PlayerStats.Instance.difficultyMod, WaveManager.Instance.currentWave);
        }

        return enemy;
    }

    public void ReturnEnemyToPool(EnemyDamage enemy)
    {
        enemy.ReturnToPool();

        // Находим соответствующий пул
        foreach (var pool in enemyPools)
        {
            if (pool.pool.Contains(enemy.gameObject))
            {
                return;
            }
        }

        // Добавляем в первый подходящий пул
        foreach (var pool in enemyPools)
        {
            if (pool.enemyPrefab.name == enemy.gameObject.name.Replace("(Clone)", ""))
            {
                pool.pool.Enqueue(enemy.gameObject);
                return;
            }
        }

        // Если пул не найден, создаем новый
        EnemyPool newPool = new EnemyPool { enemyPrefab = Resources.Load<GameObject>(enemy.gameObject.name.Replace("(Clone)", "")) };
        enemyPools.Add(newPool);
        newPool.pool.Enqueue(enemy.gameObject);
    }
}