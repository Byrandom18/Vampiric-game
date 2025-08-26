using UnityEngine;
using System.Collections;

public class DamageTextManager : MonoBehaviour
{
    [Header("Префаб текста урона")]
    public GameObject damageTextPrefab;

    [Header("Настройки по умолчанию")]
    public Color defaultColor = Color.white;
    public float defaultSize = 4f;
    public float defaultLifeTime = 2f;

    public static DamageTextManager Instance;

    

    public void CreateDamageText(int damage, Vector3 position, Color? customColor = null, float? customSize = null)
    {
        StartCoroutine(SpawnDamageText(damage, position, customColor, customSize));
    }

    private IEnumerator SpawnDamageText(int damage, Vector3 position, Color? customColor, float? customSize)
    {
        // Небольшая задержка для избежания конфликтов с уничтожением объекта
        yield return null;

        if (damageTextPrefab == null)
        {
            Debug.LogError("DamageText prefab is not assigned!");
            yield break;
        }

        GameObject textObj = Instantiate(damageTextPrefab, position, Quaternion.identity);
        DamageText damageText = textObj.GetComponent<DamageText>();

        if (damageText != null)
        {
            Color color = customColor ?? defaultColor;
            float size = customSize ?? defaultSize;

            damageText.Initialize(damage, color, size, position);
        }
    }
}