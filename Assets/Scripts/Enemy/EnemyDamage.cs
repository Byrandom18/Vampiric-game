using UnityEngine;
using System.Collections;

public class EnemyDamage : MonoBehaviour
{
    [SerializeField] private GameObject expPrefab;

    public float damage = 1;
    public float health = 10;
    public float defence = 0;

    [Header("Настройки текста урона")]
    public Color damageColor = Color.red;
    public float textSize = 1f;
    private DamageTextManager textManager;

    private Animator animator;
    private float shrinkDuration = 0.5f; // Длительность сжатия

    private void Start()
    {
        animator = GetComponent<Animator>();
        textManager = GetComponent<DamageTextManager>();
        if (PlayerStats.Instance != null)
            UpdateStats();
    }

    private void UpdateStats()
    {
        health *= 1 + PlayerStats.Instance.difficultyMod * 0.1f;
        damage *= 1 + PlayerStats.Instance.difficultyMod * 0.1f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerStats.Instance.TakeDamage(damage);
        }
    }

    public void TakeDamage(float damage, float defShred)
    {
        defence -= defShred;
        if (defence < 0)
            defence = 0;
        float critCheck = Random.Range(0f, 100f);
        float currentTextSize = textSize;
        if (critCheck <= PlayerStats.Instance.critRate)
        {
            damage *= 1 + PlayerStats.Instance.critDamage / 100;
            currentTextSize *= 1.5f;
        }

        damage -= defence;
        if (damage < 1)
            damage = 1;
        health -= damage;
        Debug.Log("здоровье врага: " + health);

        int damageText = Mathf.RoundToInt(damage);
        Vector3 spawnPos = transform.position + Vector3.up * 0.5f; // Чуть выше врага
        textManager.CreateDamageText(damageText, spawnPos, damageColor, currentTextSize);

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
        ItemCreate();
        Destroy(gameObject);
    }

    private void ItemCreate()
    {
        if (expPrefab != null)
        {
            GameObject exp = Instantiate(expPrefab, transform.position, Quaternion.identity);
            CollectibleItem expScript = exp.GetComponent<CollectibleItem>();
            if (expScript != null)
            {
                expScript.value = 1 * (1 + PlayerStats.Instance.difficultyMod * 0.05f);
            }
        }
        else
            Debug.Log("Exp Prefab doesn't exists" + this);
    }
}
