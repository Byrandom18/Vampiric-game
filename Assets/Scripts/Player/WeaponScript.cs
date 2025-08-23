using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class WeaponScript : MonoBehaviour
{
    private Transform playerTransform;
    private PlayerStats stats;
    private ConeScript cone;
    public bool isConeActive = false;

    private SpreadScript spread;
    public bool isSpreadActive = false;

    private void Start()
    {
        playerTransform = transform;
        stats = GetComponent<PlayerStats>();

        cone = GetComponent<ConeScript>();
        spread = GetComponent<SpreadScript>();
    }

    private void Update()
    {
        WeaponTrigger();
    }

    public void WeaponTrigger()
    {
        if (isConeActive && !cone.isActive)
            cone.Activation();
        else if (!isConeActive && cone.isActive)
            cone.isActive = false;

        if (isSpreadActive && !spread.isActive)
            spread.Activation();
        else if (!isSpreadActive && spread.isActive)
            spread.isActive = false;
    }

    public GameObject FindNearestEnemy()
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

    public void SpawnSingleProjectile(Vector2 direction, GameObject projectilePrefab, float baseSpeed, float baseLifetime, float baseDamage, int basePenetrate)
    {
        GameObject projectile = Instantiate(projectilePrefab, playerTransform.position, Quaternion.identity);

        // ѕоворачиваем снар€д в направлении движени€
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Projectile projectileScript = projectile.GetComponent<Projectile>();

        float scaleModifier = 1 + stats.areaMod / 100;
        projectile.transform.localScale = Vector3.one * scaleModifier;

        if (projectileScript != null)
        {
            projectileScript.speed = baseSpeed * (1 + stats.projectileSpeed / 100);
            projectileScript.lifetime = baseLifetime * (1 + stats.durations / 100);
            projectileScript.damage = baseDamage * stats.atk;
            projectileScript.penetrate = 1 + basePenetrate + stats.penetrationBoost;
            projectileScript.SetDirection(direction);
        }
    }

}
