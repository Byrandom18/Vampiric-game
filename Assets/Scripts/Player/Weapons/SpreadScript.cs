using System.ComponentModel;
using UnityEngine;
using System.Collections;

public class SpreadScript : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    private PlayerStats stats;
    private WaitForSeconds shootDelay;
    private Transform playerTransform;

    private WeaponScript weapon;

    [Header("Базовые характеристики")]
    public bool isActive = false;
    public float baseShootInterval = 3f;
    public float baseSpeed = 4f;
    public float baseLifetime = 5f;
    public float baseDamage = 1f; // 1 = 100% atk
    public int basePenetrate = 1;
    public float baseSize = 1f;
    public int baseCount = 4;


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
        float spreadAngle = 45f; // Разброс

        for (int i = 0; i < projectileCount; i++)
        {
            // Основной угол для равномерного распределения (начинаем с 0 градусов)
            float baseAngle = i * angleStep;

            float randomSpread = Random.Range(-spreadAngle, spreadAngle);
            float finalAngle = baseAngle + randomSpread;

            Vector2 shotDirection = AngleToVector2(finalAngle);
            //SpawnSingleProjectile(shotDirection);
            weapon.SpawnSingleProjectile(shotDirection, projectilePrefab, baseSpeed, baseLifetime, baseDamage, basePenetrate);
        }
    }

    private Vector2 AngleToVector2(float angle)
    {
        float angleRad = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

}
