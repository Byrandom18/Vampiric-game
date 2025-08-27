using UnityEngine;

public class ExplosiveEnemy : MonoBehaviour
{
    public Explosion explosionPrefab;
    private EnemyDamage enemyDamage;
    [SerializeField] private float explosionRadius = 1f;
    [SerializeField] private float explosionTime = 0.3f;


    private void Start()
    {
        enemyDamage = GetComponent<EnemyDamage>();
    }



    public void Death()
    {
        CreateExplosion(transform.position);
        //Destroy(gameObject);
    }

    public void CreateExplosion(Vector2 position)
    {
        // Создаем экземпляр взрыва
        Explosion explosion = Instantiate(explosionPrefab, position, Quaternion.identity);

        // Можно настроить параметры взрыва:
        explosion.maxRadius = explosionRadius;
        explosion.fillTime = explosionTime;
        explosion.damage = enemyDamage.damage;
    }
}
