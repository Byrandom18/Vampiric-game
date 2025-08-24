using System.ComponentModel;
using UnityEngine;
using System.Collections;

public class HomingSpreadScript : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    private PlayerStats stats;
    private WaitForSeconds shootDelay;
    private Transform playerTransform;

    private WeaponScript weapon;

    [Header("Базовые характеристики")]
    public bool isActive = false;
    public float baseShootInterval = 3f;
    public float baseSpeed = 6f;
    public float baseLifetime = 5f;
    public float baseDamage = 1f; // 1 = 100% atk
    public int basePenetrate = 1;
    public float baseSize = 1f;
    public int baseCount = 3;
    public float baseDefShred = 1;
    public float baseRotation = 3f;

    private void Start()
    {
        weapon = GetComponent<WeaponScript>();
        playerTransform = transform;
        stats = GetComponent<PlayerStats>();
        shootDelay = new WaitForSeconds(baseShootInterval * (1 - stats.cdRed / 100));
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
            float cdr = (1 - stats.cdRed / 100);
            shootDelay = new WaitForSeconds(baseShootInterval * cdr);
            SpawnCircleProjectilesUniform();
            yield return shootDelay;
        }
    }
    private void SpawnCircleProjectilesUniform()
    {
        int projectileCount = baseCount + (stats != null ? stats.addProjectile : 0);

        if (projectileCount <= 0) return;

        float angleStep = 360f / projectileCount;
        float spreadAngle = 15f; // Разброс

        for (int i = 0; i < projectileCount; i++)
        {
            // Основной угол для равномерного распределения (начинаем с 0 градусов)
            float baseAngle = i * angleStep;

            float randomSpread = Random.Range(-spreadAngle, spreadAngle);
            float finalAngle = baseAngle + randomSpread;

            Vector2 shotDirection = AngleToVector2(finalAngle);
            //SpawnSingleProjectile(shotDirection);
            SpawnHomingProjectile(shotDirection);
        }
    }

    private Vector2 AngleToVector2(float angle)
    {
        float angleRad = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    public void SpawnHomingProjectile(Vector2 direction)
    {
        GameObject projectile = Instantiate(projectilePrefab, playerTransform.position, Quaternion.identity);

        // Поворачиваем снаряд в направлении движения
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        SeekingMissile projectileScript = projectile.GetComponent<SeekingMissile>();

        float scaleModifier = baseSize * (1 + stats.areaMod / 100);
        projectile.transform.localScale = Vector3.one * scaleModifier;

        if (projectileScript != null)
        {
            projectileScript.speed = baseSpeed * (1 + stats.projectileSpeed / 100);
            projectileScript.lifetime = baseLifetime * (1 + stats.durations / 100);
            projectileScript.damage = baseDamage * stats.atk;
            projectileScript.penetrate = 1 + basePenetrate + stats.penetrationBoost;
            projectileScript.defShred = baseDefShred + stats.defShred;
            projectileScript.rotationSpeed = baseRotation;
            //projectileScript.SetDirection(direction);
        }
    }
}
