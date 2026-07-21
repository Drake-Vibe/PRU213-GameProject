using UnityEngine;
using UnityEngine.UI;


public class HealthBar : MonoBehaviour
{

    public Slider healthSlider;
    public Gradient gradient;
    public Image fill;
    

    public void SetMaxHealth(int health)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = health;
            healthSlider.value = health;
        }

        if (fill != null && gradient != null)
        {
            fill.color = gradient.Evaluate(1f);
        }
    }

    public void SetHealth(int health)
    {
        if (healthSlider != null)
        {
            healthSlider.value = health;
        }

        if (fill != null && gradient != null && healthSlider != null)
        {
            fill.color = gradient.Evaluate(healthSlider.normalizedValue);
        }
    }    
}
