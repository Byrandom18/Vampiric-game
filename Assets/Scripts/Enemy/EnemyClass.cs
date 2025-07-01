using System.Collections;
using UnityEngine;

public class EnemyClass : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private float speed = 2f;
    private bool canHitInside = true;
    private EnemyDamage enemyDamage;

    [Header("Настройки сближения")]
    [SerializeField] private bool canClosing = false;
    [SerializeField] private float closingDistance = 3f;
    private bool isClosing = false;
    [SerializeField] private float forceDuration = 1;
    [SerializeField] private float forceCD = 5;
    [SerializeField] private float forcePower = 2;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyDamage = GetComponent<EnemyDamage>();
    }

    // Update is called once per frame
    private void Update()
    {
        Chase();
        
    }

    private void Chase()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, PlayerMovement.Instance.transform.position);

        // Получаем позицию игрока
        Vector2 playerPosition = PlayerMovement.Instance.transform.position;

        // Вычисляем направление (нормализованное, чтобы скорость была постоянной)
        Vector2 direction = (playerPosition - (Vector2)transform.position).normalized;

        if (canClosing && distanceToPlayer < closingDistance)
        {
            StartCoroutine(CloseCD());
        }
        if (!isClosing)
            rb.linearVelocity = direction * speed;
        if (distanceToPlayer < 0.3f && canHitInside)
            StartCoroutine(InsideCheck());



        if (direction.x > 0)
            transform.localScale = new Vector3(1, 1, 1); // Смотрит вправо
        else if (direction.x < 0)
            transform.localScale = new Vector3(-1, 1, 1); // Смотрит влево


        // перенос на противоположную часть карты если игрок далеко от моба
        if (distanceToPlayer > 30f)
        {
            transform.position += (Vector3)(playerPosition - (Vector2)transform.position).normalized * 50f;
        }
    }
    private IEnumerator InsideCheck()
    {
        canHitInside = false;
        yield return new WaitForSeconds(0.5f);
        float distanceToPlayer = Vector2.Distance(transform.position, PlayerMovement.Instance.transform.position);
        if (distanceToPlayer < 0.2f)
            PlayerStats.Instance.TakeDamage(enemyDamage.damage);
        canHitInside = true;
    }

    private IEnumerator CloseCD()
    {
        Vector2 playerPosition = PlayerMovement.Instance.transform.position;
        Vector2 direction = (playerPosition - (Vector2)transform.position).normalized;
        canClosing = false;
        isClosing = true;
        rb.AddForce(direction * (speed * forcePower), ForceMode2D.Impulse);
        yield return new WaitForSeconds(forceDuration);
        isClosing = false;
        speed /= 2;
        yield return new WaitForSeconds(forceCD);
        speed *= 2;
        canClosing = true;
    }
}
