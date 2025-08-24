using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class ArmorBreakScript : MonoBehaviour
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
        float newDamage = baseDamage * (1 + projectileCount / 2);


        weapon.SpawnSingleProjectile(mainDirection, projectilePrefab, baseSpeed, baseLifetime, newDamage, basePenetrate, baseDefShred, baseSize);
    }
}
