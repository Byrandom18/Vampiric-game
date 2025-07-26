using UnityEngine;
using System.Collections;

public class Drone : MonoBehaviour
{
    private WaitForSeconds shootDelay;
    [SerializeField] private GameObject projectilePrefab;
    private EnemyDamage enemyDamage;

    [Header("Shooting Settings")]
    [SerializeField] private bool isMine = false;
    [SerializeField] private int projectilePerShoot = 4;
    [SerializeField] private float baseShootInterval = 3f;
    [SerializeField] private float baseProjectileSpeed = 5;
    [SerializeField] private float baseProjectileLifetime = 5;
    void Start()
    {
        enemyDamage = GetComponent<EnemyDamage>();
        shootDelay = new WaitForSeconds(baseShootInterval);
        StartCoroutine(ShootRoutine());
    }

    private IEnumerator ShootRoutine()
    {
        while (true)
        {
            if (!isMine)
                ShootAtPlayer();
            else if (isMine)
                for (int i = 0; i < projectilePerShoot; i++)
                {
                    ShootInRandomAngle();
                    yield return new WaitForSeconds(0.1f);
                }
            yield return shootDelay;
        }
    }
    private void ShootInRandomAngle()
    {
        float randomAngle = Random.Range(0, 360);
        Vector2 direction = new Vector2(
            Mathf.Cos(randomAngle * Mathf.Deg2Rad),
            Mathf.Sin(randomAngle * Mathf.Deg2Rad)
        );
        SpawnProjectile(direction);
    }

    private void ShootAtPlayer()
    {
        Vector2 direction = (PlayerMovement.Instance.transform.position - transform.position).normalized;
        SpawnProjectile(direction);
    }
    private void SpawnProjectile(Vector2 direction)
    {
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Projectile projectileScript = projectile.GetComponent<Projectile>();

        if (projectileScript != null)
        {
            projectileScript.speed = baseProjectileSpeed;
            projectileScript.lifetime = baseProjectileLifetime;
            projectileScript.damage = 1 * enemyDamage.damage;
            projectileScript.enemyLaunch = true;
            projectileScript.SetColor(Color.red);
            projectileScript.SetDirection(direction);
        }
    }
}
