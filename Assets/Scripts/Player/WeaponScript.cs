using UnityEngine;
using System.Collections;

public class WeaponScript : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;

    private PlayerStats stats;

    private Transform playerTransform;
    private WaitForSeconds shootDelay;

    [Header ("Базовая атака")]
    [SerializeField] private float baseShootInterval = 2f;
    [SerializeField] private float baseProjectileSpeed = 5;
    [SerializeField] private float baseProjectileLifetime = 5;




    private void Start()
    {
        stats = GetComponent<PlayerStats>();
        playerTransform = transform;
        shootDelay = new WaitForSeconds(baseShootInterval);
        StartCoroutine(ShootRoutine());
    }

    private IEnumerator ShootRoutine()
    {
        while (true)
        {
            ShootAtNearestEnemy();
            yield return shootDelay;
        }
    }

    private void ShootAtNearestEnemy()
    {
        GameObject nearestEnemy = FindNearestEnemy();
        if (nearestEnemy == null) return;

        Vector2 direction = (nearestEnemy.transform.position - playerTransform.position).normalized;
        SpawnProjectile(direction);
    }


    private void SpawnProjectile(Vector2 direction)
    {
        GameObject projectile = Instantiate(projectilePrefab, playerTransform.position, Quaternion.identity);
        Projectile projectileScript = projectile.GetComponent<Projectile>();

        if (projectileScript != null)
        {
            projectileScript.speed = baseProjectileSpeed * (1 + stats.projectileSpeed / 100);
            projectileScript.lifetime = baseProjectileLifetime * (1 + stats.durations);
            projectileScript.damage = 1 * stats.atk; // Урон из PlayerStats
            projectileScript.penetrate = 1 + stats.penetrationBoost;
            projectileScript.SetDirection(direction);
        }
    }





    private GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0) return null;

        GameObject nearestEnemy = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(playerTransform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }
}
