using UnityEngine;
using System.Collections;

public class BouncingScript : WeaponBase
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;

    [Header("Bouncing Specific Settings")]
    public bool isActive = false;
    public int aimTarget = 4;

    private PlayerStats stats;
    private WaitForSeconds shootDelay;
    private Transform playerTransform;
    private WeaponScript weapon;

    protected override void Start()
    {
        base.Start();
        playerTransform = transform;
        stats = GetComponent<PlayerStats>();
        weapon = GetComponent<WeaponScript>();
        isActive = false;
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
        int projectileCount = currentCount + (stats != null ? stats.addProjectile : 0);
        float cooldownReduction = stats != null ? (1 - stats.cdRed / 100) : 1f;
        float delay = currentShootInterval * cooldownReduction / (1 + projectileCount / 2f);
        shootDelay = new WaitForSeconds(delay);
    }

    private void ShootAtTargetEnemy()
    {
        Vector2 direction = Vector2.zero;
        GameObject targetEnemy = null;

        switch (aimTarget)
        {
            case 0: targetEnemy = weapon.FindNearestEnemy(); break;
            case 1: targetEnemy = weapon.FindRandomEnemy(); break;
            case 2: targetEnemy = weapon.GetMouseDirectionAsTarget(); break;
            case 3: targetEnemy = weapon.FindWeakestEnemy(); break;
            case 4: targetEnemy = weapon.FindStrongestEnemy(); break;
        }

        if (targetEnemy == null)
        {
            direction = Vector2.right;
        }
        else
        {
            direction = (targetEnemy.transform.position - playerTransform.position).normalized;
        }

        SpawnProjectiles(direction);
    }

    private void SpawnProjectiles(Vector2 mainDirection)
    {
        SpawnSingleProjectile(mainDirection);
    }

    private void SpawnSingleProjectile(Vector2 direction)
    {
        if (projectilePrefab == null) return;

        GameObject projectile = Instantiate(projectilePrefab, playerTransform.position, Quaternion.identity);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Bouncing projectileScript = projectile.GetComponent<Bouncing>();
        float sizeModifier = currentSize * (stats != null ? (1 + stats.areaMod / 100) : 1f);
        projectile.transform.localScale = Vector3.one * sizeModifier;

        if (projectileScript != null)
        {
            float finalSpeed = currentSpeed * (stats != null ? (1 + stats.projectileSpeed / 100) : 1f);
            float finalLifetime = currentLifetime * (stats != null ? (1 + stats.durations / 100) : 1f);
            float finalDamage = currentDamage * (stats != null ? stats.atk : 1f) * (stats != null ? (1 + stats.damageMod / 100) : 1f);
            int finalPenetrate = currentPenetrate + (stats != null ? stats.penetrationBoost : 0);
            float finalDefShred = currentDefShred + (stats != null ? stats.defShred : 0f);

            projectileScript.speed = finalSpeed;
            projectileScript.lifetime = finalLifetime;
            projectileScript.damage = finalDamage;
            projectileScript.penetrate = finalPenetrate;
            projectileScript.defShred = finalDefShred;
        }
    }
}