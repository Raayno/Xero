using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GameRestartWithFade : MonoBehaviour
{
    [Header("Fade")]
    public Image fadeImage;
    public float fadeDuration = 0.6f;

    [Header("Restart")]
    public KeyCode restartKey = KeyCode.R;

    bool isRestarting;

    void Start()
    {
        // Fade IN when scene loads
        if (fadeImage)
        {
            fadeImage.color = new Color(0, 0, 0, 1);
            StartCoroutine(Fade(1f, 0f));
        }
    }

    void Update()
    {
        if (!isRestarting && Input.GetKeyDown(restartKey))
        {
            RestartGame();
        }
    }

    // Can be called by UI Button
    public void RestartGame()
    {
        if (!isRestarting)
            StartCoroutine(RestartRoutine());
    }

    IEnumerator RestartRoutine()
    {
        isRestarting = true;

        // Fade OUT
        yield return StartCoroutine(Fade(0f, 1f));

        // Reload scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        Color c = fadeImage.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / fadeDuration);
            fadeImage.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        fadeImage.color = new Color(c.r, c.g, c.b, to);
    }
}

