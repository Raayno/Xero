using UnityEngine;

public class EnemyVisuals : MonoBehaviour
{
    public Renderer enemyRenderer;

    public Color idleColor = Color.white;
    public Color windupColor = Color.yellow;
    public Color parryWindowColor = new Color(0.7f, 0.2f, 1f); // 🟣 purple
    public Color parrySuccessColor = Color.green;

    public void SetIdle()
    {
        enemyRenderer.material.color = idleColor;
    }

    public void SetWindup()
    {
        enemyRenderer.material.color = windupColor;
    }

    public void SetParryWindow()
    {
        enemyRenderer.material.color = parryWindowColor;
    }

    public void SetParrySuccess()
    {
        enemyRenderer.material.color = parrySuccessColor;
    }
}
