using UnityEngine;
using UnityEngine.UI;

public class ExpBar : MonoBehaviour
{
    public Slider expSlider;
    public Text levelText;
    public Text expText;

    public void UpdateExpBar(float currentExp, float maxExp)
    {
        if (expSlider != null)
        {
            expSlider.maxValue = maxExp;
            expSlider.value = currentExp;
        }

        if (expText != null)
        {
            expText.text = $"{currentExp:F0}/{maxExp:F0}";
        }
    }
}