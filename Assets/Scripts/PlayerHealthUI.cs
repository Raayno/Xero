using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public Image healthBar;
    public PlayerHealth playerHealth;

    public void UpdateHealthUI()
    {
        if (healthBar == null || playerHealth == null)
            return;

        float percent =
            (float)playerHealth.CurrentHealth / playerHealth.MaxHealth;

        healthBar.fillAmount = percent;
    }
}
