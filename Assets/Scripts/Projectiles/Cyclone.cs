using UnityEngine;

public class Cyclone : MonoBehaviour
{
    public float speed = 2f;
    public float lifetime = 3f;
    public float damage = 1;
    private Vector2 direction;
    private Rigidbody2D rb;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D not found on projectile!");
            enabled = false; // Отключаем скрипт, если нет Rigidbody2D
        }
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector2 dir)
    {
        if (rb == null) return; // Защита от NullReference

        direction = dir.normalized;
        rb.linearVelocity = direction * speed;
    }

    private void FixedUpdate()
    {
        Vector2 dir = (PlayerMovement.Instance.transform.position - transform.position).normalized;
        
        float distanceToPlayer = Vector2.Distance(transform.position, PlayerMovement.Instance.transform.position);
        if ((rb.linearVelocityX < 0 && dir.x > 0) || (rb.linearVelocityX > 0 && dir.x < 0))
            rb.linearVelocityX /= 1.03f;

        if ((rb.linearVelocityY < 0 && dir.y > 0) || (rb.linearVelocityY > 0 && dir.y < 0))
            rb.linearVelocityY /= 1.03f;
        
            

        rb.AddForce(dir * speed, ForceMode2D.Force);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerStats player = collision.GetComponent<PlayerStats>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            //Destroy(gameObject);
        }
        
    }
}
