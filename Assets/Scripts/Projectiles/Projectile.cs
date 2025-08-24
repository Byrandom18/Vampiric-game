using UnityEngine;

public class Projectile : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float speed = 8f;
    public float lifetime = 3f;
    public float damage = 1;
    public int penetrate = 1;
    public float defShred = 0;

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
    public void SetColor(Color newColor)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = newColor;
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
            
        }
        if (!enemyLaunch)
        {
            if (collision.CompareTag("Enemy"))
            {
                EnemyDamage enemy = collision.GetComponent<EnemyDamage>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage, defShred);
                }
                penetrate -= 1;
                if (penetrate <= 0)
                {
                    Destroy(gameObject);
                }
                
            }
            
        }

    }
}
