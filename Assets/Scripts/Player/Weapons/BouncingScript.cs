using UnityEngine;
using System.Collections;

public class BouncingScript : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    private PlayerStats stats;
    private WaitForSeconds shootDelay;
    private Transform playerTransform;
    private WeaponScript weapon;
    public int aimTarget = 4;

    [Header("Базовые характеристики")]
    public bool isActive = false;
    public float baseShootInterval = 10f;
    public float baseSpeed = 12f;
    public float baseLifetime = 5f;
    public float baseDamage = 10f; // 1 = 100% atk
    public int basePenetrate = 0;
    public float baseSize = 1f;
    public int baseCount = 1;
    public float baseDefShred = 1000;


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
            ShootAtTargetEnemy();
            shootDelay = new WaitForSeconds(baseShootInterval * (1 - stats.cdRed / 100) / (1 + (baseCount + stats.addProjectile) / 2));
            yield return shootDelay;
        }
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
        if (targetEnemy == null) return;

        direction = (targetEnemy.transform.position - playerTransform.position).normalized;
        SpawnProjectiles(direction);
    }
    private void SpawnProjectiles(Vector2 mainDirection)
    {
        
        SpawnSingleProjectile(mainDirection);
    }
    public void SpawnSingleProjectile(Vector2 direction)
    {
        GameObject projectile = Instantiate(projectilePrefab, playerTransform.position, Quaternion.identity);

        // Поворачиваем снаряд в направлении движения
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Bouncing projectileScript = projectile.GetComponent<Bouncing>();

        float scaleModifier = baseSize * (1 + stats.areaMod / 100);
        projectile.transform.localScale = Vector3.one * scaleModifier;

        if (projectileScript != null)
        {
            projectileScript.speed = baseSpeed * (1 + stats.projectileSpeed / 100);
            projectileScript.lifetime = baseLifetime * (1 + stats.durations / 100);
            projectileScript.damage = baseDamage * stats.atk;
            projectileScript.penetrate = 1 + basePenetrate + stats.penetrationBoost;
            projectileScript.defShred = baseDefShred + stats.defShred;
        }
    }
}
