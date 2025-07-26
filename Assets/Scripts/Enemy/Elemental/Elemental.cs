using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using System.Collections;

public class Elemental : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    private WaitForSeconds shootDelay;
    private EnemyDamage enemyDamage;


    [Header("Cyclone settings")]
    [SerializeField] private bool isElite = false;
    [SerializeField] private float baseShootInterval = 5f;
    [SerializeField] private float baseProjectileSpeed = 2;
    [SerializeField] private float baseProjectileLifetime = 15;
    private void Start()
    {
        enemyDamage = GetComponent<EnemyDamage>();
        shootDelay = new WaitForSeconds(baseShootInterval);
        if (isElite)
            StartCoroutine(ShootRoutine());
    }

    private IEnumerator ShootRoutine()
    {
        while (true)
        {
            ShootAtPlayer();
            yield return shootDelay;
        }
    }

    private void ShootAtPlayer()
    {
        Vector2 direction = (PlayerMovement.Instance.transform.position - transform.position).normalized;
        SpawnProjectile(direction);
    }


    private void SpawnProjectile(Vector2 direction)
    {
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Cyclone projectileScript = projectile.GetComponent<Cyclone>();

        if (projectileScript != null)
        {
            projectileScript.speed = baseProjectileSpeed;
            projectileScript.lifetime = baseProjectileLifetime;
            projectileScript.damage = 5 * enemyDamage.damage;
            projectileScript.SetDirection(direction);
        }
    }

}
