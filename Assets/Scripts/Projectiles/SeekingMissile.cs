using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class SeekingMissile : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 6f;
    public float rotationSpeed = 5f;
    public float detectionRange = 10f;
    public float damage = 10f;
    public float lifetime = 4f;
    public float defShred = 0;
    public float penetrate = 1;
    private Transform target;
    private Vector2 movementDirection;

    private void Start()
    {
        movementDirection = transform.right;
        Destroy(gameObject, lifetime);
        InvokeRepeating("UpdateTarget", 0f, 0.3f);
        rotationSpeed += speed / 2;
    }

    private void Update()
    {
        Move();
        RotateTowardsTarget();
    }

    private void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < shortestDistance && distance <= detectionRange)
            {
                shortestDistance = distance;
                nearestEnemy = enemy.transform;
            }
        }

        target = nearestEnemy;
    }

    private void Move()
    {
        transform.Translate(movementDirection * speed * Time.deltaTime, Space.World);
    }

    private void RotateTowardsTarget()
    {
        if (target == null) return;

        // Плавный поворот к цели
        Vector2 directionToTarget = (target.position - transform.position).normalized;
        movementDirection = Vector2.Lerp(movementDirection, directionToTarget, rotationSpeed * Time.deltaTime).normalized;

        // Обновляем rotation спрайта
        float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void OnTriggerEnter2D(Collider2D collision)
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
