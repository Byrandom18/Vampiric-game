using System.Collections.Generic;
using UnityEngine;

public class Bouncing : MonoBehaviour
{
    [Header("Bounce Settings")]
    public int penetrate = 4;
    public float bounceSearchRadius = 10f;

    [Header("Projectile Settings")]
    public float speed = 15f;
    public float damage = 20f;
    public float lifetime = 5f;
    public float defShred = 0;

    private int currentBounces;
    private Vector2 currentDirection;
    private Rigidbody2D rb;
    private List<GameObject> hitEnemies = new List<GameObject>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    private void Start()
    {
        // Начальное движение в направлении spawn
        currentDirection = transform.right;
        rb.linearVelocity = currentDirection * speed;

        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        // Поддерживаем постоянную скорость и направление
        rb.linearVelocity = currentDirection * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            HandleEnemyHit(collision.gameObject);
        }
        
    }

    private void HandleEnemyHit(GameObject enemy)
    {
        // Проверяем, не попадали ли уже в этого врага
        if (hitEnemies.Contains(enemy))
            return;

        // Добавляем врага в список пораженных
        hitEnemies.Add(enemy);

        // Наносим урон
        EnemyDamage enemyDamage = enemy.GetComponent<EnemyDamage>();
        if (enemyDamage != null)
        {
            enemyDamage.TakeDamage(damage, defShred);
        }

        // Увеличиваем счетчик отскоков
        currentBounces++;

        // Проверяем лимит отскоков
        if (currentBounces >= penetrate)
        {
            Destroy(gameObject);
            return;
        }

        // Ищем нового случайного врага для отскока
        FindNewTargetForBounce();
    }

    private void FindNewTargetForBounce()
    {
        // Ищем всех врагов в радиусе
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, bounceSearchRadius, LayerMask.GetMask("Enemy"));

        // Фильтруем уже пораженных врагов и null объекты
        List<Transform> availableTargets = new List<Transform>();

        foreach (Collider2D enemyCollider in enemies)
        {
            if (enemyCollider != null && !hitEnemies.Contains(enemyCollider.gameObject))
            {
                availableTargets.Add(enemyCollider.transform);
            }
        }

        // Если есть доступные цели - выбираем случайную
        if (availableTargets.Count > 0)
        {
            int randomIndex = Random.Range(0, availableTargets.Count);
            Transform newTarget = availableTargets[randomIndex];

            // Вычисляем новое направление к цели
            Vector2 newDirection = (newTarget.position - transform.position).normalized;
            currentDirection = newDirection;

            // Обновляем rotation снаряда
            float angle = Mathf.Atan2(newDirection.y, newDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            // Обновляем velocity
            rb.linearVelocity = currentDirection * speed;
        }
        else
        {
            // Если целей нет - уничтожаем снаряд
            Destroy(gameObject);
        }
    }

    // Метод для установки параметров извне
    public void SetParameters(float newSpeed, float newDamage, int newBounces, float newLifetime)
    {
        speed = newSpeed;
        damage = newDamage;
        penetrate = newBounces;
        lifetime = newLifetime;
    }

    
}
