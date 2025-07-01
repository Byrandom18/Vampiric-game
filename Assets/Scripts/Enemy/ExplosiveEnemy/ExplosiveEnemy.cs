using UnityEngine;

public class ExplosiveEnemy : MonoBehaviour
{
    public Explosion explosionPrefab;





    public void Death()
    {
        CreateExplosion(transform.position);
        Destroy(gameObject);
    }

    public void CreateExplosion(Vector2 position)
    {
        // Создаем экземпляр взрыва
        Explosion explosion = Instantiate(explosionPrefab, position, Quaternion.identity);

        // Можно настроить параметры взрыва:
        explosion.maxRadius = 3f;
        explosion.fillTime = 0.3f;
        explosion.damage = 15f;
    }
}
