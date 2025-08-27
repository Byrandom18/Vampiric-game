using UnityEngine;
using System.Collections;

public class SpreadScript : WeaponBase
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;

    [Header("Spread Specific Settings")]
    public bool isActive = false;
    public float spreadAngle = 45f;

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
            SpawnCircleProjectilesUniform();
            UpdateShootDelay();
            yield return shootDelay;
        }
    }

    private void UpdateShootDelay()
    {
        float cooldownReduction = stats != null ? (1 - stats.cdRed / 100) : 1f;
        shootDelay = new WaitForSeconds(currentShootInterval * cooldownReduction);
    }

    private void SpawnCircleProjectilesUniform()
    {
        int projectileCount = currentCount + (stats != null ? stats.addProjectile : 0);
        if (projectileCount <= 0) return;

        float angleStep = 360f / projectileCount;

        for (int i = 0; i < projectileCount; i++)
        {
            float baseAngle = i * angleStep;
            float randomSpread = Random.Range(-spreadAngle, spreadAngle);
            float finalAngle = baseAngle + randomSpread;

            Vector2 shotDirection = AngleToVector2(finalAngle);
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

    private Vector2 AngleToVector2(float angle)
    {
        float angleRad = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }
}