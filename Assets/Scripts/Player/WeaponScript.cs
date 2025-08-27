using System.Collections;
using System.Linq;
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

    private ArmorBreakScript armorBreak;
    public bool isArmorBreakActive = false;

    private MinigunScript minigun;
    public bool isMinigunActive = false;

    private HomingSpreadScript homing;
    public bool isHomingActive = false;

    private BouncingScript bouncing;
    public bool isBouncingActive = false;

    private void Start()
    {
        playerTransform = transform;
        stats = GetComponent<PlayerStats>();

        cone = GetComponent<ConeScript>();
        spread = GetComponent<SpreadScript>();
        armorBreak = GetComponent<ArmorBreakScript>();
        minigun = GetComponent<MinigunScript>();
        homing = GetComponent<HomingSpreadScript>();
        bouncing = GetComponent<BouncingScript>();
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

        if (isArmorBreakActive && !armorBreak.isActive)
            armorBreak.Activation();
        else if (!isArmorBreakActive && armorBreak.isActive)
            armorBreak.isActive = false;

        if (isMinigunActive && !minigun.isActive)
            minigun.Activation();
        else if (!isMinigunActive && minigun.isActive)
            minigun.isActive = false;

        if (isHomingActive && !homing.isActive)
            homing.Activation();
        else if (!isHomingActive && homing.isActive)
            homing.isActive = false;

        if (isBouncingActive && !bouncing.isActive)
            bouncing.Activation();
        else if (!isBouncingActive && bouncing.isActive)
            bouncing.isActive = false;
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
    public GameObject FindRandomEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0) return null;

        // Фильтруем null объекты
        var validEnemies = enemies.Where(enemy => enemy != null).ToArray();
        if (validEnemies.Length == 0) return null;

        // Выбираем случайного врага
        int randomIndex = Random.Range(0, validEnemies.Length);
        return validEnemies[randomIndex];
    }


    // Или возвращаем GameObject с направлением (если нужно)
    public GameObject GetMouseDirectionAsTarget()
    {
        // Создаем временный объект для представления направления мыши
        GameObject mouseTarget = new GameObject("MouseTarget");
        mouseTarget.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Destroy(mouseTarget, 1f);
        return mouseTarget;
    }

    public GameObject FindWeakestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0) return null;

        GameObject weakestEnemy = null;
        float minHealth = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;

            EnemyDamage enemyDamage = enemy.GetComponent<EnemyDamage>();
            if (enemyDamage == null) continue;

            if (enemyDamage.health < minHealth)
            {
                minHealth = enemyDamage.health;
                weakestEnemy = enemy;
            }
        }

        return weakestEnemy;
    }

    public GameObject FindStrongestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0) return null;

        GameObject strongestEnemy = null;
        float maxHealth = -Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;

            // Получаем компонент здоровья врага
            EnemyDamage enemyDamage = enemy.GetComponent<EnemyDamage>();
            if (enemyDamage == null) continue;

            // Сравниваем здоровье
            if (enemyDamage.health > maxHealth)
            {
                maxHealth = enemyDamage.health;
                strongestEnemy = enemy;
            }
        }

        return strongestEnemy;
    }



    public void SpawnSingleProjectile(Vector2 direction, GameObject projectilePrefab, float baseSpeed, float baseLifetime, float baseDamage, int basePenetrate, float baseDefShred, float baseSize)
    {
        GameObject projectile = Instantiate(projectilePrefab, playerTransform.position, Quaternion.identity);

        // Поворачиваем снаряд в направлении движения
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Projectile projectileScript = projectile.GetComponent<Projectile>();

        float scaleModifier = baseSize * (1 + stats.areaMod / 100);
        projectile.transform.localScale = Vector3.one * scaleModifier;

        if (projectileScript != null)
        {
            projectileScript.speed = baseSpeed * (1 + stats.projectileSpeed / 100);
            projectileScript.lifetime = baseLifetime * (1 + stats.durations / 100);
            projectileScript.damage = baseDamage * stats.atk * (1 + stats.damageMod / 100);
            projectileScript.penetrate = 1 + basePenetrate + stats.penetrationBoost;
            projectileScript.defShred = baseDefShred + stats.defShred;
            projectileScript.SetDirection(direction);
        }
    }

}
