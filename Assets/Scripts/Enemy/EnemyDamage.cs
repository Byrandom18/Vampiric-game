using UnityEngine;
using System.Collections;

public class EnemyDamage : MonoBehaviour
{
    public float damage = 1;
    public float health = 10;
    public float defence = 0;


    private Animator animator;
    private float shrinkDuration = 0.5f; // Длительность сжатия

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerStats.Instance.TakeDamage(damage);
        }
    }

    public void TakeDamage(float damage)
    {
        damage -= defence;
        if (damage < 1)
            damage = 1;
        health -= damage;
        Debug.Log("здоровье врага: " + health);
        if (health <= 0)
        {
            animator.SetTrigger("Death");
            GetComponent<Collider2D>().enabled = false;
            GetComponent<Rigidbody2D>().simulated = false;
            StartCoroutine(ShrinkAndDie());
        }
    }

    private IEnumerator ShrinkAndDie()
    {
        float timer = 0f;
        Vector3 originalScale = transform.localScale;

        while (timer < shrinkDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / shrinkDuration;
            // Плавное сжатие по кривой (можно заменить на animation curve)
            transform.localScale = originalScale * (1 - progress);
            yield return null;
        }

        Destroy(gameObject);
    }

}
