using UnityEngine;
using System.Collections;

public class MinigunScript : WeaponBase
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;

    [Header("Minigun Specific Settings")]
    public bool isActive = false;
    public int aimTarget = 0;
    public float coneAngle = 45f;

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
        float cooldownReduction = stats != null ? (1 - stats.cdRed / 100) : 1f;
        shootDelay = new WaitForSeconds(currentShootInterval * cooldownReduction);
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
        int projectileCount = currentCount + (stats != null ? stats.addProjectile : 0);

        for (int i = 0; i < projectileCount; i++)
        {
            float deviationAngle = coneAngle / 2;
            float randomDeviation = Random.Range(-deviationAngle, deviationAngle);
            Vector2 shotDirection = RotateVector2(mainDirection, randomDeviation);
            SpawnSingleProjectile(shotDirection);
        }
    }

    private void SpawnSingleProjectile(Vector2 direction)
    {
        if (projectilePrefab == null) return;

        GameObject projectile = Instantiate(projectilePrefab, playerTransform.position, Quaternion.identity);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Projectile projectileScript = projectile.GetComponent<Projectile>();
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
            projectileScript.SetDirection(direction);
        }
    }

    private Vector2 RotateVector2(Vector2 vector, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);
        return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos);
    }
}