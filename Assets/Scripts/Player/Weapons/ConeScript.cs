using System.Collections;
using UnityEngine;

public class ConeScript : WeaponBase
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;

    [Header("Cone Specific Settings")]
    public bool isActive = false;
    public float coneAngle = 45f;
    public int aimTarget = 0;

    // Базовые характеристики теперь наследуются от WeaponBase
    // baseShootInterval, baseSpeed, baseLifetime, baseDamage,
    // basePenetrate, baseSize, baseCount, baseDefShred

    private PlayerStats stats;
    private WaitForSeconds shootDelay;
    private Transform playerTransform;
    private WeaponScript weapon;

    protected override void Start()
    {
        base.Start(); // Важно вызвать базовый Start для инициализации статов

        playerTransform = transform;
        stats = GetComponent<PlayerStats>();
        weapon = GetComponent<WeaponScript>();

        // Изначально выключаем оружие
        isActive = false;

        // Создаем начальную задержку
        UpdateShootDelay();
    }

    public void Activation()
    {
        if (!isActive)
        {
            isActive = true;
            StartCoroutine(ShootRoutine());
        }
    }

    public void Deactivation()
    {
        isActive = false;
        StopAllCoroutines();
    }

    private IEnumerator ShootRoutine()
    {
        while (isActive)
        {
            ShootAtTargetEnemy();
            UpdateShootDelay();
            yield return shootDelay;
        }
    }

    private void UpdateShootDelay()
    {
        float cooldownReduction = stats != null ? (1 - stats.cdRed / 100) : 1f;
        shootDelay = new WaitForSeconds(currentShootInterval * cooldownReduction);
    }

    private void ShootAtTargetEnemy()
    {
        Vector2 direction = Vector2.zero;
        GameObject targetEnemy = null;

        switch (aimTarget)
        {
            case 0:
                targetEnemy = weapon.FindNearestEnemy();
                break;
            case 1:
                targetEnemy = weapon.FindRandomEnemy();
                break;
            case 2:
                targetEnemy = weapon.GetMouseDirectionAsTarget();
                break;
            case 3:
                targetEnemy = weapon.FindWeakestEnemy();
                break;
            case 4:
                targetEnemy = weapon.FindStrongestEnemy();
                break;
        }

        if (targetEnemy == null)
        {
            // Если врагов нет, стреляем вперед
            direction = Vector2.right;
        }
        else
        {
            direction = (targetEnemy.transform.position - playerTransform.position).normalized;
        }

        SpawnConeProjectiles(direction);
    }

    private void SpawnConeProjectiles(Vector2 mainDirection)
    {
        int projectileCount = currentCount + (stats != null ? stats.addProjectile : 0);

        if (projectileCount <= 0) return;

        if (projectileCount == 1)
        {
            SpawnSingleProjectile(mainDirection);
            return;
        }

        StartCoroutine(SpawnProjectilesWithDelay(mainDirection, projectileCount));
    }

    private IEnumerator SpawnProjectilesWithDelay(Vector2 mainDirection, int projectileCount)
    {
        bool hasExtraProjectile = false;
        if (projectileCount % 2 == 0)
        {
            projectileCount -= 1;
            hasExtraProjectile = true;
        }

        float angleStep = coneAngle / (projectileCount - 1);
        float startAngle = -coneAngle / 2f;
        float delay = CalculateProjectileDelay();

        // Центральный снаряд
        Vector2 centerDirection = RotateVector2(mainDirection, startAngle + (angleStep * (projectileCount / 2)));
        SpawnSingleProjectile(centerDirection);

        yield return new WaitForSeconds(delay);

        // Боковые снаряды попарно
        for (int pair = 1; pair <= (projectileCount - 1) / 2; pair++)
        {
            int leftIndex = (projectileCount / 2) - pair;
            int rightIndex = (projectileCount / 2) + pair;

            if (leftIndex >= 0)
            {
                Vector2 leftDirection = RotateVector2(mainDirection, startAngle + (angleStep * leftIndex));
                SpawnSingleProjectile(leftDirection);
            }

            if (rightIndex < projectileCount)
            {
                Vector2 rightDirection = RotateVector2(mainDirection, startAngle + (angleStep * rightIndex));
                SpawnSingleProjectile(rightDirection);
            }

            // Дополнительный снаряд для четного количества
            if (hasExtraProjectile && pair == (projectileCount - 1) / 2)
            {
                SpawnSingleProjectile(centerDirection);
            }

            yield return new WaitForSeconds(delay);
        }
    }

    private float CalculateProjectileDelay()
    {
        float baseDelay = 0.1f;
        float cooldownReduction = stats != null ? (1 - stats.cdRed / 100) : 1f;
        return baseDelay * cooldownReduction;
    }

    private void SpawnSingleProjectile(Vector2 direction)
    {
        if (projectilePrefab == null) return;

        GameObject projectile = Instantiate(projectilePrefab, playerTransform.position, Quaternion.identity);

        // Поворачиваем снаряд в направлении движения
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Projectile projectileScript = projectile.GetComponent<Projectile>();

        // Рассчитываем модификатор размера
        float sizeModifier = currentSize;
        if (stats != null)
        {
            sizeModifier *= (1 + stats.areaMod / 100);
        }
        projectile.transform.localScale = Vector3.one * sizeModifier;

        if (projectileScript != null)
        {
            // Используем текущие статы из WeaponBase
            float finalSpeed = currentSpeed;
            float finalLifetime = currentLifetime;
            float finalDamage = currentDamage;
            int finalPenetrate = currentPenetrate;
            float finalDefShred = currentDefShred;

            // Применяем бонусы от статов игрока
            if (stats != null)
            {
                finalSpeed *= (1 + stats.projectileSpeed / 100);
                finalLifetime *= (1 + stats.durations / 100);
                finalDamage *= stats.atk * (stats != null ? (1 + stats.damageMod / 100) : 1f);
                finalPenetrate += stats.penetrationBoost;
                finalDefShred += stats.defShred;
            }

            projectileScript.speed = finalSpeed;
            projectileScript.lifetime = finalLifetime;
            projectileScript.damage = finalDamage;
            projectileScript.penetrate = finalPenetrate;
            projectileScript.defShred = finalDefShred;
            projectileScript.SetDirection(direction);
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

    // Метод для обновления статов извне (например, из карточек)
    public void UpdateWeaponStatsFromCard(float damageMult, float speedMult, float lifetimeMult,
                                        float sizeMult, float intervalMult, int penetrateAdd,
                                        int countAdd, float defShredAdd)
    {
        ApplyTemporaryMultipliers(damageMult, speedMult, lifetimeMult, sizeMult,
                                intervalMult, penetrateAdd, countAdd, defShredAdd);
    }

    // Метод для сброса временных модификаторов
    public void ResetWeaponStats()
    {
        ResetTemporaryMultipliers();
    }

    // Для дебаггинга
    private void OnGUI()
    {
        if (Debug.isDebugBuild && isActive)
        {
            GUI.Label(new Rect(10, 100, 300, 200),
                     $"Cone Stats:\n" +
                     $"Damage: {currentDamage}\n" +
                     $"Speed: {currentSpeed}\n" +
                     $"Count: {currentCount}\n" +
                     $"Interval: {currentShootInterval}\n" +
                     $"Active: {isActive}");
        }
    }
}