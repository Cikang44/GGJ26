using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    public float titleDelay = 2f;
    public float fadeInTime = 2f;
    public float explanationDelay = 2f;
    public float mainMenuDelay = 3f;
    public float fadeOutTime = 2f;

    public TextMeshProUGUI gameOverTitle;
    public TextMeshProUGUI gameOverExplanation;
    public Image fadeOutPanel;

    void Start()
    {
        StartCoroutine(GameOverCoroutine());
    }
    IEnumerator GameOverCoroutine()
    {
        float timePassed = 0;
        yield return new WaitForSeconds(titleDelay);
        while (timePassed < fadeInTime)
        {
            yield return null;
            timePassed += Time.deltaTime;
            gameOverTitle.alpha = Mathf.Clamp01(timePassed / fadeInTime);
        }

        yield return new WaitForSeconds(explanationDelay);
        timePassed = 0;
        while (timePassed < fadeInTime)
        {
            yield return null;
            timePassed += Time.deltaTime;
            gameOverExplanation.alpha = Mathf.Clamp01(timePassed / fadeInTime);
        }

        yield return new WaitForSeconds(mainMenuDelay);
        timePassed = 0;
        while (timePassed < fadeOutTime)
        {
            yield return null;
            timePassed += Time.deltaTime;
            Color color = fadeOutPanel.color;
            color.a = Mathf.Clamp01(timePassed / fadeInTime);
            fadeOutPanel.color = color;
        }

        SceneManager.LoadScene(0); // Load main menu
    }
}
