using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public float damage = 1;
    public float health = 10;
    
    private Animator animator;


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
        health -= damage;
        Debug.Log("здоровье врага: " + health);
        if (health <= 0)
        {
            animator.SetTrigger("Death");
        }
    }

    

}
