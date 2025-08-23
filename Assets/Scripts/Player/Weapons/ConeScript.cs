using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class ConeScript : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    private PlayerStats stats;
    private WaitForSeconds shootDelay;
    private Transform playerTransform;
    private WeaponScript weapon;


    [Header("Базовые характеристики")]
    public bool isActive = false;
    public float baseShootInterval = 2f;
    public float baseSpeed = 5f;
    public float baseLifetime = 5f;
    public float baseDamage = 1f; // 1 = 100% atk
    public int basePenetrate = 0;
    public float baseSize = 1f;
    public int baseCount = 3;
    public float coneAngle = 45f;
    

    private void Start()
    {
        playerTransform = transform;
        stats = GetComponent<PlayerStats>();
        shootDelay = new WaitForSeconds(baseShootInterval * (1 - stats.cdRed / 100));
        weapon = GetComponent<WeaponScript>();
    }

    public void Activation()
    {
        isActive = true;
        StartCoroutine(ShootRoutine());
    }

    private IEnumerator ShootRoutine()
    {
        while (isActive)
        {
            ShootAtNearestEnemy();
            shootDelay = new WaitForSeconds(baseShootInterval * (1 - stats.cdRed / 100));
            yield return shootDelay;
        }
    }

    

    private void ShootAtNearestEnemy()
    {
        GameObject nearestEnemy = weapon.FindNearestEnemy();
        if (nearestEnemy == null) return;

        Vector2 direction = (nearestEnemy.transform.position - playerTransform.position).normalized;
        SpawnConeProjectiles(direction);
    }

    
    private void SpawnConeProjectiles(Vector2 mainDirection)
    {
        int projectileCount = baseCount + stats.addProjectile;

        if (projectileCount == 1)
        {
            weapon.SpawnSingleProjectile(mainDirection, projectilePrefab, baseSpeed, baseLifetime, baseDamage, basePenetrate);
            return;
        }

        // Запускаем корутину для создания снарядов с задержками
        StartCoroutine(SpawnProjectilesWithDelay(mainDirection, projectileCount));
    }

    private IEnumerator SpawnProjectilesWithDelay(Vector2 mainDirection, int projectileCount)
    {
        bool extra = false;
        if (projectileCount % 2 == 0)
        {
            projectileCount -= 1;
            extra = true;
        }

        float angleStep = coneAngle / (projectileCount - 1);
        float startAngle = -coneAngle / 2f;
        float delay = 0.1f * (1 - stats.cdRed / 100);
        // Создаем первый снаряд сразу
        Vector2 firstDirection = RotateVector2(mainDirection, startAngle + (angleStep * (projectileCount / 2)));
        weapon.SpawnSingleProjectile(firstDirection, projectilePrefab, baseSpeed, baseLifetime, baseDamage, basePenetrate);

        // Ждем перед созданием остальных
        yield return new WaitForSeconds(delay);

        // Создаем остальные снаряды попарно с задержками
        for (int pair = 1; pair <= (projectileCount - 1) / 2; pair++)
        {
            // Создаем пару снарядов (левый и правый)
            int leftIndex = (projectileCount / 2) - pair;
            int rightIndex = (projectileCount / 2) + pair;

            if (leftIndex >= 0)
            {
                Vector2 leftDirection = RotateVector2(mainDirection, startAngle + (angleStep * leftIndex));
                weapon.SpawnSingleProjectile(leftDirection, projectilePrefab, baseSpeed, baseLifetime, baseDamage, basePenetrate);
            }

            if (rightIndex < projectileCount)
            {
                Vector2 rightDirection = RotateVector2(mainDirection, startAngle + (angleStep * rightIndex));
                weapon.SpawnSingleProjectile(rightDirection, projectilePrefab, baseSpeed, baseLifetime, baseDamage, basePenetrate);
            }

            // Фикс четного числа снарядов
            if (extra && pair == (projectileCount - 1) / 2)
                weapon.SpawnSingleProjectile(firstDirection, projectilePrefab, baseSpeed, baseLifetime, baseDamage, basePenetrate);

            // Ждем перед следующей парой
            yield return new WaitForSeconds(delay);
        }
    }

    private Vector2 RotateVector2(Vector2 vector, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        );
    }

    
}
