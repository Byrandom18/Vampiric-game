using UnityEngine;
using TMPro;
using System.Collections;

public class DamageText : MonoBehaviour
{
    [Header("Настройки")]
    public float lifeTime = 2f;
    public float floatSpeed = 3f;
    public float gravity = 2f;

    [Header("Случайное отклонение")]
    public float maxXOffset = 1f;
    public float maxYOffset = 0.5f;

    private TextMeshPro textMesh;
    private Vector3 currentVelocity;
    private float timer;

    void Awake()
    {
        // Получаем компонент TextMeshPro
        textMesh = GetComponent<TextMeshPro>();

        // Если компонента нет - добавляем его
        if (textMesh == null)
        {
            textMesh = gameObject.AddComponent<TextMeshPro>();
            Debug.Log("TextMeshPro component added to DamageText");
        }

        // Настраиваем базовые параметры
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.sortingOrder = 100; // Чтобы текст был поверх других объектов
    }

    public void Initialize(int damage, Color color, float size, Vector3 spawnPosition)
    {
        // Проверяем, что textMesh не null
        if (textMesh == null)
        {
            textMesh = GetComponent<TextMeshPro>();
            if (textMesh == null)
            {
                Debug.LogError("TextMeshPro component is missing!");
                return;
            }
        }
        //lifeTime = duration;
        //floatSpeed = speed;
        // Устанавливаем значение, цвет и размер
        textMesh.text = damage.ToString();
        textMesh.color = color;
        textMesh.fontSize = size;

        // Позиционируем у врага
        transform.position = spawnPosition;

        // Случайное начальное отклонение
        float randomX = Random.Range(-maxXOffset, maxXOffset);
        float randomY = Random.Range(0f, maxYOffset);
        currentVelocity = new Vector3(randomX, randomY, 0) * floatSpeed;

        timer = 0f;
        StartCoroutine(FloatAndFall());
    }

    private IEnumerator FloatAndFall()
    {
        while (timer < lifeTime)
        {
            timer += Time.deltaTime;

            // Применяем гравитацию
            currentVelocity.y -= gravity * Time.deltaTime;

            // Двигаем текст
            transform.position += currentVelocity * Time.deltaTime;

            // Плавное исчезновение
            if (textMesh != null)
            {
                float alpha = 1f - (timer / lifeTime);
                Color newColor = textMesh.color;
                newColor.a = alpha;
                textMesh.color = newColor;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}