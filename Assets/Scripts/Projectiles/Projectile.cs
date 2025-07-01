using UnityEngine;

public class Projectile : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float speed = 8f;
    public float lifetime = 3f;
    public float damage = 1;
    public float penetrate = 1;

    public bool enemyLaunch = false;

    private Vector2 direction;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D not found on projectile!");
            enabled = false; // Отключаем скрипт, если нет Rigidbody2D
        }
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector2 dir)
    {
        if (rb == null) return; // Защита от NullReference

        direction = dir.normalized;
        rb.linearVelocity = direction * speed;
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (enemyLaunch)
        {
            if (collision.CompareTag("Player"))
            {
                PlayerStats player = collision.GetComponent<PlayerStats>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                }
                Destroy(gameObject);
            }
            else if (!collision.CompareTag("Enemy"))
            {
                Destroy(gameObject);
            }
        }
        if (!enemyLaunch)
        {
            if (collision.CompareTag("Enemy"))
            {
                EnemyDamage enemy = collision.GetComponent<EnemyDamage>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
                penetrate -= 1;
                if (penetrate <= 0)
                {
                    Destroy(gameObject);
                }
                
            }
            else if (!collision.CompareTag("Player"))
            {
                Destroy(gameObject);
            }
        }

    }
}
