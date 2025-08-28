using System.Collections.Generic;
using UnityEngine;

public class ItemPoolManager : MonoBehaviour
{
    public static ItemPoolManager Instance;

    [System.Serializable]
    public class ItemPool
    {
        public CollectibleItem.ItemType itemType;
        public GameObject itemPrefab;
        public int poolSize = 20;
        [HideInInspector] public Queue<GameObject> pool = new Queue<GameObject>();
    }

    [Header("Item Prefabs - Drag & Drop")]
    public GameObject expPrefab;
    public GameObject goldPrefab;
    public GameObject magnetPrefab;
    public GameObject heartPrefab;
    public GameObject bombPrefab;
    public GameObject gemPrefab;

    [Header("Pool Settings")]
    public int defaultPoolSize = 20;

    [HideInInspector] public List<ItemPool> itemPools = new List<ItemPool>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePools();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePools()
    {
        // Автоматически создаем пулы на основе назначенных префабов
        CreatePoolIfPrefabExists(CollectibleItem.ItemType.Exp, expPrefab);
        CreatePoolIfPrefabExists(CollectibleItem.ItemType.Gold, goldPrefab);
        CreatePoolIfPrefabExists(CollectibleItem.ItemType.Magnet, magnetPrefab);
        CreatePoolIfPrefabExists(CollectibleItem.ItemType.Heart, heartPrefab);
        CreatePoolIfPrefabExists(CollectibleItem.ItemType.Bomb, bombPrefab);
        CreatePoolIfPrefabExists(CollectibleItem.ItemType.Gem, gemPrefab);

        if (itemPools.Count == 0)
        {
            Debug.LogError("No item pools initialized! Please assign prefabs in inspector.");
        }
    }

    private void CreatePoolIfPrefabExists(CollectibleItem.ItemType type, GameObject prefab)
    {
        if (prefab != null)
        {
            ItemPool newPool = new ItemPool
            {
                itemType = type,
                itemPrefab = prefab,
                poolSize = defaultPoolSize
            };

            // Инициализируем пул
            for (int i = 0; i < defaultPoolSize; i++)
            {
                GameObject item = Instantiate(prefab, transform);
                item.SetActive(false);
                newPool.pool.Enqueue(item);
            }

            itemPools.Add(newPool);
            Debug.Log($"Created pool for {type} with {defaultPoolSize} items");
        }
        else
        {
            Debug.LogWarning($"Prefab for {type} is not assigned!");
        }
    }

    public CollectibleItem GetItemFromPool(CollectibleItem.ItemType itemType, Vector3 position, float value = 0)
    {
        // Проверка лимитов для опыта
        if (itemType == CollectibleItem.ItemType.Exp)
        {
            if (CollectibleItem.currentItemsCount >= CollectibleItem.maxItemsOnScene)
            {
                CollectibleItem.totalStoredExp += value;
                return null;
            }
        }

        // Поиск пула
        ItemPool pool = itemPools.Find(p => p.itemType == itemType);
        if (pool == null)
        {
            Debug.LogError($"Pool for {itemType} not found! Available pools: {GetAvailablePools()}");
            return null;
        }

        // Получение объекта
        GameObject itemObject;
        if (pool.pool.Count > 0)
        {
            itemObject = pool.pool.Dequeue();
        }
        else
        {
            itemObject = Instantiate(pool.itemPrefab, transform);
        }

        itemObject.transform.position = position;
        itemObject.SetActive(true);

        CollectibleItem item = itemObject.GetComponent<CollectibleItem>();
        if (item != null && value > 0)
        {
            item.SetValue(value);
        }

        return item;
    }

    private string GetAvailablePools()
    {
        string result = "";
        foreach (var pool in itemPools)
        {
            result += pool.itemType + ", ";
        }
        return result;
    }

    public void ReturnItemToPool(CollectibleItem item)
    {
        if (item == null) return;

        ItemPool pool = itemPools.Find(p => p.itemType == item.itemType);
        if (pool != null)
        {
            item.gameObject.SetActive(false);
            pool.pool.Enqueue(item.gameObject);
        }
        else
        {
            Debug.LogWarning($"No pool found for {item.itemType}, destroying object");
            Destroy(item.gameObject);
        }
    }

    public bool TrySpawnItem(CollectibleItem.ItemType type, Vector3 position, float value = 0)
    {
        CollectibleItem item = GetItemFromPool(type, position, value);
        return item != null;
    }
}