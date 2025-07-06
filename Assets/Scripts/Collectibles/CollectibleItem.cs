using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    public enum ItemType { Exp, Gold, Magnet, Heart, Bomb, Equipment }
    public ItemType itemType;

    [Header("Value Settings")]
    public float value = 1; // Для опыта/золота
    //public EquipmentObject equipment; // Для экипировки

    [Header("Movement Settings")]
    public float attractionRadius = 2f;
    public float speed = 8f;

    [Header("Bomb Settings")]
    [SerializeField] private float explosionRadius = 5;
    [SerializeField] private float bombDamageMod = 5;

    private Transform player;
    private bool isAttracted = false;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Автоматическое притягивание при близком расстоянии
        if (distanceToPlayer <= attractionRadius && !isAttracted)
        {
            isAttracted = true;
        }

        // Движение к игроку, если предмет притягивается
        if (isAttracted)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * speed;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.isTrigger)
        {
            CollectItem();
        }
    }
    private void CollectItem()
    {
        switch (itemType)
        {
            case ItemType.Exp:
                PlayerStats.Instance.AddExp(value);
                break;
            case ItemType.Gold:
                PlayerStats.Instance.AddGold(value);
                break;
            case ItemType.Magnet:
                MagnetActivation();
                break;
            case ItemType.Heart:
                HeartHeal();
                break;
            case ItemType.Bomb:
                Explode();
                break;
            case ItemType.Equipment:
                //PlayerStats.Instance.AddEquipment(equipment);
                break;
        }

        //// Возвращаем в пул или уничтожаем
        //if (ItemPool.Instance != null)
        //{
        //    ItemPool.Instance.ReturnToPool(gameObject, itemType);
        //}
        //else
        //{
        //    Destroy(gameObject);
        //}
        Destroy(gameObject);
    }
    public void ForceAttract()
    {
        isAttracted = true;
    }

    public void MagnetActivation()
    {
        CollectibleItem[] items = FindObjectsByType<CollectibleItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var item in items)
        {
            item.ForceAttract();
            item.speed *= 2;
        }
    }

    public void HeartHeal()
    {
        PlayerStats.Instance.AddHealth();
    }
    private void Explode()
    {
        // 1. Находим всех врагов в радиусе
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            transform.position,
            explosionRadius,
            LayerMask.GetMask("Enemy") // Используйте слой или тег
        );

        // 2. Наносим урон каждому врагу
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy") && enemy.isTrigger)
            {

                enemy.GetComponent<EnemyDamage>().TakeDamage(PlayerStats.Instance.atk * bombDamageMod);
            }
        }
    }
}