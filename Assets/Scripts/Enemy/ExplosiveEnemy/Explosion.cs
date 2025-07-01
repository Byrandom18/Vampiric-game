using UnityEngine;

public class Explosion : MonoBehaviour
{
    [Header("Settings")]
    public float maxRadius = 5f;
    public float fillTime = 0.5f; // Время заполнения
    public float damage = 10f;

    [Header("Visuals")]
    public Color outlineColor = Color.red;
    public Color fillColor = new Color(1f, 0.5f, 0f, 0.5f);
    public float outlineWidth = 0.05f; // Ширина обводки

    private float fillProgress = 0f;
    private bool damageDealt = false;
    private SpriteRenderer spriteRenderer;
    private float startTime;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        transform.localScale = Vector3.one * maxRadius * 2; // Устанавливаем максимальный размер сразу
        startTime = Time.time;
        UpdateVisuals(); // Сразу создаем обводку
    }

    private void Update()
    {
        // Заполняем круг
        if (fillProgress < 1f)
        {
            fillProgress = Mathf.Min(1f, (Time.time - startTime) / fillTime);
            UpdateVisuals();
        }
        // Когда заполнение завершено, наносим урон
        else if (!damageDealt)
        {
            DealDamage();
            damageDealt = true;
            Destroy(gameObject, 0.1f);
        }
    }

    private void UpdateVisuals()
    {
        Texture2D texture = new Texture2D(128, 128);
        float fillRadius = maxRadius * fillProgress;

        // Заполняем текстуру
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                // Нормализованные координаты от центра
                float nx = (x / (float)texture.width) * 2 - 1;
                float ny = (y / (float)texture.height) * 2 - 1;
                float distance = Mathf.Sqrt(nx * nx + ny * ny);

                if (distance <= 1f && distance > 1f - outlineWidth) // Обводка
                {
                    texture.SetPixel(x, y, outlineColor);
                }
                else if (distance <= fillProgress) // Заполнение
                {
                    texture.SetPixel(x, y, fillColor);
                }
                else // Прозрачный
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }

        texture.Apply();
        spriteRenderer.sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );
    }

    private void DealDamage()
    {
        // Находим всех игроков в радиусе взрыва
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, maxRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player") && hit.isTrigger)
            {
                if (PlayerStats.Instance != null)
                {
                    PlayerStats.Instance.TakeDamage(damage);
                }
            }
        }
    }
}
