using UnityEngine;
using System.Collections;

public class MinigunScript : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    private PlayerStats stats;
    private WaitForSeconds shootDelay;
    private Transform playerTransform;
    private WeaponScript weapon;

    public int aimTarget = 0;

    [Header("Базовые характеристики")]
    public bool isActive = false;
    public float baseShootInterval = 0.5f;
    public float baseSpeed = 5f;
    public float baseLifetime = 5f;
    public float baseDamage = 1f; // 1 = 100% atk
    public int basePenetrate = 0;
    public float baseSize = 1f;
    public int baseCount = 1;
    public float coneAngle = 45f;
    public float baseDefShred = 0;

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
            shootDelay = new WaitForSeconds(baseShootInterval * (1 - stats.cdRed / 100));
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
        int projectileCount = baseCount + stats.addProjectile;
        for (int i = 1; i <= projectileCount; i++)
        {
            float deviationAngle = coneAngle / 2;
            float randomDeviation = Random.Range(-deviationAngle, deviationAngle);
            Vector2 shotDirection = RotateVector2(mainDirection, randomDeviation);
            weapon.SpawnSingleProjectile(shotDirection, projectilePrefab, baseSpeed, baseLifetime, baseDamage, basePenetrate, baseDefShred, baseSize);
        }
    }
    private Vector2 RotateVector2(Vector2 vector, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        );
    }
}
