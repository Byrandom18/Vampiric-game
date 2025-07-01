using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    public enum ItemType { Exp, Gold, Magnet, Equipment }
    public ItemType itemType;

    [Header("Value Settings")]
    public float value = 1; // Для опыта/золота
    //public EquipmentObject equipment; // Для экипировки

    [Header("Movement Settings")]
    public float attractionRadius = 2f;
    public float speed = 12f;

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
        }
    }
}
